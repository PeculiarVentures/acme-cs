using System;
using Microsoft.EntityFrameworkCore;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;
using PeculiarVentures.ACME.Server.Data.EF.Core.Models;

namespace GlobalSign.ACME.Server.Data.EF.Core.Repositories
{
    public abstract class GsBaseRepository<T, TEntity> : IBaseRepository<T>
        where T : IBaseObject
        where TEntity : class, T, new()
    {
        protected GsBaseRepository(GsAcmeContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public GsAcmeContext Context { get; }

        public abstract DbSet<TEntity> Records { get; }

        public T Add(T item)
        {
            Records.Add((TEntity)(object)item);
            Context.SaveChanges();
            return item;
        }

        public T Create()
        {
            return (T)Activator.CreateInstance(typeof(TEntity));
        }

        public IError CreateError()
        {
            return new Error();
        }

        public abstract T GetById(int id);

        public void Remove(T item)
        {
            Records.Remove((TEntity)(object)item);
            Context.SaveChanges();
        }

        public T Update(T item)
        {
            Context.SaveChanges();
            return item;
        }
    }
}
