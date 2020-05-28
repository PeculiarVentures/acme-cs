using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVentures.ACME.Server.Services
{
    /// <summary>
    /// Converter service
    /// </summary>
    public class ConverterService : BaseService, IConverterService
    {
        public ConverterService(
            IAuthorizationRepository authorizationRepository,
            IChallengeRepository challengeRepository,
            IOrderAuthorizationRepository orderAuthorizationRepository,
            IExternalAccountRepository externalAccountRepository,
            IOptions<ServerOptions> options)
            : base(options)
        {
            AuthorizationRepository = authorizationRepository
                ?? throw new ArgumentNullException(nameof(authorizationRepository));
            ChallengeRepository = challengeRepository
                ?? throw new ArgumentNullException(nameof(challengeRepository));
            OrderAuthorizationRepository = orderAuthorizationRepository
                ?? throw new ArgumentNullException(nameof(orderAuthorizationRepository));
            ExternalAccountRepository = externalAccountRepository
                ?? throw new ArgumentNullException(nameof(externalAccountRepository));
        }

        private IExternalAccountRepository ExternalAccountRepository { get; }
        private IAuthorizationRepository AuthorizationRepository { get; }
        private IChallengeRepository ChallengeRepository { get; }
        private IOrderAuthorizationRepository OrderAuthorizationRepository { get; }

        #region Account
        /// <inheritdoc/>
        public Account ToAccount(IAccount data)
        {
            var result = CreateInstance<Account>();
            return OnToAccountConvert(result, data);
        }

        /// <summary>
        /// Assign values from <see cref="IAccount"/> to JSON <see cref="Account"/>.
        /// For expended objects need add assign values
        /// </summary>
        /// <param name="account">JSON <see cref="Account"/></param>
        /// <param name="data"><see cref="IAccount"/></param>
        protected virtual Account OnToAccountConvert(Account account, IAccount data)
        {
            account.Contacts = data.Contacts?.ToArray();
            account.Status = data.Status;
            account.TermsOfServiceAgreed = data.TermsOfServiceAgreed;
            account.Key = data.Key;
            account.CreatedAt = data.CreatedAt;
            account.Orders = new Uri(new Uri(Options.BaseAddress), "orders").ToString();
            if (data.ExternalAccountId != null)
            {
                account.ExternalAccountBinding = ExternalAccountRepository.GetById(data.ExternalAccountId.Value).Account;
            }
            return account;
        }
        #endregion

        #region Challenge
        /// <inheritdoc/>
        public Challenge ToChallenge(IChallenge data)
        {
            var result = CreateInstance<Challenge>();
            return OnToChallengeConvert(result, data);
        }

        /// <summary>
        /// Assign values from <see cref="IChallenge"/> to JSON <see cref="Challenge"/>.
        /// For expended objects need add assign values  
        /// </summary>
        /// <param name="chall">JSON <see cref="Challenge"/></param>
        /// <param name="data"><see cref="IChallenge"/></param>
        protected virtual Challenge OnToChallengeConvert(Challenge chall, IChallenge data)
        {
            chall.Status = data.Status;
            chall.Type = data.Type;
            chall.Validated = data.Validated;
            chall.Error = data.Error != null ? ToError(data.Error) : null;
            chall.Token = data.Token;
            chall.Url = $"{Options.BaseAddress}challenge/{data.Id}";
            return chall;
        }
        #endregion

        #region Error
        /// <inheritdoc/>
        public Error ToError(IError data)
        {
            var result = CreateInstance<Error>();
            return OnToErrorConvert(result, data);
        }

        /// <summary>
        /// Assign values from <see cref="IError"/> to JSON <see cref="Error"/>.
        /// For expended objects need add assign values   
        /// </summary>
        /// <param name="err">JSON <see cref="Error"/></param>
        /// <param name="data"><see cref="IError"/></param>
        protected virtual Error OnToErrorConvert(Error err, IError data)
        {
            err.Detail = data.Detail;
            err.Type = data.Type;
            err.SubProblems = data.SubProblems.Select(o => ToError(o)).ToArray();
            return err;
        }
        #endregion

        #region Authorization
        /// <inheritdoc/>
        public Authorization ToAuthorization(IAuthorization data)
        {
            var challenges = ChallengeRepository.GetByAuthorization(data.Id);
            var result = CreateInstance<Authorization>();
            return OnToAuthorizationConvert(result, data, challenges);
        }

        /// <summary>
        /// Assign values from <see cref="IAuthorization"/> to JSON <see cref="Authorization"/>.
        /// For expended objects need add assign values  
        /// </summary>
        /// <param name="auth">JSON <see cref="Authorization"/></param>
        /// <param name="data"><see cref="IAuthorization"/></param>
        /// <param name="chall">Array of <see cref="IChallenge"/></param>
        protected virtual Authorization OnToAuthorizationConvert(Authorization auth, IAuthorization data, IChallenge[] chall)
        {
            auth.Expires = data.Expires;
            auth.Identifier = new Identifier
            {
                Type = data.Identifier.Type,
                Value = data.Identifier.Value,
            };
            auth.Status = data.Status;
            auth.Wildcard = data.Wildcard;
            auth.Challenges = chall.Select(o =>
                ToChallenge(o))
                .ToList();
            return auth;
        }
        #endregion

        #region Order
        /// <inheritdoc/>
        public Order ToOrder(IOrder data)
        {
            var result = CreateInstance<Order>();
            return OnToOrderConvert(result, data);
        }

        /// <inheritdoc/>
        public Protocol.OrderList ToOrderList(IOrder[] orders)
        {
            var orderList = CreateInstance<Protocol.OrderList>();
            return OnToOrderListConvert(orderList, orders);
        }

        /// <summary>
        /// Converts array of <see cref="IOrder"/> to JSON <see cref="Protocol.OrderList"/>
        /// For expended objects need add assign values
        /// </summary>
        /// <param name="orderList">JSON <see cref="Protocol.OrderList"/></param>
        /// <param name="orders">Array of <see cref="IOrder"/></param>
        protected virtual Protocol.OrderList OnToOrderListConvert(Protocol.OrderList orderList, IOrder[] orders)
        {
            var ordersId = orders.Select(o => $"{Options.BaseAddress}order/{o.Id}");
            orderList.Orders = ordersId.ToArray();
            return orderList;
        }

        /// <summary>
        /// Assign values from <see cref="IOrder"/> to JSON <see cref="Order"/>.
        /// For expended objects need add assign values   
        /// </summary>
        /// <param name="order">JSON <see cref="Order"/></param>
        /// <param name="data"><see cref="IOrder"/></param>
        protected virtual Order OnToOrderConvert(Order order, IOrder data)
        {
            var authzs = OrderAuthorizationRepository.GetByOrder(data.Id)
                .Select(o => AuthorizationRepository.GetById(o.AuthorizationId))
                .ToArray();

            order.Identifiers = authzs.Select(o =>
                    new Identifier(o.Identifier.Type, o.Identifier.Value)).ToArray();
            order.Authorizations = authzs.Select(o => $"{Options.BaseAddress}authz/{o.Id}").ToArray();
            order.Status = data.Status;
            order.NotBefore = data.NotBefore;
            order.NotAfter = data.NotAfter;
            order.Expires = data.Expires;
            order.Error = data.Error == null ? null : ToError(data.Error);
            order.Finalize = $"{Options.BaseAddress}finalize/{data.Id}";
            order.Certificate = data.Certificate?.RawData == null ? null : $"{Options.BaseAddress}cert/{data.Certificate.Thumbprint}";
            return order;
        }
        #endregion

        #region TypeConvert
        private T CreateInstance<T>()
            where T : class, new()
        {
            var type = GetType<T>();
            var obj = (T)Activator.CreateInstance(type);
            return obj;
        }

        /// <summary>
        /// Dictionary extended types
        /// </summary>
        protected Dictionary<Type, Type> Types { get; } = new Dictionary<Type, Type>();

        /// <inheritdoc/>
        public Type GetType(Type type)
        {
            if (Types.TryGetValue(type, out Type result))
            {
                return result;
            }
            return type;
        }

        /// <inheritdoc/>
        public Type GetType<T>()
            where T: class, new()
        {
            return GetType(typeof(T));
        }
        #endregion
    }
}
