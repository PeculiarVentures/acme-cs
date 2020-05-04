using System;
using PeculiarVentures.ACME.Protocol.Messages;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server
{
    public class CertificateEnrollParams
    {
            public IOrder Order { get; set; }
            public FinalizeOrder Params { get; set; }
            public object Data { get; set; }
            public bool Cancel { get; set; }
    }

}
