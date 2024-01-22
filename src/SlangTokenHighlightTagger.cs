using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq.Expressions;
using System.Windows.Media;

namespace SlangClient
{
    public class SlangTokenTag : ITag
    {
        public string type;

        public SlangTokenTag( string t )
        {
            type = t;
        }
    }
    public class SlangTokenHighlightTagger : ITagger<IClassificationTag>, IDisposable
    {
        ITextView m_View;
        bool m_bRegisterPending = true;
        SemanticTokens m_SemanticTokens;
        IClassificationTypeRegistryService m_ClassificationService;

        public SlangTokenHighlightTagger(ITextView view, IClassificationTypeRegistryService service)
        {
            m_ClassificationService = service;
            m_View = view;
            RegisterInWorkspace();
        }

        void RegisterInWorkspace()
        {
            if (m_bRegisterPending)
            {
                if (SlangWorkspace.Instance != null)
                {
                    SlangWorkspace.Instance.RegisterHighlight(m_View.TextBuffer, this);
                    m_bRegisterPending = false;
                }
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        public IEnumerable<ITagSpan<IClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            RegisterInWorkspace();
            if (m_bRegisterPending || m_SemanticTokens == null)
                yield break;

            foreach (var span in spans)
            {
                ITextSnapshot snapshot = spans[0].Snapshot;
                ITextBuffer textBuffer = snapshot.TextBuffer;
                
                int iLine = 0;
                int nStartChar = 0;
                for (int idx = 0; idx < m_SemanticTokens.Data.Length; idx += 5)
                {
                    var sp = new SnapshotSpan();

                    int nTokenType = 0;
                    try
                    {
                        int iOldLine = iLine;
                        iLine += m_SemanticTokens.Data[idx + 0];
                        if (iOldLine != iLine)
                            nStartChar = 0;
                        nStartChar += m_SemanticTokens.Data[idx + 1];
                        int nLength = m_SemanticTokens.Data[idx + 2];
                        nTokenType = m_SemanticTokens.Data[idx + 3];
                        int nTokenModifier = m_SemanticTokens.Data[idx + 4];

                        ITextSnapshotLine startLine = snapshot.GetLineFromLineNumber(iLine);
                        SnapshotPoint startPoint = new SnapshotPoint(snapshot, startLine.Start + nStartChar);

                        SnapshotPoint endPoint = new SnapshotPoint(snapshot, startLine.Start + nStartChar + nLength);
                        sp = new SnapshotSpan(startPoint, endPoint);
                    }
                    catch 
                    {
                        yield break;
                    }
                    
                    if (span.IntersectsWith(sp.Span))
                    {
                        IClassificationType classificationType = m_ClassificationService.GetClassificationType(PredefinedClassificationTypeNames.Keyword);
                        switch (nTokenType)
                        {
                            // type
                            case 0:
                                classificationType = m_ClassificationService.GetClassificationType(SlangClassificationDefintions.Slang_Keyword);
                                break;
                            // enumMember
                            case 1:
                                classificationType = m_ClassificationService.GetClassificationType(SlangClassificationDefintions.Slang_EnumMember);
                                break;
                            // Variable
                            case 2:
                                classificationType = m_ClassificationService.GetClassificationType(SlangClassificationDefintions.Slang_Variable);
                                break;
                            // Parameter
                            case 3:
                                classificationType = m_ClassificationService.GetClassificationType(SlangClassificationDefintions.Slang_Parameter);
                                break;
                            // Function
                            case 4:
                                classificationType = m_ClassificationService.GetClassificationType(SlangClassificationDefintions.Slang_Function);
                                break;
                            // Property
                            case 5:
                                classificationType = m_ClassificationService.GetClassificationType(SlangClassificationDefintions.Slang_Property);
                                break;
                            // Namespace
                            case 6:
                                classificationType = m_ClassificationService.GetClassificationType(SlangClassificationDefintions.Slang_Namespace);
                                break;
                            // keyword
                            case 7:
                                classificationType = m_ClassificationService.GetClassificationType(SlangClassificationDefintions.Slang_Keyword);
                                break;
                            // Macro
                            case 8:
                                classificationType = m_ClassificationService.GetClassificationType(SlangClassificationDefintions.Slang_Macro);
                                break;
                            // string
                            case 9:
                                classificationType = m_ClassificationService.GetClassificationType(SlangClassificationDefintions.Slang_String);
                                break;

                        }

                        var classificationTag = new ClassificationTag(classificationType);
                        yield return new TagSpan<IClassificationTag>(sp, classificationTag);

                    }
                }
            }
        }

        public void RefreshSymbols(SemanticTokens _semanticTokens)
        {
            RegisterInWorkspace();
            if (!m_bRegisterPending)
            {
                m_SemanticTokens = _semanticTokens;
                TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(m_View.TextBuffer.CurrentSnapshot, new Span(0, m_View.TextBuffer.CurrentSnapshot.Length))));
            }
        }

        public void Dispose()
        {
            SlangWorkspace.Instance?.UnRegisterHighlight(m_View.TextBuffer, this);
        }
    }
}

