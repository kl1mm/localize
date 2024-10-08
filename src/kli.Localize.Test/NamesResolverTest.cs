﻿using System;
using System.IO;
using kli.Localize.Generator.Internal.Helper;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute;
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
            var file = AdditionalTextMock(originFilePath);
            var optionsProvider = this.SetupOptionsProvider(rootNamespace: "kli.Spring", projectDir: @"_git\SLN\Project.Name");
            var resolver = new NamesResolver(file, fallback, optionsProvider);

            Assert.Equal("kli.Spring.Folder", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveByNamesapceNameParameter()
        {
            var file = AdditionalTextMock(originFilePath);
            var optionsProvider = this.SetupOptionsProvider(rootNamespace: "kli.Spring", projectDir: @"_git\SLN\Project.Name", nameSpaceName: "kli.Summer");
            var resolver = new NamesResolver(file, fallback, optionsProvider);
            Assert.Equal("kli.Summer", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveProjectDirWithoutSlash()
        {
            var file = AdditionalTextMock(originFilePath);
            var optionsProvider = this.SetupOptionsProvider(rootNamespace: "kli.Spring", projectDir: @"_git\SLN\Project.Name");
            var resolver = new NamesResolver(file, fallback, optionsProvider);
            Assert.Equal("kli.Spring.Folder", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveCaseSensitvePathRoot()
        {
            var file = AdditionalTextMock(originFilePath);
            var optionsProvider = this.SetupOptionsProvider(rootNamespace: "kli.Spring", projectDir: @"_git\SLN\Project.Name\");
            var resolver = new NamesResolver(file, fallback, optionsProvider);
            Assert.Equal("kli.Spring.Folder", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveWithWhiteSpaceInPath()
        {
            var file = AdditionalTextMock(@"_git\Components\Components CoMet Business\File.json".ToOsSpecificPath("c:\\", "/"));
            var optionsProvider = this.SetupOptionsProvider(rootNamespace: "kli.Components.CoMet", projectDir: @"_git\Components\Components CoMet Business");
            var resolver = new NamesResolver(file, fallback, optionsProvider);
            Assert.Equal("kli.Components.CoMet", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveFallbackNamespaceNoProjectDir()
        {
            var file = AdditionalTextMock(originFilePath);
            var optionsProvider = this.SetupOptionsProvider(rootNamespace: "kli.Spring");
            var resolver = new NamesResolver(file, fallback, optionsProvider);
            Assert.Equal("kli.Spring", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveFallbackNamespaceNoRoot()
        {
            var file = AdditionalTextMock(originFilePath);
            var optionsProvider = this.SetupOptionsProvider(projectDir: @"_git\SLN\Project.Name\");
            var resolver = new NamesResolver(file, fallback, optionsProvider);
            Assert.Equal("kli.Fall.Folder", resolver.ResolveNamespace());
        }

        [Fact]
        public void TestResolveFallbackNamespaceNoOptions()
        {
            var file = AdditionalTextMock(originFilePath);
            var resolver = new NamesResolver(file, fallback, this.SetupOptionsProvider());
            
            Assert.Equal("kli.Fall", resolver.ResolveNamespace());
        }

        private AnalyzerConfigOptionsProvider SetupOptionsProvider(string rootNamespace = null, string projectDir = null, string nameSpaceName = null)
        {
            projectDir = projectDir?.ToOsSpecificPath();

            var optionsMock = Substitute.For<AnalyzerConfigOptions>();
            var mockAnalyzerConfigOptionsProvider = Substitute.For<AnalyzerConfigOptionsProvider>();
            mockAnalyzerConfigOptionsProvider.GetOptions(Arg.Any<AdditionalText>()).Returns(optionsMock);
            mockAnalyzerConfigOptionsProvider.GlobalOptions.Returns(optionsMock);

            optionsMock.TryGetValue("build_property.rootnamespace", out Arg.Any<string>())
                .Returns(ci =>
                {
                    ci[1] = rootNamespace;
                    return !string.IsNullOrEmpty(rootNamespace);
                });
            optionsMock.TryGetValue("build_property.projectdir", out Arg.Any<string>())
                .Returns(ci =>
                {
                    ci[1] = projectDir;
                    return !string.IsNullOrEmpty(projectDir);
                });
            optionsMock.TryGetValue("build_metadata.AdditionalFiles.NamespaceName", out Arg.Any<string>())
                .Returns(ci =>
                {
                    ci[1] = nameSpaceName;
                    return !string.IsNullOrEmpty(nameSpaceName);
                });
            
            return mockAnalyzerConfigOptionsProvider;
        }

        private AdditionalText AdditionalTextMock(string path)
        {
            var mock = Substitute.For<AdditionalText>();
            mock.Path.Returns(path);
            return mock;
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
