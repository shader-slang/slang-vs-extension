using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace SlangClient
{
    public class SlangContentDefinition
    {
        [Export]
        [Name("slang")]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        internal static ContentTypeDefinition SlangContentTypeDefinition = null;

        [Export]
        [FileExtension(".vfx")]
        [ContentType("slang")]
        internal static FileExtensionToContentTypeDefinition VfxFileExtensionDefinition = null;

        [Export]
        [FileExtension(".fxc")]
        [ContentType("slang")]
        internal static FileExtensionToContentTypeDefinition FxcFileExtensionDefinition = null;

        [Export]
        [FileExtension(".slang")]
        [ContentType("slang")]
        internal static FileExtensionToContentTypeDefinition SlangFileExtensionDefinition = null;

    }
}
