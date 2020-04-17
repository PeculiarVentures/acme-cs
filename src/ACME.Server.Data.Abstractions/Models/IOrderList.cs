using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PeculiarVentures.ACME.Protocol;

namespace PeculiarVentures.ACME.Server.Data.Abstractions.Models
{
    public interface IOrderList
    {
        IOrder[] Orders { get; set; }
        bool NextPage { get; set; }
    }
}
