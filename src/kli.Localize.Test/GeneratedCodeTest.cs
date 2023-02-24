using System.Globalization;
using kli.Localilizations;
using Xunit;

namespace kli.Localize.Test
{
    public class GeneratedCodeTest
    {
        public GeneratedCodeTest()
        {
            CultureInfo.CurrentUICulture = new CultureInfo("de-DE");
        }

        [Theory]
        [InlineData("Neutral", "de")]
        [InlineData("EN", "en")]
        [InlineData("US", "en-US")]
        [InlineData("EN", "en-AU")]
        [InlineData("Neutral", "fr")]
        [InlineData("¡Hola Mundo!", "es-EC")]
        [InlineData("嗨，大家好", "zh-CHS")]
        public void TestLocale(string expected, string culture)
        {
            CultureInfo.CurrentUICulture = new CultureInfo(culture);
            Assert.Equal(expected, MyLocale.TestKey);
            Assert.Equal("Nür hiär", MyLocale.OnlyHere); 
        }

        [Fact]
        public void TestSpecialChars()
        {
            Assert.Equal("\" \\ @ ß", MyLocale.SpecialChars);
        }

        [Fact]
        public void TestGetString()
        {
            Assert.Equal("Neutral", MyLocale.GetString(nameof(MyLocale.TestKey)));
            Assert.Equal("US", MyLocale.GetString(nameof(MyLocale.TestKey), new CultureInfo("en-US")));
            Assert.Equal("Unknown", MyLocale.GetString("Unknown"));
            Assert.Equal("Unknown", MyLocale.GetString("Unknown", new CultureInfo("en-US")));
        }

        [Fact]
        public void TestGetAll()
        {
            Assert.Equal(6, MyLocale.GetAll().Count);
            Assert.Equal(1, MyLocale.GetAll(new CultureInfo("en-US")).Count);
            Assert.Equal(6, MyLocale.GetAll(new CultureInfo("fr")).Count);
        }
    }
}
