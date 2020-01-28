using System;
using System.Collections.Generic;
using System.Linq;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;
using PeculiarVentures.ACME.Server.Data.Abstractions.Repositories;

namespace PeculiarVenturs.ACME.Server.Data.Memory.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T>
        where T : IBaseObject
    {
        private int _lastId = 0;
        private List<T> _items = new List<T>();

        public IEnumerable<T> Items => _items;

        public T Add(T item)
        {
            _items.Add(item);
            item.Id = ++_lastId;

            return item;
        }

        public T GetById(int id)
        {
            return Items.FirstOrDefault(o => o.Id == id);
        }

        public void Remove(T item)
        {
            _items.Remove(item);
        }

        public T Update(T item)
        {
            return item;
        }
    }
}
