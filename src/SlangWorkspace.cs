using Microsoft.VisualStudio;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SlangClient
{
    [Export(typeof(ITextViewCreationListener))]
    [ContentType("slang")]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    public class SlangWorkspace : ITextViewCreationListener, IDisposable
    {
        ITextDocumentFactoryService m_DocumentFactoryService { get; set; }
        SVsServiceProvider m_ServiceProvider { get; set; }

        public ConcurrentDictionary<string, ITextView> m_TextViewsDictionary = new ConcurrentDictionary<string, ITextView>(Environment.ProcessorCount * 2, 32);
        public ConcurrentDictionary<string, SlangSquiggleTagger> m_DiagnosticTaggersDictionary = new ConcurrentDictionary<string, SlangSquiggleTagger>(Environment.ProcessorCount * 2, 32);
        public ConcurrentDictionary<string, SlangTokenHighlightTagger> m_HighlightTaggersDictionary = new ConcurrentDictionary<string, SlangTokenHighlightTagger>(Environment.ProcessorCount * 2, 32);
        public ConcurrentDictionary<string, SemanticTokens> m_SemanticTokensDictionary = new ConcurrentDictionary<string, SemanticTokens>(Environment.ProcessorCount * 2, 32);

        public static SlangWorkspace Instance = null;

        public delegate void DiagnosticsReadyEvent(string path);

        [ImportingConstructor]
        SlangWorkspace(ITextDocumentFactoryService documentFactory, SVsServiceProvider serviceProvider)
        {
            Instance = this;
            m_DocumentFactoryService = documentFactory;
            m_ServiceProvider = serviceProvider;
        }

        [Export]
        public async Task InitializeAsync(CancellationToken token)
        {
            await DoSettingsAsync();
        }
        public async Task DoSettingsAsync()
        {
            CancellationToken token = CancellationToken.None;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(token);
        }

        public string GetPathFromTextBuffer(ITextBuffer textBuffer)
        {
            ITextDocument textDocument;
            if (m_DocumentFactoryService.TryGetTextDocument(textBuffer, out textDocument))
            {
                Uri fileUri;
                if (Uri.TryCreate(textDocument.FilePath, UriKind.Absolute, out fileUri))
                {
                    return Uri.UnescapeDataString(fileUri.AbsoluteUri);
                }
            }
            return null;
        }

        public void TextViewCreated(ITextView textView)
        {
            string path = GetPathFromTextBuffer(textView.TextBuffer);
            m_TextViewsDictionary.TryAdd(path, textView);
            textView.Closed += TextView_Closed;
        }

        public List<string> GetVisibleDocuments()
        {
            List<string> names = new List<string>();
            foreach (var it in m_TextViewsDictionary)
            {
                ITextViewLineCollection col = it.Value.TextViewLines;
                if (col != null)
                {
                    if (col.Any())
                    {
                        names.Add(it.Key);
                    }
                }
            }
            return names;
        }

        public void Dispose()
        {
            m_TextViewsDictionary.Clear();
            Instance = null;
        }

        private void TextView_Closed(object sender, EventArgs e)
        {
            ITextView view = (ITextView)sender;
            view.Closed -= TextView_Closed;
            var item = m_TextViewsDictionary.First(kvp => kvp.Value == view);

            ITextView removeView;
            m_TextViewsDictionary.TryRemove(item.Key, out removeView);
        }

        public void RegisterTagger(ITextBuffer textBuffer, SlangSquiggleTagger tagger)
        {
            string path = GetPathFromTextBuffer(textBuffer);
            if (m_DiagnosticTaggersDictionary.ContainsKey(path))
            {
                
            }
            else
            {
                m_DiagnosticTaggersDictionary.TryAdd(path, tagger);
            }
        }

        public void UnRegisterTagger(ITextBuffer textBuffer, SlangSquiggleTagger tagger)
        {
            string path = GetPathFromTextBuffer(textBuffer);
            m_DiagnosticTaggersDictionary.TryRemove(path, out var taggerout);
        }

        public void RegisterHighlight(ITextBuffer textBuffer, SlangTokenHighlightTagger tagger)
        {
            string path = GetPathFromTextBuffer(textBuffer);
            if (m_HighlightTaggersDictionary.ContainsKey(path))
            {
                
            }
            else
            {
                m_HighlightTaggersDictionary.TryAdd(path, tagger);
            }
        }

        public void UnRegisterHighlight(ITextBuffer textBuffer, SlangTokenHighlightTagger tagger)
        {
            string path = GetPathFromTextBuffer(textBuffer);
            m_HighlightTaggersDictionary.TryRemove(path, out var taggerout);
        }


        public void NotifyDiagnosticsReady(PublishDiagnosticParams diagnosticsParams)
        {
            string path = Uri.UnescapeDataString(diagnosticsParams.Uri.AbsoluteUri);

            if (m_DiagnosticTaggersDictionary.TryGetValue(path, out var tagger))
            {
                tagger.RefreshDiagnostics(diagnosticsParams);
            }
        }

        public void NotifySymbolsReady(string path, SemanticTokens semanticTokens)
        {
            //string path = Uri.UnescapeDataString(symbolsParams.TextDocument.Uri.AbsoluteUri);

            if (m_HighlightTaggersDictionary.TryGetValue(path, out var tagger))
            {
                tagger.RefreshSymbols(semanticTokens);
            }
        }

        public void NotifySemanticTokensReady(string path, SemanticTokens semanticTokens)
        {
            m_SemanticTokensDictionary.AddOrUpdate(path, semanticTokens, (key, oldValue) => semanticTokens);
            if (m_HighlightTaggersDictionary.TryGetValue(path, out var tagger))
            {
                tagger.RefreshSymbols(semanticTokens);
            }

        }

    }
}

