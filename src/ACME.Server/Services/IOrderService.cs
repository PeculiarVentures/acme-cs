using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IOrderService
    {
        IOrder Create(int accountId, NewOrder @params);
        IOrder GetById(int accountId, int id);
        IOrder GetActual(int accountId, NewOrder @params);
        IOrder EnrollCertificate(int accountId, int orderId, FinalizeOrder @params);
        ICertificate[] GetCertificate(int accountId, string thumbprint);
        void RevokeCertificate(int accountId, RevokeCertificate @params);
        void RevokeCertificate(JsonWebKey key, RevokeCertificate @params);
    }
}
