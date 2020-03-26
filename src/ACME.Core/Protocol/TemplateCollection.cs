using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Protocol
{
    public class TemplateCollection
    {
        [JsonProperty("templates")]
        public List<Template> Templates { get; set; } = new List<Template>();

        [JsonProperty("cas")]
        public List<CertificateAuthority> CAs { get; set; } = new List<CertificateAuthority>();
    }
}
