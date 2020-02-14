using System;
using System.Collections.Generic;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IBaseRepository<T>
        where T : IBaseObject
    {
        //IEnumerable<T> Items { get; }

        T GetById(int id);

        T Add(T item);
        T Update(T item);
        void Remove(T item);
        T Create();
        IError CreateError();
    }
}
