using System;
using PeculiarVentures.ACME.Protocol;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IOrderService
    {
        IOrder Create(int accountId, NewOrder @params);
        IOrder GetById(int accountId, int id);
        IOrder GetActual(int accountId, Identifier[] identifiers);
        IOrder EnrollCertificate(int accountId, int orderId, FinalizeOrder @params);
        ICertificate[] GetCertificate(int accountId, string thumbprint);
    }
}
