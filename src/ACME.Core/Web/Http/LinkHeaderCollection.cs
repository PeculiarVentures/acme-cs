using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PeculiarVentures.ACME.Web.Http
{
    public class LinkHeaderCollection : ObservableCollection<LinkHeader>
    {

        internal HeaderCollection Parent { get; set; }

        public LinkHeaderCollection()
        {
        }

        public LinkHeaderCollection(string[] values)
        {
            #region Check arguments
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            #endregion
            foreach (var item in values)
            {
                Items.Add(LinkHeader.Parse(item));
            }
        }

        public string FindUrl(string rel)
        {
            foreach (var item in Items)
            {
                if (item.Items.FirstOrDefault(o => o.Value.ToUpper() == rel.ToUpper()) != null)
                {
                    return item.Url.ToString();
                };
            }
            return null;
        }

        public LinkHeaderCollection(LinkHeader[] linkHeaders)
        {
            #region Check arguments

            if (linkHeaders is null)
            {
                throw new ArgumentNullException(nameof(linkHeaders));
            }
            #endregion
            foreach (var item in linkHeaders)
            {
                Items.Add(item);
            }
        }

        public override string ToString()
        {
            return string.Join(", ", this.ToArray().Select(o => o.ToString()).ToArray());
        }
    }
}
