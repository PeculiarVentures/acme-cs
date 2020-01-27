using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PeculiarVentures.ACME.Web.Http
{
    public class LinkHeaderItemCollection : CollectionBase, IEnumerable<LinkHeaderItem>
    {
        /// <summary>
        /// Gets <see cref="LinkHeaderItem"/> by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Returns <see cref="LinkHeaderItem"/> or null</returns>
        public LinkHeaderItem this[string name]
        {
            get
            {
                foreach (LinkHeaderItem item in InnerList)
                {
                    if (item.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return item;
                    }
                }
                return null;
            }
        }

        public LinkHeaderItemCollection()
        {
        }

        public LinkHeaderItemCollection(LinkHeaderItem item)
        {
            Add(item);
        }

        /// <summary>
        /// Adds an <see cref="LinkHeaderItem"/> to the end of the collection
        /// </summary>
        /// <param name="item">The object to add to the collection</param>
        /// <returns>
        /// The position into which the new element was inserted,
        /// or -1 to indicate that the item was not inserted into the collection.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        public int Add(LinkHeaderItem item)
        {
            #region Check arguments
            if (item is null)
            {
                throw new System.ArgumentNullException(nameof(item));
            }
            #endregion

            return InnerList.Add(item);
        }

        /// <summary>
        /// Adds an <see cref="LinkHeaderItem"/> to the end of the collection
        /// </summary>
        /// <param name="name">Link parameter name</param>
        /// <param name="value">Link parameter value</param>
        /// <param name="quated">Determines if the value is in quater marks</param>
        /// <returns>
        /// The position into which the new element was inserted,
        /// or -1 to indicate that the item was not inserted into the collection.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="NotSupportedException"/>
        public int Add(string name, string value, bool quated = false)
        {
            return Add(new LinkHeaderItem(name, value, quated));
        }

        /// <summary>
        /// Adds a list of <see cref="LinkHeaderItem"/>
        /// </summary>
        /// <param name="items">A list of <see cref="LinkHeaderItem"/></param>
        public void AddRange(LinkHeaderItem[] items)
        {
            InnerList.AddRange(items);
        }

        IEnumerator<LinkHeaderItem> IEnumerable<LinkHeaderItem>.GetEnumerator()
        {
            return InnerList.Cast<LinkHeaderItem>().GetEnumerator();
        }
    }
}
