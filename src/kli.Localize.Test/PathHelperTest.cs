using kli.Localize.Generator.Internal.Helper;
using Shouldly;
using Xunit;

namespace kli.Localize.Test;

public class PathHelperTest
{
    [Theory]
    [InlineData("foo_de.json", "foo")]
    [InlineData("foo_foo_de.json", "foo_foo")]
    public void TestFileNameWithoutCulture(string filePath, string expected)
    {
        var actual = PathHelper.FileNameWithoutCulture(filePath);
        
        actual.ShouldBe(expected);
    }
}