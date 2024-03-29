﻿using EnvDTE;
using Microsoft.Build.Framework.XamlTypes;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.ComponentModel.Composition;
using System.Windows.Media;

namespace SlangClient
{
    internal sealed class SlangClassificationDefintions
    {
        public const string Slang_Type = "Slang Type";
        public const string Slang_EnumMember = "Slang EnumMember";
        public const string Slang_Variable = "Slang Variable";
        public const string Slang_Parameter = "Slang Parameter";
        public const string Slang_Function = "Slang Function";
        public const string Slang_Property = "Slang Property";
        public const string Slang_Namespace = "Slang Namespace";
        public const string Slang_Keyword = "Slang Keyword";
        public const string Slang_Macro = "Slang Macro";
    }
    #region SlangKeyword
    internal static class SlangKeywordClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(SlangClassificationDefintions.Slang_Keyword)]
        internal static ClassificationTypeDefinition SlangKeyword = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = SlangClassificationDefintions.Slang_Keyword)]
    [Name(SlangClassificationDefintions.Slang_Keyword)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    [BaseDefinition(PredefinedClassificationTypeNames.Keyword)]
    internal sealed class SlangKeyword : ClassificationFormatDefinition
    {
        public SlangKeyword()
        {
            DisplayName = SlangClassificationDefintions.Slang_Keyword;
            //ForegroundColor = Colors.Gold;
        }
    }
    #endregion

    #region SlangType
    internal static class SlangTypeClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(SlangClassificationDefintions.Slang_Type)]
        internal static ClassificationTypeDefinition SlangType = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = SlangClassificationDefintions.Slang_Type)]
    [Name(SlangClassificationDefintions.Slang_Type)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    [BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
    internal sealed class SlangType : ClassificationFormatDefinition
    {
        public SlangType()
        {
            DisplayName = SlangClassificationDefintions.Slang_Type;
            //ForegroundColor = Colors.Pink;
        }
    }
    #endregion

    #region SlangEnumMember
    internal static class SlangEnumMemberClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(SlangClassificationDefintions.Slang_EnumMember)]
        internal static ClassificationTypeDefinition SlangEnumMember = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = SlangClassificationDefintions.Slang_EnumMember)]
    [Name(SlangClassificationDefintions.Slang_EnumMember)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    [BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
    internal sealed class SlangEnumMember : ClassificationFormatDefinition
    {
        public SlangEnumMember()
        {
            DisplayName = SlangClassificationDefintions.Slang_EnumMember;
            //ForegroundColor = System.Windows.Media.Color.FromRgb( 0xb9, 0x77, 0x1e);
        }
    }
    #endregion

    #region SlangVariable
    internal static class SlangVariableClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(SlangClassificationDefintions.Slang_Variable)]
        internal static ClassificationTypeDefinition SlangVariable = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = SlangClassificationDefintions.Slang_Variable)]
    [Name(SlangClassificationDefintions.Slang_Variable)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    [BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
    internal sealed class SlangVariable : ClassificationFormatDefinition
    {
        public SlangVariable()
        {
            DisplayName = SlangClassificationDefintions.Slang_Variable;
            //ForegroundColor = Colors.DarkKhaki;
        }
    }
    #endregion

    #region SlangParameter
    internal static class SlangParameterClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(SlangClassificationDefintions.Slang_Parameter)]
        internal static ClassificationTypeDefinition SlangParameter = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = SlangClassificationDefintions.Slang_Parameter)]
    [Name(SlangClassificationDefintions.Slang_Parameter)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    [BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
    internal sealed class SlangParameter : ClassificationFormatDefinition
    {
        public SlangParameter()
        {
            DisplayName = SlangClassificationDefintions.Slang_Parameter;
        }
    }
    #endregion

    #region SlangFunction
    internal static class SlangFunctionClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(SlangClassificationDefintions.Slang_Function)]
        internal static ClassificationTypeDefinition SlangFunction = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = SlangClassificationDefintions.Slang_Function)]
    [Name(SlangClassificationDefintions.Slang_Function)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    [BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
    internal sealed class SlangFunction : ClassificationFormatDefinition
    {
        public SlangFunction()
        {
            DisplayName = SlangClassificationDefintions.Slang_Function;
            //ForegroundColor = Color.FromRgb( 0xff, 0x80, 0 );
        }
    }
    #endregion

    #region SlangProperty
    internal static class SlangPropertyClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(SlangClassificationDefintions.Slang_Property)]
        internal static ClassificationTypeDefinition SlangProperty = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = SlangClassificationDefintions.Slang_Property)]
    [Name(SlangClassificationDefintions.Slang_Property)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class SlangProperty : ClassificationFormatDefinition
    {
        public SlangProperty()
        {
            DisplayName = SlangClassificationDefintions.Slang_Property;
            //ForegroundColor = Color.FromRgb( 0xff, 0x80, 0 );
        }
    }
    #endregion

    #region SlangNamespace
    internal static class SlangNamespaceClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(SlangClassificationDefintions.Slang_Namespace)]
        internal static ClassificationTypeDefinition SlangNamespace = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = SlangClassificationDefintions.Slang_Namespace)]
    [Name(SlangClassificationDefintions.Slang_Namespace)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    [BaseDefinition(PredefinedClassificationTypeNames.Identifier)]
    internal sealed class SlangNamespace : ClassificationFormatDefinition
    {
        public SlangNamespace()
        {
            DisplayName = SlangClassificationDefintions.Slang_Namespace;
            //ForegroundColor = Color.FromRgb( 0xb8, 0xd7, 0xa3 );
        }
    }
    #endregion

    #region SlangMacro
    internal static class SlangMacroClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(SlangClassificationDefintions.Slang_Macro)]
        internal static ClassificationTypeDefinition SlangMacro = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = SlangClassificationDefintions.Slang_Macro)]
    [Name(SlangClassificationDefintions.Slang_Macro)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    [Priority(0)]
    [BaseDefinition(PredefinedClassificationTypeNames.PreprocessorKeyword)]
    internal sealed class SlangMacro : ClassificationFormatDefinition
    {
    }
    #endregion
}