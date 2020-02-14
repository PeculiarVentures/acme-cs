using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Repositories
{
    public class DefaultExternalAccountRepository : IExternalAccountRepository
    {
        public IExternalAccount Add(IExternalAccount item)
        {
            throw new NotImplementedException();
        }

        public IExternalAccount Create()
        {
            throw new NotImplementedException();
        }

        public IError CreateError()
        {
            throw new NotImplementedException();
        }

        public IExternalAccount GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Remove(IExternalAccount item)
        {
            throw new NotImplementedException();
        }

        public IExternalAccount Update(IExternalAccount item)
        {
            throw new NotImplementedException();
        }
    }
}
