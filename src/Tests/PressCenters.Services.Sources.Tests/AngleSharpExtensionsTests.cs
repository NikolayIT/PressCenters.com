namespace PressCenters.Services.Sources.Tests
{
    using AngleSharp.Html.Parser;

    using Xunit;

    public class AngleSharpExtensionsTests
    {
        private const string Html =
            "<div id=\"container\"><p id=\"first\">One <b id=\"nested\">two</b></p><p>Three</p></div><p id=\"outside\">Four</p>";

        [Fact]
        public void RemoveRecursivelyShouldRemoveANestedDescendant()
        {
            var document = new HtmlParser().ParseDocument(Html);
            var container = document.GetElementById("container");

            container.RemoveRecursively(document.GetElementById("nested"));

            Assert.Equal("<p id=\"first\">One </p><p>Three</p>", container.InnerHtml);
        }

        [Fact]
        public void RemoveRecursivelyShouldNotRemoveNodesOutsideTheContainerOrTheContainerItself()
        {
            var document = new HtmlParser().ParseDocument(Html);
            var container = document.GetElementById("container");

            container.RemoveRecursively(document.GetElementById("outside"));
            container.RemoveRecursively(container);

            Assert.NotNull(document.GetElementById("outside"));
            Assert.NotNull(document.GetElementById("container"));
        }

        [Fact]
        public void RemoveRecursivelyShouldIgnoreAMissingContainerOrElement()
        {
            var document = new HtmlParser().ParseDocument(Html);

            document.QuerySelector(".missing").RemoveRecursively(document.GetElementById("first"));
            document.GetElementById("container").RemoveRecursively(null);

            Assert.NotNull(document.GetElementById("first"));
        }
    }
}
