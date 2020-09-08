using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Cryptography;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    /// <summary>
    /// Order service
    /// </summary>
    public class OrderService : BaseService, IOrderService
    {
        private const string IDENTIFIER_HASH = "SHA256";

        public OrderService(
            IOrderRepository orderRepository,
            IAccountService accountService,
            IAccountSecurityService accountSecurityService,
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
            AccountSecurityService = accountSecurityService
                ?? throw new ArgumentNullException(nameof(accountSecurityService));
            AuthorizationService = authorizationService
                ?? throw new ArgumentNullException(nameof(authorizationService));
            OrderAuthorizationRepository = orderAuthorizationRepository
                ?? throw new ArgumentNullException(nameof(orderAuthorizationRepository));
            CertificateEnrollmentService = certificateEnrollmentService
                ?? throw new ArgumentNullException(nameof(certificateEnrollmentService));
        }

        public IOrderRepository OrderRepository { get; }
        public IAccountService AccountService { get; }
        public IAccountSecurityService AccountSecurityService { get; }
        public IAuthorizationService AuthorizationService { get; }
        public IOrderAuthorizationRepository OrderAuthorizationRepository { get; }
        public ICertificateEnrollmentService CertificateEnrollmentService { get; }
        public ICertificateEnrollmentService GetCertificateEnrollmentService { get; }

        /// <summary>
        /// Returns hash
        /// </summary>
        /// <param name="identifiers"></param>
        /// <param name="hashAlgorithm"></param>
        /// <returns></returns>
        protected string ComputerIdentifier(Identifier[] identifiers, string hashAlgorithm)
        {
            var strIdentifiers = identifiers
                .Select(o => $"{o.Type}:{o.Value}".ToLower())
                .OrderBy(o => o)
                .ToArray();
            var str = string.Join(";", strIdentifiers);
            var hash = HashAlgorithm.Create(hashAlgorithm).ComputeHash(Encoding.UTF8.GetBytes(str));
            return Base64Url.Encode(hash);
        }

        /// <inheritdoc/>
        public IOrder Create(int accountId, NewOrder @params)
        {
            #region Check arguments
            if (@params is null)
            {
                throw new ArgumentNullException(nameof(@params));
            }
            #endregion

            // create order
            var order = OrderRepository.Create();

            // fill params
            OnCreateParams(order, @params, accountId);

            // save order
            order = OrderRepository.Add(order);

            // create authorization
            OnCreateAuth(order, @params);
            order.Identifier = ComputerIdentifier(@params.Identifiers.ToArray(), IDENTIFIER_HASH);

            // update order
            order = OrderRepository.Update(order);

            Logger.Info("Order {id} created", order.Id);

            return order;
        }

        /// <summary>
        /// Fills parameters to create <see cref="IOrder"/>
        /// For expended objects need add assign values
        /// </summary>
        /// <param name="order"><see cref="IOrder"/></param>
        /// <param name="params">Params to create</param>
        /// <param name="accountId"><see cref="IAccount"/> identifier</param>
        /// <returns></returns>
        protected virtual void OnCreateParams(IOrder order, NewOrder @params, int accountId)
        {
            order.AccountId = accountId;
            order.NotBefore = @params.NotBefore;
            order.NotAfter = @params.NotAfter;
        }

        /// <summary>
        /// Finds and creats authorizations for order
        /// For expended objects need add assign values
        /// </summary>
        /// <param name="order"><see cref="IOrder"/></param>
        /// <param name="params">Params to create</param>
        protected virtual void OnCreateAuth(IOrder order, NewOrder @params)
        {
            var listDate = new List<DateTime>();
            foreach (var identifier in @params.Identifiers)
            {
                // get actual or create new authorization
                var authz = AuthorizationService.GetActual(order.AccountId, identifier)
                    ?? AuthorizationService.Create(order.AccountId, identifier);

                // create order authorization
                var orderAuthz = OrderAuthorizationRepository.Create(order.Id, authz.Id);
                OrderAuthorizationRepository.Add(orderAuthz);

                // check expires
                if (authz.Expires != null)
                {
                    listDate.Add(authz.Expires.Value);
                }
            }
            // set min expiration date from authorizations
            order.Expires = listDate.Min(o => o);
        }

        /// <summary>
        /// Allows add additional task before enroll certificate
        /// </summary>
        /// <param name="certificateEnrollParams"><see cref="CertificateEnrollParams"/></param>
        /// <returns></returns>
        protected virtual CertificateEnrollParams OnEnrollCertificateBefore(CertificateEnrollParams certificateEnrollParams)
        {
            return certificateEnrollParams;
        }

        /// <summary>
        /// Allows add additional enroll certificate task
        /// </summary>
        /// <param name="certificateEnrollParams"><see cref="CertificateEnrollParams"/></param>
        protected virtual void OnEnrollCertificateTask(CertificateEnrollParams certificateEnrollParams) { }

        /// <summary>
        /// Returns <see cref="IOrder"/> from repository.
        /// Allows to run additional arguments validations before GetActualCheck calling 
        /// </summary>
        /// <param name="params">Params to get <see cref="IOrder"/></param>
        /// <param name="accountId"><see cref="IAccount"/> specific id</param>
        /// <returns></returns>
        protected virtual IOrder OnGetActualCheckBefore(NewOrder @params, int accountId)
        {
            // Gets order from repository
            return LastByIdentifiers(accountId, @params.Identifiers.ToArray());
        }

        /// <inheritdoc/>
        public IOrder EnrollCertificate(int accountId, int orderId, FinalizeOrder @params)
        {
            #region Check arguments
            if (@params is null)
            {
                throw new ArgumentNullException(nameof(@params));
            }
            #endregion

            var order = GetById(accountId, orderId);

            // Check status ready
            if (order.Status != OrderStatus.Ready)
            {
                throw new AcmeException(ErrorType.OrderNotReady);
            }

            var certificateEnrollParams = new CertificateEnrollParams()
            {
                Order = order,
                Params = @params,
            };

            try
            {
                OnEnrollCertificateBefore(certificateEnrollParams);
            }
            catch (Exception ex)
            {
                // return invalid order
                CreateOrderError(ex, certificateEnrollParams.Order);
                return certificateEnrollParams.Order;
            }

            order.Status = OrderStatus.Processing;
            OrderRepository.Update(order);
            Logger.Info("Order {id} status updated to {status}", order.Id, order.Status);

            // check cancel
            if (!certificateEnrollParams.Cancel)
            {

                Task
                    .Run(async () =>
                    {
                        var requestRaw = Base64Url.Decode(@params.Csr);
                        var request = new Pkcs10CertificateRequest(requestRaw);

                        var certificate = await CertificateEnrollmentService.Enroll(order, request); // todo ? using certEnrollParams
                        order.Certificate = OrderRepository.CreateCertificate(certificate);
                        OrderRepository.Update(order);

                        OnEnrollCertificateTask(certificateEnrollParams);

                    })
                    .ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            // TODO Optimize Error assignment
                            CreateOrderError(t.Exception.InnerException, order);
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
            }

            return order;
        }

        /// <summary>
        /// Assign values from <see cref="Exception"/> to <see cref="IOrder.Error"/>.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="order"></param>
        private void CreateOrderError(Exception ex, IOrder order)
        {
            Error err = ex;
            order.Error = OrderRepository.CreateError();
            order.Error.Detail = err.Detail;
            order.Error.Type = err.Type;
            order.Status = OrderStatus.Invalid;
            OrderRepository.Update(order);
        }

        /// <inheritdoc/>
        public IOrder GetById(int accountId, int id)
        {
            var order = OrderRepository.GetById(id);
            if (order == null)
            {
                throw new MalformedException("Order not found");
            }

            // check access
            AccountSecurityService.CheckAccess(new AccountAccess
            {
                Account = AccountService.GetById(accountId),
                Target = order,
            });

            RefreshStatus(order);

            return order;
        }

        /// <summary>
        /// Returns last <see cref="IOrder"/>
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> specific id</param>
        /// <param name="identifiers">Array of identifiers</param>
        /// <returns></returns>
        public IOrder LastByIdentifiers(int accountId, Identifier[] identifiers)
        {
            var identifier = ComputerIdentifier(identifiers, IDENTIFIER_HASH);
            var order = OrderRepository.LastByIdentifier(accountId, identifier);
            RefreshStatus(order);
            return order;
        }

        /// <inheritdoc/>
        public IOrder GetActual(int accountId, NewOrder @params)
        {
            #region Check arguments
            if (@params == null)
            {
                throw new ArgumentNullException(nameof(@params));
            }
            #endregion

            IOrder order = OnGetActualCheckBefore(@params, accountId);
            RefreshStatus(order);

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

        protected void RefreshStatus(IOrder order)
        {
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
                        .Select(o => AuthorizationService.GetById(order.AccountId, o.AuthorizationId))
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
        }

        /// <summary>
        /// Returns <see cref="IOrder"/> by <see cref="X509Certificate2"/>
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> specific id</param>
        /// <param name="x509"><see cref="X509Certificate2"/></param>
        public IOrder GetByCertificate(int accountId, X509Certificate2 x509)
        {
            var cert = OrderRepository.CreateCertificate(x509);
            return GetByCertificate(accountId, cert.Thumbprint);
        }

        /// <summary>
        /// Returns <see cref="IOrder"/> by thumbprint of certificate
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> specific id</param>
        /// <param name="thumbprint">Thumbprint of certificate</param>
        /// <exception cref="MalformedException"/>
        public IOrder GetByCertificate(int accountId, string thumbprint)
        {
            // get order
            var order = OrderRepository.GetByThumbprint(thumbprint);
            if (order == null)
            {
                throw new MalformedException("Order not found");
            }
            RefreshStatus(order);

            // check access
            AccountSecurityService.CheckAccess(new AccountAccess
            {
                Account = AccountService.GetById(accountId),
                Target = order,
            });
            return order;
        }

        /// <inheritdoc/>
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
            chain.Build(new X509Certificate2(order.Certificate.RawData));

            // create array from chain
            var res = new List<ICertificate>();
            foreach (var item in chain.ChainElements)
            {
                res.Add(OrderRepository.CreateCertificate(item.Certificate));
            }
            return res.ToArray();
        }

        /// <inheritdoc/>
        public void RevokeCertificate(int accountId, RevokeCertificate @params)
        {
            // find order
            var x509 = new X509Certificate2(Base64Url.Decode(@params.Certificate));
            var order = GetByCertificate(accountId, x509);

            // revoke
            RevokeCertificate(order, @params.Reason);
        }

        /// <inheritdoc/>
        public void RevokeCertificate(JsonWebKey key, RevokeCertificate @params)
        {
            var x509 = new X509Certificate2(Base64Url.Decode(@params.Certificate));
            throw new NotImplementedException($"Not implemented method {nameof(RevokeCertificate)}");

            // todo see https://tools.ietf.org/html/rfc8555#section-7.6
            //    The server MUST also consider a revocation request valid if it is
            //       signed with the private key corresponding to the public key in the
            //       certificate.

            //var order = GetByCertificate(accountId, x509);

            //RevokeCertificate(order, @params.Reason);
        }

        /// <summary>
        /// Revokes <see cref="ICertificate"/>
        /// </summary>
        /// <param name="order"><see cref="IOrder"/></param>
        /// <param name="reason">Revoke reason</param>
        public void RevokeCertificate(IOrder order, RevokeReason reason)
        {
            // revoke
            Task.Run(async () => await CertificateEnrollmentService.Revoke(order, reason))
                .Wait();

            // update status
            order.Certificate.Revoked = true;
            OrderRepository.Update(order);

            Logger.Info("Certificate {thumbprint} revoked", order.Certificate.Thumbprint);
        }

        /// <inheritdoc/>
        public IOrderList GetList(int accountId, Query @params)
        {
            var orderList = OrderRepository.GetList(accountId, @params, Options.OrdersPageSize);
            return orderList;
        }
    }
}
