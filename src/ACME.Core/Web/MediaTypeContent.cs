using System;
using System.IO;
using System.Text;

namespace PeculiarVentures.ACME.Web
{
    public class MediaTypeContent
    {
        public string Type { get; }

        public Stream Content { get; }

        public MediaTypeContent(string type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public MediaTypeContent(string type, Stream content) : this(type)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public MediaTypeContent(string type, string content) : this(type)
        {
            Content = new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        public MediaTypeContent(string type, byte[] content) : this(type)
        {
            Content = new MemoryStream(content);
        }

        public byte[] ToArray()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Content.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
