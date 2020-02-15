using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Web;

namespace PeculiarVentures.ACME.Server.Services
{
    public interface IExternalAccountService
    {
        IExternalAccount Create(object data);
        IExternalAccount GetById(int id);
        IExternalAccount GetById(string kid);
        IExternalAccount Validate(JsonWebKey accountKey, JsonWebSignature token);
    }
}
