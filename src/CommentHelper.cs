using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Diagnostics;
using System.Linq;

namespace SlangClient
{
    public static class CommentHelper
    {
        enum eCommentType
        {
            None,
            SingleLine,
            Block,
        }

        public static bool CommentOrUncommentBlock(ITextView view, bool comment)
        {
            SnapshotPoint start, end;
            SnapshotPoint? mappedStart, mappedEnd;

            if (view.Selection.IsActive && !view.Selection.IsEmpty)
            {
                // comment every line in the selection
                start = view.Selection.Start.Position;
                end = view.Selection.End.Position;
                mappedStart = MapPoint(view, start);

                var endLine = end.GetContainingLine();
                if (endLine.Start == end)
                {
                    // http://pytools.codeplex.com/workitem/814
                    // User selected one extra line, but no text on that line.  So let's
                    // back it up to the previous line.  It's impossible that we're on the
                    // 1st line here because we have a selection, and we end at the start of
                    // a line.  In normal selection this is only possible if we wrapped onto the
                    // 2nd line, and it's impossible to have a box selection with a single line.
                    end = end.Snapshot.GetLineFromLineNumber(endLine.LineNumber - 1).End;
                }

                mappedEnd = MapPoint(view, end);
            }
            else
            {
                // comment the current line
                start = end = view.Caret.Position.BufferPosition;
                mappedStart = mappedEnd = MapPoint(view, start);
            }

            if (mappedStart != null && mappedEnd != null &&
                mappedStart.Value <= mappedEnd.Value)
            {
                eCommentType commentType = eCommentType.None;
                if (comment)
                {
                    commentType = CommentRegion(view, mappedStart.Value, mappedEnd.Value);
                }
                else
                {
                    commentType = UncommentRegion(view, mappedStart.Value, mappedEnd.Value);
                }

                // select multiple spans?
                // Select the full region we just commented, do not select if in projection buffer
                // (the selection might span non-language buffer regions)
                if (view.TextBuffer.IsSlangContent())
                {
                    UpdateSelection(view, start, end, commentType);
                }

                return true;
            }

            return false;
        }

        private static bool IsSlangContent(this ITextBuffer buffer)
        {
            return buffer.ContentType.IsOfType(SlangContentDefinition.SlangContentType);
        }

        private static bool IsSlangContent(this ITextSnapshot buffer)
        {
            return buffer.ContentType.IsOfType(SlangContentDefinition.SlangContentType);
        }

        private static SnapshotPoint? MapPoint(ITextView view, SnapshotPoint point)
        {
            return view.BufferGraph.MapDownToFirstMatch(
               point,
               PointTrackingMode.Positive,
               IsSlangContent,
               PositionAffinity.Successor);
        }

