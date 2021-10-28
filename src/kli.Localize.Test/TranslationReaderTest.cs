﻿using System;
using System.IO;
using System.Linq;
using kli.Localize.Generator.Internal;
using Microsoft.CodeAnalysis;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace kli.Localize.Test
{
    public class TranslationReaderTest
    {
        private readonly Mock<Action<Diagnostic>> reporterMock = new();

        [Fact]
        public void TestRead()
        {
            var filePath = Path.GetRandomFileName();
            try
            {
                File.WriteAllText(filePath, "{\"Foo\": \"One\", \"Bar\": \"Two\"}");

                var reader = new TranslationReader(reporterMock.Object);
                var translations = reader.Read(Path.Combine(filePath));

                Assert.Equal(2, translations.Count);
                Assert.Equal("One", translations["Foo"]);
                Assert.Equal("Two", translations["Bar"]);

                reporterMock.Verify(m => m(It.IsAny<Diagnostic>()), Times.Never);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public void TestReadWarnings()
        {
            var filePath = Path.GetRandomFileName();
            try
            {
                File.WriteAllText(filePath, "{\"Foo\": \"One\", \"1\": \"One\", \" \": \"Blank\"}");

                var reader = new TranslationReader(reporterMock.Object);

                var result = reader.Read(Path.Combine(filePath));
                Assert.Single(result);
                reporterMock.Verify(m => m(It.IsAny<Diagnostic>()), Times.Exactly(2));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        [Fact]
        public void TestReadError()
        {
            var filePath = Path.GetRandomFileName();
            try
            {
                File.WriteAllText(filePath, "{\"ArrayValue\": [ 1, 2, 3 ]}");

                var reader = new TranslationReader(reporterMock.Object);

                var ex = Assert.Throws<FormatException>(() => reader.Read(Path.Combine(filePath)));
                Assert.Contains("Only string values are allowed", ex.Message);
            }
            finally
            {
                File.Delete(filePath);
            }
        }


        [Fact]
        public void TestReadYaml()
        {
            var filePath = Path.GetRandomFileName()+".yml";
            try
            {
                File.WriteAllText(filePath, "Foo: One \r\n1: One \r\nMyKey: My key");

                var reader = new TranslationReader(reporterMock.Object);
                var result = reader.Read(Path.Combine(filePath));
                Assert.Equal(2, result.Count);
                reporterMock.Verify(m => m(It.IsAny<Diagnostic>()), Times.Exactly(1));
            }
            finally
            {
                File.Delete(filePath);
            }
        }

    }
}
