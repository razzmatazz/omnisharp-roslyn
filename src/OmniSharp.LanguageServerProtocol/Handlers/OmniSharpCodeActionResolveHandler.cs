using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Models.V2.CodeActions;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;

namespace OmniSharp.LanguageServerProtocol.Handlers
{
    internal sealed class OmniSharpCodeActionResolveHandler : CodeActionResolveHandlerBase
    {
        public static IEnumerable<IJsonRpcHandler> Enumerate(
            RequestHandlers handlers,
            ISerializer serializer,
            ILanguageServer mediator,
            DocumentVersions versions)
        {
            foreach (var (_, runActionHandler) in handlers
                     .OfType<Mef.IRequestHandler<RunCodeActionRequest, RunCodeActionResponse>>())
            {
                yield return new OmniSharpCodeActionResolveHandler(runActionHandler, serializer, mediator, versions);
            }
        }

        private readonly Mef.IRequestHandler<RunCodeActionRequest, RunCodeActionResponse> _runActionHandler;
        private readonly ISerializer _serializer;
        private readonly ILanguageServer _server;
        private readonly DocumentVersions _documentVersions;

        public OmniSharpCodeActionResolveHandler(
            Mef.IRequestHandler<RunCodeActionRequest, RunCodeActionResponse> runActionHandler,
            ISerializer serializer,
            ILanguageServer server,
            DocumentVersions documentVersions)
        {
            _runActionHandler = runActionHandler;
            _serializer = serializer;
            _server = server;
            _documentVersions = documentVersions;
        }

        public override async Task<CodeAction> Handle(CodeAction request, CancellationToken cancellationToken)
        {
            var data = request.Data.ToObject<CodeActionCommandData>();

            var omnisharpCaRequest = new RunCodeActionRequest {
                Identifier = data.Identifier,
                FileName = data.Uri.GetFileSystemPath(),
                Column = data.Range.Start.Character,
                Line = data.Range.Start.Line,
                Selection = Helpers.FromRange(data.Range),
                ApplyTextChanges = false,
                WantsTextChanges = true,
                WantsAllCodeActionOperations = true
            };

            var omnisharpCaResponse = await _runActionHandler.Handle(omnisharpCaRequest);
            if (omnisharpCaResponse.Changes != null)
            {
                var edit = Helpers.ToWorkspaceEdit(
                    omnisharpCaResponse.Changes,
                    _server.ClientSettings.Capabilities.Workspace!.WorkspaceEdit.Value,
                    _documentVersions
                );

                return new CodeAction
                {
                    Edit = edit,
                };
            }
            else
            {
                return new CodeAction();
            }
        }
    }
}
