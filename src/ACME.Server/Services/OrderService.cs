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
    public class OrderService : IOrderService
    {
        private const string IDENTIFIER_HASH = "SHA256";

        public OrderService(
            IOrderRepository orderRepository,
            IAuthorizationService authorizationService,
            IOrderAuthorizationRepository orderAuthorizationRepository,
            IOptions<ServerOptions> options)
        {
            OrderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            AuthorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            OrderAuthorizationRepository = orderAuthorizationRepository ?? throw new ArgumentNullException(nameof(orderAuthorizationRepository));
            Options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public IOrderRepository OrderRepository { get; }
        public IAuthorizationService AuthorizationService { get; }
        public IOrderAuthorizationRepository OrderAuthorizationRepository { get; }
        public ServerOptions Options { get; }

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

        public IOrder Create(int accountId, NewOrder @params)
        {
            #region Check arguments
            if (@params is null)
            {
                throw new ArgumentNullException(nameof(@params));
            }
            #endregion

            var order = OrderRepository.Create();

            order.AccountId = accountId;

            order = OrderRepository.Add(order);

            var listDate = new List<DateTime>();
            foreach (var identifier in @params.Identifiers)
            {
                var authz = AuthorizationService.GetActual(accountId, identifier)
                    ?? AuthorizationService.Create(accountId, identifier);

                var orderAuthz = OrderAuthorizationRepository.Create(order.Id, authz.Id);
                OrderAuthorizationRepository.Add(orderAuthz);

                if (authz.Expires != null)
                {
                    listDate.Add(authz.Expires.Value);
                }
            }
            // set min expiration date from authorizations
            order.Expires = listDate.Min(o => o);

            order.Identifier = ComputerIdentifier(@params.Identifiers.ToArray(), IDENTIFIER_HASH);
            OrderRepository.Update(order);

            return order;
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

            Task
                .Run(async () =>
                {
                    var requestRaw = Base64Url.Decode(@params.Csr);
                    var request = new Pkcs10CertificateRequest(requestRaw);
                    var certificate = await Options.EnrollmentHandler.Enroll(order, request);

                    order.Certificate = OrderRepository.CreateCertificate(certificate);
                    OrderRepository.Update(order);
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
                        }
                    }
                });

            return order;
        }

        public IOrder GetById(int accountId, int id)
        {
            var order = OrderRepository.GetById(id) ?? throw new MalformedException("Order not found");
            if (order.AccountId != accountId)
            {
                throw new MalformedException("Access denied");
            }
            return order;
        }

        public IOrder LastByIdentifiers(int accountId, Identifier[] identifiers)
        {
            var identifier = ComputerIdentifier(identifiers, IDENTIFIER_HASH);
            return OrderRepository.LastByIdentifier(accountId, identifier);
        }

        public IOrder GetActual(int accountId, Identifier[] identifiers)
        {
            #region Check arguments
            if (identifiers is null)
            {
                throw new ArgumentNullException(nameof(identifiers));
            }
            #endregion

            // Gets order from repository
            var order = LastByIdentifiers(accountId, identifiers);
            if (order == null || order.Status == OrderStatus.Invalid)
            {
                return null;
            }

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

            return order;
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
                throw new MalformedException("Certificate not found");
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
                // TODO Write WARN that cannot build a cert chain
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
            Task.Run(async () => await Options.EnrollmentHandler.Revoke(order, reason))
                .Wait();

            order.Certificate.Revoked = true;
            OrderRepository.Update(order);
        }
    }
}
