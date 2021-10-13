using kli.Localize.Generator.Internal;
using Xunit;

namespace kli.Localize.Test
{
    public class CultureDataTest
    {
        [Theory]
        [InlineData("en-US", "Locale_en-US.json")]
        [InlineData("de-DE", "Locale_de-DE.json")]
        [InlineData("de-DE", "Locale.de-DE.json")]
        [InlineData("de", "Locale.de.json")]
        [InlineData("en", "Locale.en.json")]
        public void CanReadCultureNameFromFileName(string expected, string fileName)
        {
            Assert.Equal(expected, CultureData.ReadCultureNameFromFileName(fileName));
        }
    }
}