using System;
using System.IO;
using kli.Localize.Generator.Internal.Helper;
using kli.Localize.Generator.Internal.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
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
            var additionalText = AdditionalTextMock(content: "No Json");

            var reader = new JsonTranslationReader(reporterMock);
            var translations = reader.Read(additionalText);

            translations.Count.ShouldBe(0);
            this.reporterMock.Received(Quantity.Exactly(1));
        }

        [Fact]
        public void TestRead()
        {
            var additionalText = AdditionalTextMock(content: "{\"Foo\": \"One\", \"Bar\": \"Two\"}");

            var reader = new JsonTranslationReader(reporterMock);
            var translations = reader.Read(additionalText);

            Assert.Equal(2, translations.Count);
            Assert.Equal("One", translations["Foo"]);
            Assert.Equal("Two", translations["Bar"]);

            this.reporterMock.Received(Quantity.None());
        }

        [Fact]
        public void TestReadComplex()
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
            var additionalText = AdditionalTextMock(content: testData);

            var reader = new JsonTranslationReader(reporterMock);
            var translations = reader.Read(additionalText);

            translations.Count.ShouldBe(4);
            translations["UI"].ShouldBeOfType<TranslationData>();
            ((TranslationData)translations["UI"])["Login"].ShouldBeOfType<TranslationData>();

            this.reporterMock.Received(Quantity.Exactly(1));
        }

        [Fact]
        public void TestReadWarnings()
        {
            var additionalText = AdditionalTextMock(content: "{\"Foo\": \"One\", \"1\": \"One\", \" \": \"Blank\"}");

            var reader = new JsonTranslationReader(this.reporterMock);

            reader.Read(additionalText).Count.ShouldBe(1);
            this.reporterMock.Received(Quantity.Exactly(2));
        }

        [Fact]
        public void TestReadError()
        {
            var additionalText = AdditionalTextMock(content: "{\"ArrayValue\": [ 1, 2, 3 ]}");
            var reader = new JsonTranslationReader(reporterMock);

            reader.Read(additionalText).Count.ShouldBe(0);
            
            this.reporterMock.Received(Quantity.Exactly(1));
        }

        private AdditionalText AdditionalTextMock(string path = null, string content = null)
        {
            var mock = Substitute.For<AdditionalText>();
            path ??= Path.GetRandomFileName();
            mock.Path.Returns(path);
            if (content != null)
                mock.GetText().Returns(SourceText.From(content));
            return mock;
        }
    }
}
