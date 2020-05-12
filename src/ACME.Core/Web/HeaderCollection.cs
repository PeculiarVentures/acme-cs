using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using PeculiarVentures.ACME.Web.Http;

namespace PeculiarVentures.ACME.Web
{
    public class HeaderCollection : WebHeaderCollection
    {
        public const string ReplayNonceHeader = "Replay-Nonce";
        public const string ContentTypeHeader = "Content-Type";
        public const string LinkHeader = "Link";
        public const string LocationHeader = "Location";

        public HeaderCollection()
        {
        }

        public string ReplayNonce {
            get => Get(ReplayNonceHeader);
            set => Set(ReplayNonceHeader, value);
        }

        public string Location
        {
            get => Get(LocationHeader);
            set => Set(LocationHeader, value);
        }

        public string ContentType
        {
            get => Get(ContentTypeHeader);
            set => Set(ContentTypeHeader, value);
        }

        private LinkHeaderCollection _link;
        public LinkHeaderCollection Link
        {
            get
            {
                if (_link is null) {
                    var header = Get(LinkHeader);
                    var links = header?
                        .Split(',')
                        .Select(o => o.Trim())
                        .ToArray()
                        ?? new string[0];
                    _link = new LinkHeaderCollection(links)
                    {
                        Parent = this,
                    };
                    _link.CollectionChanged += Link_CollectionChanged;
                }

                return _link;
            }
            set
            {
                if (_link is null)
                {
                    _link = new LinkHeaderCollection()
                    {
                        Parent = this,
                    };
                    _link.CollectionChanged += Link_CollectionChanged;
                }
                Set(LinkHeader, _link.ToString());
            }
        }

        private static void Link_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var links = (LinkHeaderCollection)sender;
            links.Parent.Set(LinkHeader, links.ToString());
        }
    }
}
