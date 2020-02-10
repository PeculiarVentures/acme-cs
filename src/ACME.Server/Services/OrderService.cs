using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Cryptography;
using PeculiarVentures.ACME.Helpers;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Services
{
    public class OrderService : IOrderService
    {
        public OrderService(
            IOrderRepository orderRepository, IAuthorizationService authorizationService,
            IOptions<ServerOptions> options, ICertificateRepository certificateRepository)
        {
            OrderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            AuthorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            CertificateRepository = certificateRepository ?? throw new ArgumentNullException(nameof(certificateRepository));
        }

        public IOrderRepository OrderRepository { get; }
        public IAuthorizationService AuthorizationService { get; }
        public IOptions<ServerOptions> Options { get; }
        public ICertificateRepository CertificateRepository { get; }

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

            foreach (var identifier in @params.Identifiers)
            {
                var authz = AuthorizationService.GetActual(accountId, identifier)
                    ?? AuthorizationService.Create(accountId, identifier);

                order.Authorizations.Add(authz);
            }

            return OrderRepository.Add(order);
        }

        public IOrder EnrollCertificate(int accountId, int orderId, FinalizeOrder @params)
        {
            #region Check arguments
            if (@params is null)
            {
                throw new ArgumentNullException(nameof(@params));
            }
            #endregion

            var order = GetById(accountId, orderId)
                ?? throw new MalformedException("Access denied");

            order.Status = OrderStatus.Processing;
            OrderRepository.Update(order);

            Task
                .Run(async () =>
                {
                    var requestRaw = Base64Url.Decode(@params.Csr);
                    var request = new Pkcs10CertificateRequest(requestRaw);
                    var certificate = await Options.Value.EnrollmentHandler.Enroll(order, request);

                    // Save cert
                    var cert = CertificateRepository.Create(certificate);
                    CertificateRepository.Add(cert);

                    // Assign cert to order
                    order.Certificate = cert;
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
            var order = OrderRepository.GetById(id);
            if (order.AccountId == accountId)
            {
                return order;
            }
            return null;
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
            var order = OrderRepository.GetByIdentifiers(accountId, identifiers);
            if (order == null || order.Status == OrderStatus.Invalid)
            {
                return null;
            }

            // Checks expires
            if (order.Expires != null && order.Expires < DateTime.Now)
            {
                order.Status = OrderStatus.Invalid;
            }
            else
            {
                // RefreshStatus authorizations
                var authorizations = order.Authorizations;
                authorizations.Select(auth => AuthorizationService.RefreshStatus(auth));

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

        public ICertificate[] GetCertificate(int accountId, string thumbprint)
        {
            var cert = CertificateRepository.GetByThumbprint(thumbprint);
            if (cert == null)
            {
                throw new MalformedException("Certificate not found");
            }

            // Build chain
            var chain = new X509Chain();
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
            foreach (var item in Options.Value.ExtraCertificateStorage)
            {
                chain.ChainPolicy.ExtraStore.Add(item);
            }
            if (!chain.Build(new X509Certificate2(cert.RawData)))
            {
                // TODO Write WARN that cannot build a cert chain
            }

            var res = new List<ICertificate>();
            foreach (var item in chain.ChainElements)
            {
                res.Add(CertificateRepository.Create(item.Certificate));
            }
            return res.ToArray();
        }
    }
}
