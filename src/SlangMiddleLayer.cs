using Microsoft.VisualStudio.Core.Imaging;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlangClient
{
    public class SlangMiddleLayer : ILanguageClientMiddleLayer
    {
        internal readonly static SlangMiddleLayer Instance = new SlangMiddleLayer();

        const string WorkspaceSemanticTokensRefresh = "workspace/semanticTokens/refresh";

        DateTime m_LastSemanticRequest;

        public bool CanHandle(string methodName)
        {
            switch( methodName )
            {
                case Methods.TextDocumentDidOpenName:
                case Methods.TextDocumentDidChangeName:
                case WorkspaceSemanticTokensRefresh:
                case Methods.TextDocumentPublishDiagnosticsName:
                case Methods.TextDocumentHoverName:
                case Methods.TextDocumentCompletionName:
                case Methods.TextDocumentCompletionResolveName:
                case Methods.TextDocumentDocumentSymbolName:
                case VSMethods.GetProjectContextsName:
                    return true;
            }
            return false;


        }

        public async Task RequestSemanticSymbolsAsync( Uri fileUri, bool bForceRequest = false )
        {
            if( !bForceRequest )
            {
                if( m_LastSemanticRequest == null  )
                {
                    m_LastSemanticRequest = DateTime.Now;
                }
                if( (DateTime.Now - m_LastSemanticRequest).TotalMilliseconds < 2000 )
                {
                    return;
                }
            }

            SemanticTokensParams semanticTokensParams = new SemanticTokensParams();
            semanticTokensParams.TextDocument = new TextDocumentIdentifier
            {
                Uri = fileUri
            };
            JToken token = await SlangLanguageClient.Instance._rpc.InvokeAsync<JToken>(Methods.TextDocumentSemanticTokensFullName, argument: semanticTokensParams);

            SemanticTokens result = token.ToObject<SemanticTokens>();

            SlangWorkspace.Instance.NotifySemanticTokensReady(Uri.UnescapeDataString(fileUri.AbsoluteUri), result);

            return;
        }

        public async Task HandleNotificationAsync(string methodName, JToken methodParam, Func<JToken, Task> sendNotification)
        {
            //Debug.WriteLine("%%%%%%%%%%%%%%%%%%%%");
            //Debug.WriteLine(methodName);
            //Debug.WriteLine("%%%%%%%%%%%%%%%%%%%%");

            switch( methodName )
            {
                case Methods.TextDocumentDidOpenName:
                    await sendNotification(methodParam);
                    DidOpenTextDocumentParams didOpenTextDocumentParams = methodParam.ToObject<DidOpenTextDocumentParams>(new Newtonsoft.Json.JsonSerializer() { MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore });
                    var configPath = SlangLanguageClient.Instance.GetNewConfigurationPath(didOpenTextDocumentParams.TextDocument.Uri);
                    if (configPath != null)
                    {
                        SlangLanguageClient.Instance.UpdateConfiguration(new Uri(configPath));
                    }
                    await RequestSemanticSymbolsAsync(didOpenTextDocumentParams.TextDocument.Uri);
                    break;
                case Methods.TextDocumentDidChangeName:
                    {
                        await sendNotification(methodParam);
                        DidChangeTextDocumentParams didChangeTextDocumentParams = methodParam.ToObject<DidChangeTextDocumentParams>(new Newtonsoft.Json.JsonSerializer() { MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore });

                        await RequestSemanticSymbolsAsync(didChangeTextDocumentParams.TextDocument.Uri);
                    }
                    break;
                case Methods.TextDocumentPublishDiagnosticsName:
                    PublishDiagnosticParams p = methodParam.ToObject<PublishDiagnosticParams>(new Newtonsoft.Json.JsonSerializer() { MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore });
                    foreach (var d in p.Diagnostics)
                    {
                        Debug.WriteLine(d.Range.ToString());
                    }
                    await sendNotification(methodParam);
                    if (SlangLanguageClient.Instance != null)
                    {
                        SlangLanguageClient.Instance.UpdateConfiguration(p.Uri);
                    }
                    break;
                case WorkspaceSemanticTokensRefresh:
                    {
                        List<string> visibleDocuments = SlangWorkspace.Instance.GetVisibleDocuments();

                        if (visibleDocuments.Any())
                        {
                            foreach( string path in visibleDocuments )
                            {
                                await RequestSemanticSymbolsAsync(new Uri(path));
                            }
                        }
                    }
                    break;
                case "NotificationReceived":
                    break;
                default:
                    await sendNotification(methodParam);
                    break;

            }
        }

        public async Task<JToken> HandleRequestAsync(string methodName, JToken methodParam, Func<JToken, Task<JToken>> sendRequest)
        {
            JToken token;
            switch (methodName)
            {
                case "$/cancelRequest":
                    {
                        return methodParam;
                    }
                case "NotificationReceived":
                    {
                        return methodParam;
                    }
                case "workspace/semanticTokens/refresh":
                    {
                        return methodParam;
                    }
                case Methods.TextDocumentCompletionName:
                    {
                        CompletionParams completionParams = methodParam.ToObject<CompletionParams>();
                        if(completionParams.Context.TriggerCharacter != null  
                            && completionParams.Context.TriggerCharacter.TrimStart() == string.Empty )
                        {
                            return JToken.FromObject(new CompletionItem());
                        }
                        token = await sendRequest(methodParam);
                        //Debug.WriteLine(token);
                        return token;
                    }
                case Methods.TextDocumentCompletionResolveName:
                    {
                        CompletionItem completionParams = methodParam.ToObject<CompletionItem>();
                        token = await sendRequest(methodParam);
                        //Debug.WriteLine(token);
                        return token;
                    }
                case Methods.TextDocumentHoverName:
                    {
                        TextDocumentPositionParams hoverParams = methodParam.ToObject<TextDocumentPositionParams>();
                        token = await sendRequest(methodParam);
                        Hover hover = token.ToObject<Hover>();

                        //Debug.WriteLine("^^^^^^^^^^^^^^^^^^^^^^");
                        //Debug.WriteLine( methodParam.ToString() );
                        //Debug.WriteLine("^^^^^^^^^^^^^^^^^^^^^^");
                        //Debug.WriteLine( token.ToString() );
                        //Debug.WriteLine("^^^^^^^^^^^^^^^^^^^^^^");
                        if (hover.Range != null)
                        {
                            ITextView textView = null;
                            SlangWorkspace.Instance.m_TextViewsDictionary.TryGetValue(Uri.UnescapeDataString(hoverParams.TextDocument.Uri.AbsoluteUri), out textView);

                            if (textView != null)
                            {
                                var snapshot = textView.TextBuffer.CurrentSnapshot;
                                ITextSnapshotLine startLine = snapshot.GetLineFromLineNumber(hover.Range.Start.Line);
                                
                                if(hover.Range.Start.Character < 0 || hover.Range.Start.Character >= startLine.Length )
                                {
                                    hover.Range = null;
                                    token = JToken.FromObject(hover);
                                    return token;
                                }
                                SnapshotPoint startPoint = new SnapshotPoint(snapshot, startLine.Start + hover.Range.Start.Character);
                                
                                ITextSnapshotLine endLine = snapshot.GetLineFromLineNumber(hover.Range.End.Line);
                                SnapshotPoint endPoint = new SnapshotPoint(snapshot, endLine.Start + hover.Range.End.Character);

                                if (hover.Range.End.Character < 0 ||  hover.Range.End.Character > endLine.Length)
                                {
                                    hover.Range.End = hover.Range.Start;
                                    token = JToken.FromObject(hover);
                                    return token;
                                }

                            }
                        }
                        return token;
                    }
                case Methods.TextDocumentDocumentSymbolName:
                    {
                        var symParmas = methodParam.ToObject<DocumentSymbolParams>();
                        token = await sendRequest(methodParam);
                        DocumentSymbol[] symbols = token.ToObject<DocumentSymbol[]>();
                        List<VSSymbolInformation> flattendSymbols = new List<VSSymbolInformation>();
                        foreach (var symbol in symbols)
                            GetVSSymbolInfoArray(flattendSymbols, "", symParmas.TextDocument.Uri, symbol);
                        var resultToken = JToken.FromObject(flattendSymbols.ToArray());
                        return resultToken;
                    }
                case VSMethods.GetProjectContextsName:
                    {
                        var result = new VSProjectContextList();
                        result.ProjectContexts = new VSProjectContext[1] {
                            new VSProjectContext()
                            {
                                Id = "SlangProject",
                                Label = "(Slang Module)",
                                Kind = VSProjectKind.CPlusPlus
                            }
                        };
                        result.DefaultIndex = 0;
                        return JToken.FromObject(result);
                    }

            }

            token = await sendRequest(methodParam);
            return token;
        }

        void GetVSSymbolInfoArray(List<VSSymbolInformation> list, String parentName, Uri uri, DocumentSymbol symbol)
        {
            VSSymbolInformation vsSymbol = new VSSymbolInformation();
            StringBuilder nameSB = new StringBuilder();
            nameSB.Append(parentName);
            if (nameSB.Length > 0)
                nameSB.Append(".");
            nameSB.Append(symbol.Name);
            vsSymbol.Name = nameSB.ToString();
            vsSymbol.Kind = symbol.Kind;
            vsSymbol.Location = new Location();
            vsSymbol.Location.Uri = uri;
            vsSymbol.Location.Range = symbol.Range;
            vsSymbol.Description = symbol.Detail;
            switch (symbol.Kind)
            {
                case SymbolKind.Function:
                case SymbolKind.Property:
                case SymbolKind.Variable:
                case SymbolKind.Field:
                case SymbolKind.Method:
                case SymbolKind.EnumMember:
                    if (parentName.Length == 0)
                        vsSymbol.ContainerName = "(Global Scope)";
                    else
                        vsSymbol.ContainerName = parentName;
                    vsSymbol.Name = symbol.Name;
                    break;
                default:
                    vsSymbol.ContainerName = vsSymbol.Name;
                    ImageId icon = new ImageId();
                    switch (vsSymbol.Kind)
                    {
                    case SymbolKind.Struct:
                        icon = KnownMonikers.Structure.ToImageId();
                        break;
                    case SymbolKind.Namespace:
                        icon = KnownMonikers.Namespace.ToImageId();
                        break;
                    case SymbolKind.Class:
                        icon = KnownMonikers.Class.ToImageId();
                        break;
                    case SymbolKind.Enum:
                        icon = KnownMonikers.Enumeration.ToImageId();
                        break;
                    }
                    vsSymbol.Icon = new VSImageId();
                    vsSymbol.Icon.Guid = icon.Guid;
                    vsSymbol.Icon.Id = icon.Id;
                    break;
            }
            list.Add(vsSymbol);
            if (symbol.Children != null)
            {
                foreach (var child in symbol.Children)
                    GetVSSymbolInfoArray(list, vsSymbol.Name, uri, child);
            }
        }
    }
}
