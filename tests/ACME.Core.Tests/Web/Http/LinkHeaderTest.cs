using Xunit;

namespace PeculiarVentures.ACME.Web.Http
{
    public class LinkHeaderTest
    {
        [Fact(DisplayName = "Parse: </>;rel=\"index\"")]
        public void Parse1()
        {
            var link = LinkHeader.Parse("</>;rel=\"index\"");
            Assert.Equal("/", link.Url.OriginalString);

            var rel = link.Items["REL"];
            Assert.NotNull(rel);
            Assert.Equal("rel", rel.Name);
            Assert.Equal("index", rel.Value);
            Assert.True(rel.Quoted);
        }

        [Fact(DisplayName = "Parse: <http://example.com/TheBook/chapter2>; rel=\"previous\"; title=\"previous chapter\"")]
        public void Parse2()
        {
            var link = LinkHeader.Parse("<http://example.com/TheBook/chapter2>; rel=\"previous\"; title=\"previous chapter\"");
            Assert.Equal("http://example.com/TheBook/chapter2", link.Url.OriginalString);

            var rel = link.Items["rel"];
            Assert.NotNull(rel);
            Assert.Equal("rel", rel.Name);
            Assert.Equal("previous", rel.Value);
            Assert.True(rel.Quoted);

            var title = link.Items["title"];
            Assert.NotNull(title);
            Assert.Equal("title", title.Name);
            Assert.Equal("previous chapter", title.Value);
            Assert.True(rel.Quoted);
        }

        [Fact(DisplayName = "Parse: </TheBook/chapter2>; rel=\"previous\"; title*=UTF-8'de'letztes%20Kapitel")]
        public void Parse3()
        {
            var link = LinkHeader.Parse("</TheBook/chapter2>; rel=\"previous\"; title*=UTF-8'de'letztes%20Kapitel");
            Assert.Equal("/TheBook/chapter2", link.Url.OriginalString);

            var rel = link.Items["rel"];
            Assert.NotNull(rel);
            Assert.Equal("rel", rel.Name);
            Assert.Equal("previous", rel.Value);
            Assert.True(rel.Quoted);

            var title = link.Items["title*"];
            Assert.NotNull(title);
            Assert.Equal("title*", title.Name);
            Assert.Equal("UTF-8'de'letztes%20Kapitel", title.Value);
            Assert.True(rel.Quoted);
        }

        [Fact(DisplayName = "Serialize: </>; rel=\"index\"")]
        public void Serializae1()
        {
            var link = new LinkHeader("/", new LinkHeaderItem("rel", "index", true));

            Assert.Equal("</>; rel=\"index\"", link.ToString());
        }

        [Fact(DisplayName = "Serialize: </TheBook/chapter2>; rel=\"previous\"; title*=UTF-8'de'letztes%20Kapitel")]
        public void Serializae2()
        {
            var link = new LinkHeader("/TheBook/chapter2");
            link.Items.Add(new LinkHeaderItem("rel", "previous", true));
            link.Items.Add(new LinkHeaderItem("title*", "UTF-8'de'letztes%20Kapitel"));

            Assert.Equal("</TheBook/chapter2>; rel=\"previous\"; title*=UTF-8'de'letztes%20Kapitel", link.ToString());
        }
    }
}
