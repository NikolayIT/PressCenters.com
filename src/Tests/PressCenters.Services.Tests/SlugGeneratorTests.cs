namespace PressCenters.Services.Tests
{
    using PressCenters.Services;

    using Xunit;

    public class SlugGeneratorTests
    {
        [Fact]
        public void GenerateSlugShouldTrimResult()
        {
            var input =
                "Входът може да бъде много много много много много много много много много много много много много много много много много дълъг тест ";
            var slugGenerator = new SlugGenerator();
            var result = slugGenerator.GenerateSlug(input);
            Assert.True(result.Length <= 100);
        }

        [Fact]
        public void GenerateSlugShouldWorkCorrectly()
        {
            var input = "МВР: Свада в училище в ж.к.-„Западен парк“.";
            var expected = "mvr-svada-v-uchilishte-v-jk-zapaden-park";
            var slugGenerator = new SlugGenerator();
            var result = slugGenerator.GenerateSlug(input);
            Assert.Equal(expected, result);
        }
    }
}
