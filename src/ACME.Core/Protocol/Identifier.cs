using System;
using Newtonsoft.Json;

namespace PeculiarVentures.ACME.Protocol
{
    /// <summary>
    /// JSON ACME Identifir object
    /// </summary>
    public class Identifier
    {
        public Identifier()
        {
        }

        public Identifier(string type, string value)
        {
            Type = type;
            Value = value;
        }

        /// <summary>
        /// The type of identifier.
        /// </summary>
        /// type (required, string)
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// The identifier itself.
        /// </summary>
        /// value (required, string)
        [JsonProperty("value")]
        public string Value { get; set; }

        public int Compare(Identifier identifier, bool ignoreCase = false)
        {
            var res = string.Compare(Type, identifier.Type, ignoreCase);
            if (res == 0)
            {
                res = string.Compare(Value, identifier.Value, ignoreCase);
            }
            return res;
        }

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