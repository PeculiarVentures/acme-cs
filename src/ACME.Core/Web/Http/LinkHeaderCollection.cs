using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PeculiarVentures.ACME.Web.Http
{
    public class LinkHeaderCollection : CollectionBase, IEnumerable<LinkHeader>
    {

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

            InnerList.AddRange(values.Select(LinkHeader.Parse).ToArray());
        }

        public LinkHeaderCollection(LinkHeader[] linkHeaders)
        {
            #region Check arguments

            if (linkHeaders is null)
            {
                throw new ArgumentNullException(nameof(linkHeaders));
            }
            #endregion

            InnerList.AddRange(linkHeaders.ToArray());
        }

        public int Add(LinkHeader item)
        {
            #region Check arguments
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            #endregion

            return List.Add(item);
        }

        IEnumerator<LinkHeader> IEnumerable<LinkHeader>.GetEnumerator()
        {
            return List.Cast<LinkHeader>().GetEnumerator();
        }
    }
}
