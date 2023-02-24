using System;
using System.IO;
using kli.Localize.Generator.Internal;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Moq;
using Xunit;

namespace kli.Localize.Test
{
    public class NamesResolverTest
    {
        private const string fallback = "kli.Fall";
        private static readonly string originFilePath = @"_git\SLN\Project.Name\Folder\File.json".ToOsSpecificPath();

        [Fact]
        public void TestResolve()
        {
            var file = Mock.Of<AdditionalText>(m => m.Path == originFilePath);
            var optionsProvider = this.SetupOptionsProvider(rootNamespace: "kli.Spring", projectDir: @"_git\SLN\Project.Name");
            var resolver = new NamesResolver(file, fallback, optionsProvider);

            Assert.Equal("kli.Spring.Folder", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveByNamesapceNameParameter()
        {
            var file = Mock.Of<AdditionalText>(m => m.Path == originFilePath);
            var optionsProvider = this.SetupOptionsProvider(rootNamespace: "kli.Spring", projectDir: @"_git\SLN\Project.Name", nameSpaceName: "kli.Summer");
            var resolver = new NamesResolver(file, fallback, optionsProvider);
            Assert.Equal("kli.Summer", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveProjectDirWithoutSlash()
        {
            var file = Mock.Of<AdditionalText>(m => m.Path == originFilePath);
            var optionsProvider = this.SetupOptionsProvider(rootNamespace: "kli.Spring", projectDir: @"_git\SLN\Project.Name");
            var resolver = new NamesResolver(file, fallback, optionsProvider);
            Assert.Equal("kli.Spring.Folder", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveCaseSensitvePathRoot()
        {
            var file = Mock.Of<AdditionalText>(m => m.Path == originFilePath);
            var optionsProvider = this.SetupOptionsProvider(rootNamespace: "kli.Spring", projectDir: @"_git\SLN\Project.Name\");
            var resolver = new NamesResolver(file, fallback, optionsProvider);
            Assert.Equal("kli.Spring.Folder", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveWithWhiteSpaceInPath()
        {
            var file = Mock.Of<AdditionalText>(m => m.Path == @"_git\Components\Components CoMet Business\File.json".ToOsSpecificPath("c:\\", "/"));
            var optionsProvider = this.SetupOptionsProvider(rootNamespace: "kli.Components.CoMet", projectDir: @"_git\Components\Components CoMet Business");
            var resolver = new NamesResolver(file, fallback, optionsProvider);
            Assert.Equal("kli.Components.CoMet", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveFallbackNamespaceNoProjectDir()
        {
            var file = Mock.Of<AdditionalText>(m => m.Path == originFilePath);
            var optionsProvider = this.SetupOptionsProvider(rootNamespace: "kli.Spring");
            var resolver = new NamesResolver(file, fallback, optionsProvider);
            Assert.Equal("kli.Spring", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveFallbackNamespaceNoRoot()
        {
            var file = Mock.Of<AdditionalText>(m => m.Path == originFilePath);
            var optionsProvider = this.SetupOptionsProvider(projectDir: @"_git\SLN\Project.Name\");
            var resolver = new NamesResolver(file, fallback, optionsProvider);
            Assert.Equal("kli.Fall.Folder", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveFallbackNamespaceNoOptions()
        {
            var file = Mock.Of<AdditionalText>(m => m.Path == originFilePath);
            var resolver = new NamesResolver(file, fallback, this.SetupOptionsProvider());
            
            Assert.Equal("kli.Fall", resolver.ResolveNamespace());
        }

        private AnalyzerConfigOptionsProvider SetupOptionsProvider(string rootNamespace = null, string projectDir = null, string nameSpaceName = null)
        {
            projectDir = projectDir?.ToOsSpecificPath();
            
            var optionsMock = new Mock<AnalyzerConfigOptions>();
            var mockAnalyzerConfigOptionsProvider = new Mock<AnalyzerConfigOptionsProvider>();
            mockAnalyzerConfigOptionsProvider.Setup(m => m.GetOptions(It.IsAny<AdditionalText>())).Returns(optionsMock.Object);
            mockAnalyzerConfigOptionsProvider.Setup(m => m.GlobalOptions).Returns(optionsMock.Object);
            
            optionsMock.Setup(m => m.TryGetValue("build_property.rootnamespace", out rootNamespace)).Returns(!string.IsNullOrEmpty(rootNamespace));
            optionsMock.Setup(m => m.TryGetValue("build_property.projectdir", out projectDir)).Returns(!string.IsNullOrEmpty(projectDir));
            optionsMock.Setup(m => m.TryGetValue("build_metadata.AdditionalFiles.NamespaceName", out nameSpaceName)).Returns(!string.IsNullOrEmpty(nameSpaceName));
            
            return mockAnalyzerConfigOptionsProvider.Object;
        }
    }

    internal static class Ext
    {
        public static string ToOsSpecificPath(this string path, string winRoot = @"c:\", string linRoot = "/")
        {
            string osRootPath = OperatingSystem.IsWindows() ? winRoot : linRoot;
            return Path.Combine(osRootPath, Path.Combine(path.Split('\\')));
        }
    }
}
