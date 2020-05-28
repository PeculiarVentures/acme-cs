using System;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IOrderService
    {
        /// <summary>
        /// Creats new <see cref="IOrder"/>
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="params"></param>
        /// <exception cref="ArgumentNullException"/>
        IOrder Create(int accountId, NewOrder @params);

        /// <summary>
        /// Returns <see cref="IOrderList"/> by <see cref="IAccount"/> specific id
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> specific id</param>
        /// <param name="query">Query params</param>
        IOrderList GetList(int accountId, Query query);

        /// <summary>
        /// Returns <see cref="IOrder"/> by specified Id
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> identifier</param>
        /// <param name="id"><see cref="IOrder"/> identifier</param>
        /// <exception cref="MalformedException"/>
        IOrder GetById(int accountId, int id);

        /// <summary>
        /// Returns actual <see cref="IOrder"/>
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> identifier</param>
        /// <param name="params"></param>
        /// <returns></returns>
        IOrder GetActual(int accountId, NewOrder @params);

        /// <summary>
        /// Enrolls certificate
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> specific id</param>
        /// <param name="orderId"><see cref="IOrder"/> specific id</param>
        /// <param name="params">Params to finalize order</param>
        /// <returns></returns>
        IOrder EnrollCertificate(int accountId, int orderId, FinalizeOrder @params);

        /// <summary>
        /// Returns chain for <see cref="ICertificate"/>
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> specific id</param>
        /// <param name="thumbprint">Thumbprint of <see cref="ICertificate"/></param>
        ICertificate[] GetCertificate(int accountId, string thumbprint);

        /// <summary>
        /// Revokes <see cref="ICertificate"/>
        /// </summary>
        /// <param name="accountId"><see cref="IAccount"/> identifier</param>
        /// <param name="params">Params to revoke certificate</param>
        void RevokeCertificate(int accountId, RevokeCertificate @params);

        /// <summary>
        /// Revokes <see cref="ICertificate"/> 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="params"></param>
        /// <exception cref="NotImplementedException"/>
        void RevokeCertificate(JsonWebKey key, RevokeCertificate @params);
    }
}
