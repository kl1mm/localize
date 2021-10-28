using System.Globalization;
using System.Linq;
using kli.Localize.Test.TestLocalizations;
using kli.Localize.Web;
using Xunit;

namespace kli.Localize.Test
{
    public class LocalizerTest
    {

        [Fact]
        public void TestWebLocalizer()
        {
            CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            var localizer = new FromGeneratedLocalizer<object, Locale>();

            var locales = localizer.GetAllStrings(true).ToList();
            Assert.Equal(3, locales.Count);
            Assert.Equal("1", locales.First(l => l.Name == "XYZ"));
            Assert.Equal("US", locales.First(l => l.Name == "TestKey"));
            Assert.Equal("US", localizer["TestKey"]);
            Assert.False(localizer["TestKey"].ResourceNotFound);
            Assert.Equal("NotExistingKey", localizer["NotExistingKey"]);
            Assert.True(localizer["NotExistingKey"].ResourceNotFound);
            Assert.Equal("what", localizer["Key with space"]);
        }

        [Fact]
        public void TestServiceCollection()
        {
            ServiceCollectionExtensions.AddLocalizationFromGenerated(null, o =>
            {
                o.LocationsToSearchGeneratedLocales = new []{ typeof(Locale).Assembly};
                o.UseExistingLocalizationRegistrationsAsFallback = true;
            });
        }
    }
}