        private static eCommentType CommentRegion(ITextView view, SnapshotPoint start, SnapshotPoint end)
        {
            var snapshot = start.Snapshot;
            eCommentType commentType = eCommentType.None;
            using (var edit = snapshot.TextBuffer.CreateEdit())
            {
                if (!SpanIncludesAllTextOnIncludedLines(view, start, end) && IsNonEmptyBlock(view, start, end) )
                {
                    commentType = eCommentType.Block;
                    edit.Insert(start.Position, CommentOptions.BlockCommentStartString);
                    edit.Insert(end.Position, CommentOptions.BlockCommentEndString);
                }
                else
                {
                    commentType = eCommentType.SingleLine;
                    int minColumn = int.MaxValue;

                    // first pass, determine the position to place the comment
                    for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++)
                    {
                        var curLine = snapshot.GetLineFromLineNumber(i);
                        var text = curLine.GetText();

                        int firstNonWhitespace = IndexOfNonWhitespaceCharacter(text);
                        if (firstNonWhitespace >= 0 && firstNonWhitespace < minColumn)
                        {
                            // ignore blank lines
                            minColumn = firstNonWhitespace;
                        }
                    }

                    // second pass, place the comment
                    for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++)
                    {
                        var curLine = snapshot.GetLineFromLineNumber(i);
                        if (string.IsNullOrWhiteSpace(curLine.GetText()))
                        {
                            continue;
                        }

                        edit.Insert(curLine.Start.Position + minColumn, CommentOptions.SingleLineCommentString);
                    }
                }
                edit.Apply();
                return commentType;
            }
        }
        private static bool IsNonEmptyBlock(ITextView view, SnapshotPoint start, SnapshotPoint end)
        {
            if (start.GetContainingLine().LineNumber == end.GetContainingLine().LineNumber)
            {
                var text = view.Selection.SelectedSpans.FirstOrDefault().GetText();
                int firstNonWhitespace = IndexOfNonWhitespaceCharacter(text);
                if (firstNonWhitespace >= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private static int IndexOfNonWhitespaceCharacter(string text)
        {
            for (int j = 0; j < text.Length; j++)
            {
                if (!char.IsWhiteSpace(text[j]))
                {
                    return j;
                }
            }

            return -1;
        }

        /// <summary>
        /// Removes a comment markers (//) from the start of each line.  If there is a selection the character is
        /// removed from each selected line.  Otherwise the character is removed from the current line.  Uncommented
        /// lines are ignored.
        /// </summary>
        private static eCommentType UncommentRegion(ITextView view, SnapshotPoint start, SnapshotPoint end)
        {
            Debug.Assert(start.Snapshot == end.Snapshot, "???");
            var snapshot = start.Snapshot;
            eCommentType commentType = eCommentType.None;
            using (var edit = snapshot.TextBuffer.CreateEdit())
            {
                if (!SpanIncludesAllTextOnIncludedLines(view, start, end) && start.GetContainingLine().LineNumber == end.GetContainingLine().LineNumber)
                {
                    var span = view.Selection.SelectedSpans.FirstOrDefault();
                    var positionOfStart = -1;
                    var positionOfEnd = -1;
                    var spanText = span.GetText();
                    var trimmedSpanText = spanText.Trim();

                    // See if the selection includes just a block comment (plus whitespace)
                    if (trimmedSpanText.StartsWith(CommentOptions.BlockCommentStartString, StringComparison.Ordinal) && trimmedSpanText.EndsWith(CommentOptions.BlockCommentEndString, StringComparison.Ordinal))
                    {
                        positionOfStart = span.Start + spanText.IndexOf(CommentOptions.BlockCommentStartString, StringComparison.Ordinal);
                        positionOfEnd = span.Start + spanText.LastIndexOf(CommentOptions.BlockCommentEndString, StringComparison.Ordinal);
                    }
                    else
                    {
                        // See if we are (textually) contained in a block comment.
                        // This could allow a selection that spans multiple block comments to uncomment the beginning of
                        // the first and end of the last.  Oh well.
                        var text = span.Snapshot.GetText();
                        positionOfStart = text.LastIndexOf(CommentOptions.BlockCommentStartString, span.Start);

                        // If we found a start comment marker, make sure there isn't an end comment marker after it but before our span.
                        if (positionOfStart >= 0)
                        {
                            var lastEnd = text.LastIndexOf(CommentOptions.BlockCommentEndString, span.Start);
                            if (lastEnd < positionOfStart)
                            {
                                positionOfEnd = text.IndexOf(CommentOptions.BlockCommentEndString, span.End);
                            }
                            else if (lastEnd + CommentOptions.BlockCommentEndString.Length > span.End)
                            {
                                // The end of the span is *inside* the end marker, so searching backwards found it.
                                positionOfEnd = lastEnd;
                            }
                        }
                    }

                    if (positionOfStart >= 0 || positionOfEnd >= 0)
                    {
                        commentType = eCommentType.Block;
                        edit.Delete(positionOfStart, CommentOptions.BlockCommentStartString.Length);
                        edit.Delete(positionOfEnd, CommentOptions.BlockCommentStartString.Length);
                    }
                }
                else
                {
                    commentType = eCommentType.SingleLine;
                    // first pass, determine the position to place the comment
                    for (int i = start.GetContainingLine().LineNumber; i <= end.GetContainingLine().LineNumber; i++)
                    {
                        var curLine = snapshot.GetLineFromLineNumber(i);

                        DeleteFirstCommentChar(edit, curLine);
                    }
                }
                edit.Apply();
                return commentType;
            }
        }

        private static void UpdateSelection(ITextView view, SnapshotPoint start, SnapshotPoint end, eCommentType commentType)
        {
            if (commentType == eCommentType.Block)
            {
                view.Selection.Select(
                    new SnapshotSpan(
                    start.TranslateTo( view.TextBuffer.CurrentSnapshot, PointTrackingMode.Negative ),
                    end.TranslateTo(view.TextBuffer.CurrentSnapshot, PointTrackingMode.Positive )),
                    false);
            }
            else
            {
                view.Selection.Select(
                    new SnapshotSpan(
                        start.GetContainingLine().Start.TranslateTo(view.TextBuffer.CurrentSnapshot, PointTrackingMode.Negative),
                        end.GetContainingLine().End.TranslateTo(view.TextBuffer.CurrentSnapshot, PointTrackingMode.Positive)),
                    false);
            }
        }

        private static void DeleteFirstCommentChar(ITextEdit edit, ITextSnapshotLine curLine)
        {
            var text = curLine.GetText();
            for (int j = 0; j < text.Length; j++)
            {
                if (char.IsWhiteSpace(text[j]))
                {
                    continue;
                }

                if (string.Concat(text.Skip(j).Take(CommentOptions.SingleLineCommentString.Length)) == CommentOptions.SingleLineCommentString)
                {
                    edit.Delete(curLine.Start.Position + j, CommentOptions.SingleLineCommentString.Length);
                }

                break;
            }
        }

        public static int? GetFirstNonWhitespacePosition(ITextSnapshotLine line)
        {
            var text = line.GetText();

            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return line.Start + i;
                }
            }

            return null;
        }
        public static int? GetLastNonWhitespacePosition(ITextSnapshotLine line)
        {
            var text = line.GetText();

            for (int i = text.Length - 1; i >= 0; i--)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return line.Start + i;
                }
            }

            return null;
        }


        private static bool SpanIncludesAllTextOnIncludedLines(ITextView textView, SnapshotPoint start, SnapshotPoint end)
        {
            ITextSnapshotLine firstLine = start.GetContainingLine();
            ITextSnapshotLine lastLine = end.GetContainingLine();

            var firstNonWhitespacePosition = GetFirstNonWhitespacePosition(firstLine);
            var lastNonWhitespacePosition = GetLastNonWhitespacePosition(lastLine);

            var allOnFirst = !firstNonWhitespacePosition.HasValue ||
                             textView.Selection.Start.Position <= firstNonWhitespacePosition.Value;
            var allOnLast = !lastNonWhitespacePosition.HasValue ||
                            textView.Selection.End.Position > lastNonWhitespacePosition.Value;

            return allOnFirst && allOnLast;
        }

    }

}
