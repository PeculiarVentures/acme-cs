using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Models
{
    public abstract class BaseObject : IBaseObject
    {
        public int Id { get; set; }
    }
}
