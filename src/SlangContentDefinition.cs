using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace SlangClient
{
    public class SlangContentDefinition
    {
        public const string SlangContentType = "slang";
        [Export]
        [Name("slang")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition SlangContentTypeDefinition = null;

        [Export]
        [FileExtension(".vfx")]
        [ContentType(SlangContentType)]
        internal static FileExtensionToContentTypeDefinition VfxFileExtensionDefinition = null;

        [Export]
        [FileExtension(".fxc")]
        [ContentType(SlangContentType)]
        internal static FileExtensionToContentTypeDefinition FxcFileExtensionDefinition = null;

        [Export]
        [FileExtension(".slang")]
        [ContentType(SlangContentType)]
        internal static FileExtensionToContentTypeDefinition SlangFileExtensionDefinition = null;

    }
}
