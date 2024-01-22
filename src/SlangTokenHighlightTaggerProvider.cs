using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;

namespace SlangClient
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IClassificationTag))]
    [ContentType("slang")]
    public class SlangTokenHighlighTaggerProvider : IViewTaggerProvider, IDisposable
    {
        [Import]
        private IClassificationTypeRegistryService classificationService = null;

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            return new SlangTokenHighlightTagger(textView, classificationService) as ITagger<T>;
        }

        public void Dispose()
        {

        }
    }
}
