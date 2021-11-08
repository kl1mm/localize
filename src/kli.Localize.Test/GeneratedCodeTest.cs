using System.Globalization;
using kli.Localize.Generator.Base;
using kli.Localize.Test.TestLocalizations;
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
            Assert.Equal(expected, Locale.TestKey);
            Assert.Equal("Nür hiär", Locale.OnlyHere);
        }

        [Fact]
        public void TestSpecialChars()
        {
            Assert.Equal("\" \\ @ ß", Locale.SpecialChars);
        }

        [Fact]
        public void TestGetString()
        {
            Assert.Equal("Neutral", Locale.GetString(nameof(Locale.TestKey)));
            Assert.Equal("US", Locale.GetString(nameof(Locale.TestKey), new CultureInfo("en-US")));
            Assert.Equal("Unknown", Locale.GetString("Unknown"));
            Assert.Equal("Unknown", Locale.GetString("Unknown", new CultureInfo("en-US")));
        }

        [Fact]
        public void TestNameBehavior()
        {
            Assert.Equal("was", Locale.GetString("Key with space"));
            Assert.Equal("was", Locale.Key_with_space);
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            Assert.Equal("what", Locale.GetString("Key with space"));
            Assert.Equal("what", Locale.Key_with_space);
        }

        [Fact]
        public void TestGetAll()
        {
            
            Assert.Equal(10, Locale.GetAll().Count);
            Assert.Equal(1, Locale.GetAll(new CultureInfo("en-US")).Count);
            Assert.Equal(10, Locale.GetAll(new CultureInfo("fr")).Count);
        }

        [Fact]
        public void TestStatic()
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            Assert.Equal("US", Locale.GetString(nameof(Locale.TestKey), false));
            Assert.Equal("Nür hiär", Locale.GetString(nameof(Locale.OnlyHere), true));
            Assert.Null(Locale.GetString(nameof(Locale.OnlyHere), false));
            
            var allWithParents = Locale.GetAll(true);
            Assert.Equal(3, allWithParents.Count);
            Assert.Equal("1", allWithParents["XYZ"]);
            Assert.Equal("US", allWithParents["TestKey"]);
            var allWithoutParents = Locale.GetAll(false);
            Assert.Equal(1, allWithoutParents.Count);
            Assert.Equal("US", allWithParents["TestKey"]);
        }
    }
}

