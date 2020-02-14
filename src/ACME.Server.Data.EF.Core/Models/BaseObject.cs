using System.ComponentModel.DataAnnotations;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.EF.Core.Models
{
    public abstract class BaseObject : IBaseObject
    {
        [Key]
        public int Id { get; set; }
    }
}
