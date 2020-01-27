using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace PeculiarVentures.ACME.Web.Http
{
    public class LinkHeader
    {
        internal const string HeaderName = "Link";

        public Uri Url { get; set; }

        public LinkHeaderItemCollection Items { get; } = new LinkHeaderItemCollection();

        public LinkHeader()
        {
        }

        public LinkHeader(string url)
        {
            Url = new Uri(url);
        }

        public LinkHeader(string url, LinkHeaderItem item) : this(url)
        {
            Items.Add(item);
        }

        /// <summary>
        /// Creates <see cref="LinkHeader"/> from string value/>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="FormatException"/>
        public static LinkHeader Parse(string value)
        {
            #region Check arguments
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            #endregion

            var result = new LinkHeader();

            var regex = new Regex("<(.+)> ?; ?(.+)", RegexOptions.IgnoreCase);
            var match = regex.Match(value);
            if (!match.Success)
            {
                throw new FormatException("Link doesn't match to regular expression");
            }

            result.Url = new Uri(match.Groups[1].Value);
            var attributes = match.Groups[2].Value
                .Split(";")
                .Select(LinkHeaderItem.Parse)
                .ToArray();

            result.Items.AddRange(attributes);

            return result;
        }

        public override string ToString()
        {
            var result = $"<{Url.OriginalString}>";
            if (Items.Count > 0)
            {
                result += $"; {string.Join("; ", Items.Select(o => o.ToString()))}";
            }
            return result;
        }
    }
}
