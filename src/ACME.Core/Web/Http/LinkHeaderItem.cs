using System;
using System.Text.RegularExpressions;

namespace PeculiarVentures.ACME.Web.Http
{
    /// <summary>
    /// Param and value for Link header.
    /// param1=value1; param2="value2"
    /// </summary>
    public class LinkHeaderItem
    {
        /// <summary>
        /// Creates a new instance of <see cref="LinkHeaderItem"/>
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        /// <param name="quoted">
        /// If value is true surrounds value by quatermarks
        /// </param>
        /// <exception cref="ArgumentNullException"/>
        public LinkHeaderItem(string name, string value, bool quoted = false)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Quoted = quoted;
        }

        /// <summary>
        /// Sets/gets parameter name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Sets/gets paramtere value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Enables/disables a quated flag
        /// </summary>
        public bool Quoted { get; set; }

        public override string ToString()
        {
            #region Check arguments
            if (string.IsNullOrEmpty(Name))
            {
                throw new ArgumentNullException(nameof(Name));
            }
            if (string.IsNullOrEmpty(Value))
            {
                throw new ArgumentNullException(nameof(Value));
            }
            #endregion

            var q = Quoted ? "\"" : "";
            return $"{Name}={q}{Value}{q}";
        }

        /// <summary>
        /// Parses Link attributes (eg rel="index")
        /// </summary>
        /// <param name="data">String data which must be parsed</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="FormatException"/>
        public static LinkHeaderItem Parse(string data)
        {
            #region Check arguments
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            #endregion

            var regex = new Regex("^ *([^ =]+) *= *\"?([^\"]+)\"? *$", RegexOptions.IgnoreCase);
            var match = regex.Match(data);
            if (!match.Success)
            {
                throw new FormatException("Link doesn't match to regular expression");
            }

            return new LinkHeaderItem(
                match.Groups[1].Value,
                match.Groups[2].Value,
                new Regex("= *\"").IsMatch(data));
        }
    }
}
