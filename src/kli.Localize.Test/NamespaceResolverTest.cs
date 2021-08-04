using System;
using System.Collections.Generic;
using System.IO;
using kli.Localize.Generator.Internal;
using Xunit;

namespace kli.Localize.Test
{
    public class NamespaceResolverTest
    {
        private const string fallback = "kli.Fall";
        private static string originFilePath = @"_git\SLN\Project.Name\Folder\File.json".ToOsSpecificPath();

        [Fact]
        public void TestResolve()
        {
            var options = new Dictionary<string, string>
            {
                { "build_property.rootnamespace", "kli.Spring" },
                { "build_property.projectdir", @"_git\SLN\Project.Name\".ToOsSpecificPath() },
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
                { "build_property.projectdir", @"_git\SLN\Project.Name".ToOsSpecificPath() },
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
                { "build_property.projectdir", @"_git\SLN\Project.Name\".ToOsSpecificPath(@"c:\") },
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
               { "build_property.projectdir", @"_git\SLN\Project.Name".ToOsSpecificPath()  },
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

    internal static class Ext
    {
        public static string ToOsSpecificPath(this string path, string winRoot = @"C:\", string linRoot = "/")
        {
            string osRootPath = OperatingSystem.IsWindows() ? winRoot : linRoot;
            return Path.Combine(osRootPath, Path.Combine(path.Split('\\')));
        }
    }
}
