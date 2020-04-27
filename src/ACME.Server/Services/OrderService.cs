using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Cryptography;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public class OrderService : BaseService, IOrderService
    {
        private const string IDENTIFIER_HASH = "SHA256";

        public OrderService(
            IOrderRepository orderRepository,
            IAccountService accountService,
            IAuthorizationService authorizationService,
            IOrderAuthorizationRepository orderAuthorizationRepository,
            ICertificateEnrollmentService certificateEnrollmentService,
            IOptions<ServerOptions> options)
            : base(options)
        {
            OrderRepository = orderRepository
                ?? throw new ArgumentNullException(nameof(orderRepository));
            AccountService = accountService
                ?? throw new ArgumentNullException(nameof(accountService));
            AuthorizationService = authorizationService
                ?? throw new ArgumentNullException(nameof(authorizationService));
            OrderAuthorizationRepository = orderAuthorizationRepository
                ?? throw new ArgumentNullException(nameof(orderAuthorizationRepository));
            CertificateEnrollmentService = certificateEnrollmentService
                ?? throw new ArgumentNullException(nameof(certificateEnrollmentService));
        }

        public IOrderRepository OrderRepository { get; }
        public IAccountService AccountService { get; }
        public IAuthorizationService AuthorizationService { get; }
        public IOrderAuthorizationRepository OrderAuthorizationRepository { get; }
        public ICertificateEnrollmentService CertificateEnrollmentService { get; }
        public ICertificateEnrollmentService GetCertificateEnrollmentService { get; }

        public string ComputerIdentifier(Identifier[] identifiers, string hashAlgorithm)
        {
            var strIdentifiers = identifiers
                .Select(o => $"{o.Type}:{o.Value}".ToLower())
                .OrderBy(o => o)
                .ToArray();
            var str = string.Join(";", strIdentifiers);
            var hash = HashAlgorithm.Create(hashAlgorithm).ComputeHash(Encoding.UTF8.GetBytes(str));
            return Base64Url.Encode(hash);
        }

        /// <summary>
        /// Creats new account
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="params"></param>
        /// <returns></returns>
        public IOrder Create(int accountId, NewOrder @params)
        {
            #region Check arguments
            if (@params is null)
            {
                throw new ArgumentNullException(nameof(@params));
            }
            #endregion

            var order = OrderRepository.Create();
            OnCreateParams(order, @params, accountId);
            order = OrderRepository.Add(order);
            OnCreateAuth(order, @params);
            order.Identifier = ComputerIdentifier(@params.Identifiers.ToArray(), IDENTIFIER_HASH);
            order = OrderRepository.Update(order);

            Logger.Info("Order {id} created", order.Id);

            return order;
        }

        /// <summary>
        /// Fills parameters
        /// </summary>
        /// <param name="order"></param>
        /// <param name="params"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        protected virtual void OnCreateParams(IOrder order, NewOrder @params, int accountId)
        {
            order.AccountId = accountId;
            order.NotBefore = @params.NotBefore;
            order.NotAfter = @params.NotAfter;
        }

        /// <summary>
        /// Finds and creats authorizations for order
        /// </summary>
        /// <param name="order"></param>
        /// <param name="params"></param>
        protected virtual void OnCreateAuth(IOrder order, NewOrder @params)
        {
            var listDate = new List<DateTime>();
            foreach (var identifier in @params.Identifiers)
            {
                var authz = AuthorizationService.GetActual(order.AccountId, identifier)
                    ?? AuthorizationService.Create(order.AccountId, identifier);

                var orderAuthz = OrderAuthorizationRepository.Create(order.Id, authz.Id);
                OrderAuthorizationRepository.Add(orderAuthz);

                if (authz.Expires != null)
                {
                    listDate.Add(authz.Expires.Value);
                }
            }
            // set min expiration date from authorizations
            order.Expires = listDate.Min(o => o);
        }

        protected virtual object OnEnrollCertificateBefore(IOrder order, FinalizeOrder @params)
        {
            return null;
        }

        protected virtual void OnEnrollCertificateTask(IOrder order, FinalizeOrder @params, object result) { }

        protected virtual IOrder OnGetActualCheckBefore(NewOrder @params, int accountId)
        {
            // Gets order from repository
            return LastByIdentifiers(accountId, @params.Identifiers.ToArray());
        }

        public IOrder EnrollCertificate(int accountId, int orderId, FinalizeOrder @params)
        {
            #region Check arguments
            if (@params is null)
            {
                throw new ArgumentNullException(nameof(@params));
            }
            #endregion

            var order = GetById(accountId, orderId);

            order.Status = OrderStatus.Processing;
            OrderRepository.Update(order);

            Logger.Info("Order {id} status updated to {status}", order.Id, order.Status);

            var result = OnEnrollCertificateBefore(order, @params);

            Task
                .Run(async () =>
                {
                    var requestRaw = Base64Url.Decode(@params.Csr);
                    var request = new Pkcs10CertificateRequest(requestRaw);

                    var certificate = await CertificateEnrollmentService.Enroll(order, request);
                    order.Certificate = OrderRepository.CreateCertificate(certificate);
                    OrderRepository.Update(order);

                    OnEnrollCertificateTask(order, @params, result);

                })
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        // TODO Optimize Error assignment
                        Error err = t.Exception.InnerException;
                        order.Error = OrderRepository.CreateError();
                        order.Error.Detail = err.Detail;
                        order.Error.Type = err.Type;
                        order.Status = OrderStatus.Invalid;
                        OrderRepository.Update(order);
                    }
                    if (t.IsCompleted)
                    {
                        if (order.Status == OrderStatus.Processing)
                        {
                            order.Status = OrderStatus.Valid;
                            OrderRepository.Update(order);

                            Logger.Info("Certificate {thumbprint} for Order {id} issued successfully", order.Certificate.Thumbprint, order.Id);
                        }
                    }

                    Logger.Info("Order {id} status updated to {status}", order.Id, order.Status);
                });

            return order;
        }

        private AsymmetricAlgorithm DecryptArchivedKey(string archivedKey)
        {
            var json = Base64Url.Decode(archivedKey);
            var jwk = JsonConvert.DeserializeObject<JsonWebKey>(Encoding.UTF8.GetString(json));
            return jwk.GetAsymmetricAlgorithm();
        }

        public IOrder GetById(int accountId, int id)
        {
            var order = OrderRepository.GetById(id);
            if (order == null)
            {
                throw new MalformedException("Order not found");
            }
            if (order.AccountId != accountId)
            {
                throw new MalformedException("Access denied");
            }
            return order;
        }

        public IOrder LastByIdentifiers(int accountId, Identifier[] identifiers)
        {
            var identifier = ComputerIdentifier(identifiers, IDENTIFIER_HASH);
            var order = OrderRepository.LastByIdentifier(accountId, identifier);
            return order;
        }

        public IOrder GetActual(int accountId, NewOrder @params)
        {
            #region Check arguments
            if (@params == null)
            {
                throw new ArgumentNullException(nameof(@params));
            }
            #endregion

            IOrder order = OnGetActualCheckBefore(@params, accountId);

            if (!(order == null || order.Status == OrderStatus.Invalid))
            {
                // Checks expires
                if (order.Expires != null && order.Expires < DateTime.UtcNow)
                {
                    order.Status = OrderStatus.Invalid;
                }
                else
                {
                    // RefreshStatus authorizations
                    var authorizations = OrderAuthorizationRepository.GetByOrder(order.Id)
                        .Select(o => AuthorizationService.GetById(accountId, o.AuthorizationId))
                        .ToArray();

                    if (order.Status == OrderStatus.Pending)
                    {
                        // Check Authz statuses
                        if (!authorizations.All(o => o.Status == AuthorizationStatus.Pending
                            || o.Status == AuthorizationStatus.Valid))
                        {
                            order.Status = OrderStatus.Invalid;
                        }
                        else if (authorizations.All(o => o.Status == AuthorizationStatus.Valid))
                        {
                            order.Status = OrderStatus.Ready;
                        }
                    }
                }

                // Update repository
                OrderRepository.Update(order);

                Logger.Info("Order {id} status updated to {status}", order.Id, order.Status);
            }

            if (order != null
                && (order.Status == OrderStatus.Pending
                || order.Status == OrderStatus.Ready
                || order.Status == OrderStatus.Processing))
            {
                return order;
            }
            else
            {
                return null;
            }
        }

        public IOrder GetByCertificate(int accountId, X509Certificate2 x509)
        {
            var cert = OrderRepository.CreateCertificate(x509);
            return GetByCertificate(accountId, cert.Thumbprint);
        }

        public IOrder GetByCertificate(int accountId, string thumbprint)
        {
            var order = OrderRepository.GetByThumbprint(thumbprint);
            if (order == null)
            {
                throw new MalformedException("Order not found");
            }
            if (order.AccountId != accountId)
            {
                throw new MalformedException("Access denied"); // TODO Check RFC Error
            }
            return order;
        }

        public ICertificate[] GetCertificate(int accountId, string thumbprint)
        {
            var order = GetByCertificate(accountId, thumbprint);

            // Build chain
            var chain = new X509Chain();
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            foreach (var item in Options.ExtraCertificateStorage)
            {
                chain.ChainPolicy.ExtraStore.Add(item);
            }
            if (!chain.Build(new X509Certificate2(order.Certificate.RawData)))
            {
            }

            var res = new List<ICertificate>();
            foreach (var item in chain.ChainElements)
            {
                res.Add(OrderRepository.CreateCertificate(item.Certificate));
            }
            return res.ToArray();
        }

        public void RevokeCertificate(int accountId, RevokeCertificate @params)
        {
            var x509 = new X509Certificate2(Base64Url.Decode(@params.Certificate));
            var order = GetByCertificate(accountId, x509);

            RevokeCertificate(order, @params.Reason);
        }

        public void RevokeCertificate(JsonWebKey key, RevokeCertificate @params)
        {
            var x509 = new X509Certificate2(Base64Url.Decode(@params.Certificate));
            throw new NotImplementedException($"Not implemented method {nameof(RevokeCertificate)}");
            //var order = GetByCertificate(accountId, x509);

            //RevokeCertificate(order, @params.Reason);
        }

        public void RevokeCertificate(IOrder order, RevokeReason reason)
        {
            Task.Run(async () => await CertificateEnrollmentService.Revoke(order, reason))
                .Wait();

            order.Certificate.Revoked = true;
            OrderRepository.Update(order);

            Logger.Info("Certificate {thumbprint} revoked", order.Certificate.Thumbprint);
        }

        public IOrderList GetList(int accountId, Query @params)
        {
            var orderList = OrderRepository.GetList(accountId, @params, Options.OrdersPageSize);
            return orderList;
        }
    }
}
