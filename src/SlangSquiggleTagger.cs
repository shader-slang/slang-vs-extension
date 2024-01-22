using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;

namespace SlangClient
{
    public class SlangSquiggleTagger : ITagger<ErrorTag>, IDisposable
    {
        ITextBuffer m_SourceBuffer { get; set; }
        ITagAggregator<ErrorTag> m_TagAggregator { get; set; }

        ITextStructureNavigator m_TextStructureNavigator;

        public static SlangSquiggleTagger Instance = null;

        public PublishDiagnosticParams diagnosticParams;
        public SlangSquiggleTagger(ITextBuffer sourceBuffer, ITagAggregator<ErrorTag> tagAggregator, ITextStructureNavigator textStructureNavigator)
        {
            this.m_SourceBuffer = sourceBuffer;
            this.m_TagAggregator = tagAggregator;
            m_TextStructureNavigator = textStructureNavigator;

            this.m_SourceBuffer.Changed += BufferChanged;

            Debug.Assert(SlangWorkspace.Instance != null);

            SlangWorkspace.Instance.RegisterTagger(sourceBuffer, this);
        }


        public void RefreshDiagnostics(PublishDiagnosticParams diagnotics)
        {
            diagnosticParams = diagnotics;
            TagsChanged?.Invoke(this, new SnapshotSpanEventArgs(new SnapshotSpan(m_SourceBuffer.CurrentSnapshot, 0, m_SourceBuffer.CurrentSnapshot.Length)));
        }

        public void Dispose()
        {
            SlangWorkspace.Instance.UnRegisterTagger(m_SourceBuffer, this);
        }
        static bool WordExtentIsValid(SnapshotPoint currentRequest, TextExtent word)
        {
            return word.IsSignificant
                && currentRequest.Snapshot.GetText(word.Span).Any(c => char.IsLetter(c));
        }

        public IEnumerable<ITagSpan<ErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count > 0 && diagnosticParams != null)
            {
                ITextSnapshot snapshot = spans[0].Snapshot;
                ITextBuffer textBuffer = snapshot.TextBuffer;

                foreach (var span in spans)
                {
                    foreach (Diagnostic d in diagnosticParams.Diagnostics)
                    {
                        ITextSnapshotLine startLine = snapshot.GetLineFromLineNumber(d.Range.Start.Line);
                        SnapshotPoint startPoint = new SnapshotPoint(snapshot, startLine.Start + d.Range.Start.Character);

                        TextExtent word = m_TextStructureNavigator.GetExtentOfWord(startPoint);

                        bool foundWord = true;
                        //If we've selected something not worth highlighting, we might have missed a "word" by a little bit
                        if (!WordExtentIsValid(startPoint, word))
                        {
                            //Before we retry, make sure it is worthwhile 
                            if (word.Span.Start != startPoint
                                 || startPoint == startPoint.GetContainingLine().Start
                                 || char.IsWhiteSpace((startPoint - 1).GetChar()))
                            {
                                foundWord = false;
                            }
                            else
                            {
                                // Try again, one character previous.  
                                //If the caret is at the end of a word, pick up the word.
                                word = m_TextStructureNavigator.GetExtentOfWord(startPoint - 1);

                                //If the word still isn't valid, we're done 
                                if (!WordExtentIsValid(startPoint, word))
                                    foundWord = false;
                            }
                        }

                        if (foundWord)
                        {
                            if (span.IntersectsWith(word.Span))
                            {
                                yield return new TagSpan<ErrorTag>(word.Span, new ErrorTag("PredefinedErrorTypeNames.SyntaxError", d.Message));
                            }
                        }
                        else
                        {
                            ITextSnapshotLine endLine = snapshot.GetLineFromLineNumber(d.Range.End.Line);
                            SnapshotPoint endPoint = new SnapshotPoint(snapshot, endLine.Start + d.Range.End.Character);

                            SnapshotSpan sp = new SnapshotSpan(startPoint, endPoint);
                            yield return new TagSpan<ErrorTag>(sp, new ErrorTag("PredefinedErrorTypeNames.SyntaxError", d.Message));
                        }
                    }
                }
            }
        }

        void BufferChanged(object sender, TextContentChangedEventArgs e)
        {
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
    }
}

