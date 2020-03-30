using System.Globalization;
using kli.Localize.Test.Localization;
using Xunit;

namespace kli.Localizer.Tool.Test
{
    public class UnitTests
    {
        [Theory]
        [InlineData("German", "de")]
        [InlineData("US", "en-US")]
        [InlineData("Englisch", "en-EN")]
        [InlineData("Englisch", "en")]
        [InlineData("German", "zt")]
        public void TestAccessGeneratedCode(string expected, string culture)
        {
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            Assert.Equal(expected, Resources.Name);
            Assert.Equal("Wert", Resources.OnlyInGerman);
        }
    }
}
