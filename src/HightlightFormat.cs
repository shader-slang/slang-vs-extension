using EnvDTE;
using Microsoft.Build.Framework.XamlTypes;
using Microsoft.VisualStudio;
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
        public const string Slang_String = "Slang String";

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
    internal sealed class SlangParameter : ClassificationFormatDefinition
    {
        public SlangParameter()
        {
            DisplayName = SlangClassificationDefintions.Slang_Parameter;
            
            //ForegroundColor = Colors.OliveDrab;
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
    [Order(Before = Priority.Default)]
    internal sealed class SlangMacro : ClassificationFormatDefinition
    {
        public SlangMacro()
        {
            DisplayName = SlangClassificationDefintions.Slang_Macro;
            //ForegroundColor = Color.FromRgb( 0xbd, 0x63, 0xc5 );
        }
    }
    #endregion

    #region SlangString
    internal static class SlangStringClassificationDefinition
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(SlangClassificationDefintions.Slang_String)]
        internal static ClassificationTypeDefinition SlangString = null;
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = SlangClassificationDefintions.Slang_String)]
    [Name(SlangClassificationDefintions.Slang_String)]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class SlangString : ClassificationFormatDefinition
    {
        public SlangString()
        {
            DisplayName = SlangClassificationDefintions.Slang_String;
            //ForegroundColor = Color.FromRgb(232, 201, 187);
        }
    }
    #endregion


    #region Autogenerated resource keys
    class SlangColorKeys
    {
        // These resource keys are generated by Visual Studio Extension Color Editor, and should be replaced when new colors are added to this category.
        public static readonly Guid Category = new Guid("4b758f8f-f59a-4a88-b6be-49ecf08521b5");

        private static ThemeResourceKey _SlangenumTextColorKey;
        private static ThemeResourceKey _SlangenumTextBrushKey;
        public static ThemeResourceKey SlangenumTextColorKey { get { return _SlangenumTextColorKey ?? (_SlangenumTextColorKey = new ThemeResourceKey(Category, "Slang enum", ThemeResourceKeyType.ForegroundColor)); } }
        public static ThemeResourceKey SlangenumTextBrushKey { get { return _SlangenumTextBrushKey ?? (_SlangenumTextBrushKey = new ThemeResourceKey(Category, "Slang enum", ThemeResourceKeyType.ForegroundBrush)); } }

        private static ThemeResourceKey _SlangkeywordTextColorKey;
        private static ThemeResourceKey _SlangkeywordTextBrushKey;
        public static ThemeResourceKey SlangkeywordTextColorKey { get { return _SlangkeywordTextColorKey ?? (_SlangkeywordTextColorKey = new ThemeResourceKey(Category, "Slang keyword", ThemeResourceKeyType.ForegroundColor)); } }
        public static ThemeResourceKey SlangkeywordTextBrushKey { get { return _SlangkeywordTextBrushKey ?? (_SlangkeywordTextBrushKey = new ThemeResourceKey(Category, "Slang keyword", ThemeResourceKeyType.ForegroundBrush)); } }

        private static ThemeResourceKey _SlangmethodTextColorKey;
        private static ThemeResourceKey _SlangmethodTextBrushKey;
        public static ThemeResourceKey SlangmethodTextColorKey { get { return _SlangmethodTextColorKey ?? (_SlangmethodTextColorKey = new ThemeResourceKey(Category, "Slang method", ThemeResourceKeyType.ForegroundColor)); } }
        public static ThemeResourceKey SlangmethodTextBrushKey { get { return _SlangmethodTextBrushKey ?? (_SlangmethodTextBrushKey = new ThemeResourceKey(Category, "Slang method", ThemeResourceKeyType.ForegroundBrush)); } }

        private static ThemeResourceKey _SlangparameterTextColorKey;
        private static ThemeResourceKey _SlangparameterTextBrushKey;
        public static ThemeResourceKey SlangparameterTextColorKey { get { return _SlangparameterTextColorKey ?? (_SlangparameterTextColorKey = new ThemeResourceKey(Category, "Slang parameter", ThemeResourceKeyType.ForegroundColor)); } }
        public static ThemeResourceKey SlangparameterTextBrushKey { get { return _SlangparameterTextBrushKey ?? (_SlangparameterTextBrushKey = new ThemeResourceKey(Category, "Slang parameter", ThemeResourceKeyType.ForegroundBrush)); } }

        private static ThemeResourceKey _SlangtypeTextColorKey;
        private static ThemeResourceKey _SlangtypeTextBrushKey;
        public static ThemeResourceKey SlangtypeTextColorKey { get { return _SlangtypeTextColorKey ?? (_SlangtypeTextColorKey = new ThemeResourceKey(Category, "Slang type", ThemeResourceKeyType.ForegroundColor)); } }
        public static ThemeResourceKey SlangtypeTextBrushKey { get { return _SlangtypeTextBrushKey ?? (_SlangtypeTextBrushKey = new ThemeResourceKey(Category, "Slang type", ThemeResourceKeyType.ForegroundBrush)); } }

        private static ThemeResourceKey _SlangvariableTextColorKey;
        private static ThemeResourceKey _SlangvariableTextBrushKey;
        public static ThemeResourceKey SlangvariableTextColorKey { get { return _SlangvariableTextColorKey ?? (_SlangvariableTextColorKey = new ThemeResourceKey(Category, "Slang variable", ThemeResourceKeyType.ForegroundColor)); } }
        public static ThemeResourceKey SlangvariableTextBrushKey { get { return _SlangvariableTextBrushKey ?? (_SlangvariableTextBrushKey = new ThemeResourceKey(Category, "Slang variable", ThemeResourceKeyType.ForegroundBrush)); } }
    }
    #endregion
}