using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using TestUtility;
using Xunit;
using Xunit.Abstractions;

namespace OmniSharp.Lsp.Tests
{
    public class OmniSharpDefinitionHandlerFacts : AbstractLanguageServerTestBase
    {
        public OmniSharpDefinitionHandlerFacts(ITestOutputHelper output)
            : base(output)
        {
        }

        [Fact]
        public async Task CanFindDefinitionOfBaseClass()
        {
            const string code = @"
                public class FooBase
                {
                }

                public class FooDerivedA : Foo$$Base
                {
                }";

            var definition = await FindDefinitionAsync(code);

            Assert.Single(definition);
            Assert.Equal("", definition.Single().Location.Uri);
            Assert.Equal(new Range((0,0), (0,0)), definition.Single().Location.Range);
        }

        private Task<LocationOrLocationLinks> FindDefinitionAsync(string code)
        {
            return FindDefinitionAsync(new[] { new TestFile("dummy.cs", code) });
        }

        private async Task<LocationOrLocationLinks> FindDefinitionAsync(TestFile[] testFiles)
        {
            OmniSharpTestHost.AddFilesToWorkspace(testFiles
                .Select(f =>
                    new TestFile(
                        ((f.FileName.StartsWith("/") || f.FileName.StartsWith("\\")) ? f.FileName : ("/" + f.FileName))
                        .Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar), f.Content))
                .ToArray()
            );

            var file = testFiles.Single(tf => tf.Content.HasPosition);
            var point = file.Content.GetPointFromPosition();

            Client.TextDocument.DidChangeTextDocument(new DidChangeTextDocumentParams
            {
                ContentChanges = new Container<TextDocumentContentChangeEvent>(new TextDocumentContentChangeEvent
                {
                    Text = file.Content.Code
                }),
                TextDocument = new OptionalVersionedTextDocumentIdentifier
                {
                    Uri = DocumentUri.From(file.FileName),
                    Version = 1
                }
            });

            return await Client.TextDocument.RequestDefinition(new DefinitionParams
            {
                Position = new Position(point.Line, point.Offset),
                TextDocument = new TextDocumentIdentifier(DocumentUri.From(file.FileName))
            }, CancellationToken);
        }
    }
}
