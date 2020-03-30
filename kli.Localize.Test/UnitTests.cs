using System.Globalization;
using kli.Localize.Test;
using kli.Localize.Test.Localization;
using Xunit;

namespace kli.Localizer.Tool.Test
{
    public class UnitTests
    {
        [Theory]
        [InlineData("TestWert", "de")]
        [InlineData("TestValue", "en")]
        public void TestResources(string expected, string culture)
        {
            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            Assert.Equal(expected, Resources.TestKey);
        }

        [Fact]
        public void TestGetAll()
        {
            Assert.Equal(1, Resources.GetAll().Count);
            Assert.Equal(2, InNestedNamespace.GetAll().Count);
        }

        [Theory]
        [InlineData("German", "de")]
        [InlineData("US", "en-US")]
        [InlineData("Englisch", "en-EN")]
        [InlineData("Englisch", "en")]
        [InlineData("German", "zt")]
        public void TestInNestedNamespace(string expected, string culture)
        {
            CultureInfo.CurrentUICulture = new CultureInfo(culture);

            Assert.Equal(expected, InNestedNamespace.Name);
            Assert.Equal("Wert", InNestedNamespace.OnlyInGerman);
        }  
    }        
}
