using System;

namespace GlobalSign.ACME.Server.Data.EF.Core.Models
{
    public class GsOrder : PeculiarVentures.ACME.Server.Data.EF.Core.Models.Order
    {
        public string TemplateId { get; set; }
    }
}
