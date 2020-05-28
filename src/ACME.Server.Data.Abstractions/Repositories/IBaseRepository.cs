using System;
using System.Collections.Generic;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Repositories
{
    public interface IBaseRepository<T>
        where T : IBaseObject
    {
        //IEnumerable<T> Items { get; }

        /// <summary>
        /// Returns the item by identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        T GetById(int id);

        /// <summary>
        /// Adds the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        T Add(T item);

        /// <summary>
        /// Updates the item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        T Update(T item);

        /// <summary>
        /// Removes the item
        /// </summary>
        /// <param name="item"></param>
        void Remove(T item);

        /// <summary>
        /// Creates and returns the new item
        /// </summary>
        /// <returns></returns>
        T Create();

        /// <summary>
        /// Creates the error
        /// </summary>
        /// <returns></returns>
        IError CreateError();
    }
}
