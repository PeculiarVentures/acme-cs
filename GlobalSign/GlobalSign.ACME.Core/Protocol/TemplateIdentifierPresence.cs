using System.Runtime.Serialization;

namespace GlobalSign.ACME.Protocol
{
    public enum TemplateIdentifierPresence
    {
        [EnumMember(Value = "required")]
        Required,

        [EnumMember(Value = "optional")]
        Optional,
    }
}
