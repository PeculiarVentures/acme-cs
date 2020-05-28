using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IConverterService
    {
        /// <summary>
        /// Connverts <see cref="IChallenge"/> to JSON <see cref="Challenge"/>
        /// </summary>
        /// <param name="data"></param>
        Challenge ToChallenge(IChallenge data);

        /// <summary>
        /// Connverts <see cref="IError"/> to JSON <see cref="Error"/>
        /// </summary>
        /// <param name="data"><see cref="IError"/></param>
        Error ToError(IError data);

        /// <summary>
        /// Connverts <see cref="IAccount"/> to JSON <see cref="Account"/>
        /// </summary>
        /// <param name="account"><see cref="IAccount"/></param>
        Account ToAccount(IAccount account);

        /// <summary>
        /// Connverts <see cref="IAuthorization"/> to JSON <see cref="Authorization"/>
        /// </summary>
        /// <param name="data"><see cref="IAuthorization"/></param>
        Authorization ToAuthorization(IAuthorization data);

        /// <summary>
        /// Connverts <see cref="IOrder"/> to JSON <see cref="Order"/>
        /// </summary>
        /// <param name="data"><see cref="IOrder"/></param>
        Order ToOrder(IOrder data);

        /// <summary>
        /// Converts array of <see cref="IOrder"/> to JSON <see cref="Protocol.OrderList"/>
        /// </summary>
        /// <param name="orders">Array of <see cref="IOrder"/></param>
        Protocol.OrderList ToOrderList(IOrder[] orders);

        /// <summary>
        /// Returns type from Types by key
        /// </summary>
        /// <param name="type"></param>
        Type GetType(Type type);

        /// <summary>
        /// Returns type from Types by key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        Type GetType<T>() where T: class, new();
    }
}
