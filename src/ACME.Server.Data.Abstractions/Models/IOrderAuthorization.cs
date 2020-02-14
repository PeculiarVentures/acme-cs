using System;
using System.Collections.Generic;
using System.Text;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IOrderAuthorization : IBaseObject
    {
        int AuthorizationId { get; set; }
        int OrderId { get; set; }
    }
}
