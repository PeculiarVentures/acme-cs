using System;
using PeculiarVentures.ACME.Server.Data.Abstractions.Models;

namespace PeculiarVentures.ACME.Server.Data.Memory.Models
{
    public class Identifier : IIdentifier
    {
        public Identifier()
        {
        }

        public Identifier(string type, string value)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; set; }
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Identifier id)
            {
                return id.Type.Equals(Type, StringComparison.CurrentCultureIgnoreCase)
                    && id.Value.Equals(Value, StringComparison.CurrentCultureIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Type}:{Value}";
        }
    }
}
