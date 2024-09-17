using System;
using System.IO;
using kli.Localize.Generator.Internal.Helper;
using kli.Localize.Generator.Internal.Json;
using Microsoft.CodeAnalysis;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Shouldly;
using Xunit;

namespace kli.Localize.Test
{
    public class JsonTranslationReaderTest
    {
        private readonly Action<Diagnostic> reporterMock = Substitute.For<Action<Diagnostic>>();

        [Fact]
        public void TestInvalidFile()
        {
            var filePath = Path.GetRandomFileName();
            try
            {
                File.WriteAllText(filePath, "No Json");

                var reader = new JsonTranslationReader(reporterMock);
                var translations = reader.Read(Path.Combine(filePath));

                translations.Count.ShouldBe(0);
                this.reporterMock.Received(Quantity.Exactly(1));
            }
            finally
            {
                File.Delete(filePath);
            }
        }          
        
        [Fact]
        public void TestRead()
        {
            var filePath = Path.GetRandomFileName();
            try
            {
                File.WriteAllText(filePath, "{\"Foo\": \"One\", \"Bar\": \"Two\"}");

                var reader = new JsonTranslationReader(reporterMock);
                var translations = reader.Read(Path.Combine(filePath));

                Assert.Equal(2, translations.Count);
                Assert.Equal("One", translations["Foo"]);
                Assert.Equal("Two", translations["Bar"]);

                this.reporterMock.Received(Quantity.None());
            }
            finally
            {
                File.Delete(filePath);
            }
        }    
        
        [Fact]
        public void TestReadComplex()
        {
            var filePath = Path.GetRandomFileName();
            try
            {
                var testData = """
                    {
                        "SomeText": "irgendein Text",
                        "Errors": 
                        {
                            "FileNotFound": "Datei nicht gefunden",
                            "DivideByZero": "Durch Null geteilt"
                        },
                        "Warnings":{
                            " ": "Blank",                   // -> skiped with warning
                            "Caution": "Achtung"
                        },
                        
                        "UI":{
                            "LabelOne": "Eins",
                            "LabelTwo": "Zwei",
                            "Login": {
                                "LabelUserName": "Benutzer",
                                "LabelPassword": "Passwort"
                            }
                        }
                    }
                    """;                 
                File.WriteAllText(filePath, testData);

                var reader = new JsonTranslationReader(reporterMock);
                var translations = reader.Read(Path.Combine(filePath));

                translations.Count.ShouldBe(4);
                translations["UI"].ShouldBeOfType<TranslationData>();
                ((TranslationData)translations["UI"])["Login"].ShouldBeOfType<TranslationData>();

                this.reporterMock.Received(Quantity.Exactly(1));
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

                var reader = new JsonTranslationReader(this.reporterMock);

                reader.Read(Path.Combine(filePath)).Count.ShouldBe(1);
                this.reporterMock.Received(Quantity.Exactly(2));
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

                var reader = new JsonTranslationReader(reporterMock);

                reader.Read(Path.Combine(filePath)).Count.ShouldBe(0);
                this.reporterMock.Received(Quantity.Exactly(1));
            }
            finally
            {
                File.Delete(filePath);
            }
        }
    }
}
