using System.Runtime.Serialization;

namespace PeculiarVentures.ACME.Protocol
{
    public enum TemplateIdentifierPresence
    {
        [EnumMember(Value = "required")]
        Required,

        [EnumMember(Value = "optional")]
        Optional,
    }
}
