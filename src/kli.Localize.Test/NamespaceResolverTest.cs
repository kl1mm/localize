using System.Collections.Generic;
using kli.Localize.Generator.Internal;
using Xunit;

namespace kli.Localize.Test
{
    public class NamespaceResolverTest
    {
        private const string originFilePath = @"C:\_git\SLN\Project.Name\Folder\File.json";
        private const string fallback = "kli.Fall";

        [Fact]
        public void TestResolve()
        {
            var options = new Dictionary<string, string>
            {
                { "build_property.rootnamespace", "kli.Spring" },
                { "build_property.projectdir", @"C:\_git\SLN\Project.Name\" },
            };

            var resolver = new NamespaceResolver(originFilePath, fallback, options.TryGetValue);

            Assert.Equal("kli.Spring.Folder", resolver.Resolve());
        }

        [Fact]
        public void TestResolveProjectDirWithoutSlash()
        {
            var options = new Dictionary<string, string>
            {
                { "build_property.rootnamespace", "kli.Spring" },
                { "build_property.projectdir", @"C:\_git\SLN\Project.Name" },
            };

            var resolver = new NamespaceResolver(originFilePath, fallback, options.TryGetValue);
            Assert.Equal("kli.Spring.Folder", resolver.Resolve());
        }

        [Fact]
        public void TestResolveCaseSensitvePathRoot()
        {
            var options = new Dictionary<string, string>
            {
                { "build_property.rootnamespace", "kli.Spring" },
                { "build_property.projectdir", @"c:\_git\SLN\Project.Name\" },
            };

            var resolver = new NamespaceResolver(originFilePath, fallback, options.TryGetValue);
            Assert.Equal("kli.Spring.Folder", resolver.Resolve());
        }

        [Fact]
        public void TestResolveFallbackNamespaceNoProjectDir()
        {
            var options = new Dictionary<string, string>
            {
                { "build_property.rootnamespace", "kli.Spring" },
            };

            var resolver = new NamespaceResolver(originFilePath, fallback, options.TryGetValue);
            Assert.Equal("kli.Spring", resolver.Resolve());
        }

        [Fact]
        public void TestResolveFallbackNamespaceNoRoot()
        {
            var options = new Dictionary<string, string>
            {
                { "build_property.projectdir", @"C:\_git\SLN\Project.Name\" },
            };

            var resolver = new NamespaceResolver(originFilePath, fallback, options.TryGetValue);
            Assert.Equal("kli.Fall.Folder", resolver.Resolve());
        }

        [Fact]
        public void TestResolveFallbackNamespaceNoOptions()
        {
            var options = new Dictionary<string, string>();

            var resolver = new NamespaceResolver(originFilePath, fallback, options.TryGetValue);
            Assert.Equal("kli.Fall", resolver.Resolve());
        }
    }
}
