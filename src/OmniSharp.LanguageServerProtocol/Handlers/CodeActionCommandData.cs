using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace OmniSharp.LanguageServerProtocol.Handlers
{
    internal class CodeActionCommandData
    {
        public DocumentUri Uri { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public Range Range { get; set; }
    }
}
