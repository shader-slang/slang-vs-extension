using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.Composition;

namespace SlangClient
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType("slang")]
    public class SlangSquiggleTaggerProvider : ITaggerProvider, IDisposable
    {
        [Import]
        private IBufferTagAggregatorFactoryService m_TagAggregatorFactory { get; set; }
        [Import]
        internal ITextStructureNavigatorSelectorService m_TextStructureNavigatorSelector { get; set; }

        private readonly ConcurrentDictionary<ITextBuffer, SlangSquiggleTagger> m_TaggerCache = new ConcurrentDictionary<ITextBuffer, SlangSquiggleTagger>();

        public SlangSquiggleTaggerProvider()
        {
        }

        public void Dispose()
        {
            m_TaggerCache.Clear();
        }

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }


            if (m_TaggerCache.TryGetValue(buffer, out var tagger))
            {
                return tagger as ITagger<T>;
            }

            ITagAggregator<ErrorTag> tagAggregator = m_TagAggregatorFactory.CreateTagAggregator<ErrorTag>(buffer);

            ITextStructureNavigator textStructureNavigator =
                m_TextStructureNavigatorSelector.GetTextStructureNavigator(buffer);

            var newTagger = new SlangSquiggleTagger(buffer, tagAggregator, textStructureNavigator);

            m_TaggerCache.TryAdd(buffer, newTagger);

            return newTagger as ITagger<T>;
        }
    }
}
