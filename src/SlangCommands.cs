using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace SlangClient
{
    static public class CommentOptions
    {
        public const string SingleLineCommentString = "//";
        public const string BlockCommentStartString = "/*";
        public const string BlockCommentEndString = "*/";
    }

    [Export(typeof(ICommandHandler))]
    [Name(nameof(CommentSelection))]
    [ContentType(SlangContentDefinition.SlangContentType)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class CommentSelection : ICommandHandler<CommentSelectionCommandArgs>
    {
        
        public string DisplayName => nameof(CommentSelection);

        public bool ExecuteCommand(CommentSelectionCommandArgs args, CommandExecutionContext executionContext)
        {
            return CommentHelper.CommentOrUncommentBlock(args.TextView, comment: true);
        }

        public CommandState GetCommandState(CommentSelectionCommandArgs args)
        {
            return CommandState.Available;
        }
    }

    [Export(typeof(ICommandHandler))]
    [Name(nameof(UncommentSelection))]
    [ContentType(SlangContentDefinition.SlangContentType)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]

    internal class UncommentSelection : ICommandHandler<UncommentSelectionCommandArgs>
    {
        public string DisplayName => nameof(UncommentSelection);

        public bool ExecuteCommand(UncommentSelectionCommandArgs args, CommandExecutionContext executionContext)
        {
            return CommentHelper.CommentOrUncommentBlock(args.TextView, comment: false);
        }

        public CommandState GetCommandState(UncommentSelectionCommandArgs args)
        {
            return CommandState.Available;
        }
    }
 }
