using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace HtmlForJavascript
{
    internal static class FormatNames
    {
        public const string Delimiter = "HtmlDelimiter";
        public const string Element = "HtmlElement";
        public const string AttributeName = "HtmlAttributeName";
        public const string Quote = "HtmlQuote";
        public const string AttributeValue = "HtmlAttributeValue";
        public const string Text = "HtmlText";
    }

    internal static class ClassificationTypeDefinitions
    {
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(FormatNames.Delimiter)]
        internal static ClassificationTypeDefinition Delimiter = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(FormatNames.Element)]
        internal static ClassificationTypeDefinition Element = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(FormatNames.AttributeName)]
        internal static ClassificationTypeDefinition AttributeName = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(FormatNames.Quote)]
        internal static ClassificationTypeDefinition Quote = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(FormatNames.AttributeValue)]
        internal static ClassificationTypeDefinition AttributeValue = null;

        [Export(typeof(ClassificationTypeDefinition))]
        [Name(FormatNames.Text)]
        internal static ClassificationTypeDefinition Text = null;
    }

    // When JS file is opened, the format definitions are created
    // Closing and reopen JS file, doesn't recreate the definitions

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = FormatNames.Delimiter)]
    [Name(FormatNames.Delimiter)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class HtmlDelimiterFormatDefinition : ClassificationFormatDefinition
    {
        public HtmlDelimiterFormatDefinition()
        {
            DisplayName = "HTML Delimiter Character (JS String Literal)";
            ForegroundColor = ThemeColorHelper.IsThemeLight ? Colors.Blue :
                                                            Colors.Silver;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = FormatNames.Element)]
    [Name(FormatNames.Element)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class HtmlElementFormatDefinition : ClassificationFormatDefinition
    {

        public HtmlElementFormatDefinition()
        {
            DisplayName = "HTML Element (JS String Literal)";
            ForegroundColor = ThemeColorHelper.IsThemeLight ? Color.FromRgb(128, 0, 0) :
                                                            Color.FromRgb(86, 156, 214);

        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = FormatNames.AttributeName)]
    [Name(FormatNames.AttributeName)]
    [UserVisible(true)]
    [Order(After = Priority.High)] // VS2013 only needs Before = Priority.Default, VS2012 needs Before = Priority.High, VS2010 needs After = Priority.High
    internal sealed class HtmlAttributeNameFormatDefinition : ClassificationFormatDefinition
    {
        public HtmlAttributeNameFormatDefinition()
        {
            DisplayName = "HTML Attribute Name (JS String Literal)";
            ForegroundColor = ThemeColorHelper.IsThemeLight ? Colors.Red :
                                                              Color.FromRgb(156, 220, 254);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = FormatNames.Quote)]
    [Name(FormatNames.Quote)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class HtmlQuoteFormatDefinition : ClassificationFormatDefinition
    {
        public HtmlQuoteFormatDefinition()
        {
            DisplayName = "HTML Quote (JS String Literal)";
            ForegroundColor = ThemeColorHelper.IsThemeLight ? Colors.Black :
                                                              Color.FromRgb(210, 210, 210);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = FormatNames.AttributeValue)]
    [Name(FormatNames.AttributeValue)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class HtmlAttributeValueFormatDefinition : ClassificationFormatDefinition
    {
        public HtmlAttributeValueFormatDefinition()
        {
            DisplayName = "HTML Attribute Value (JS String Literal)";
            ForegroundColor = ThemeColorHelper.IsThemeLight ? Colors.Blue :
                                                              Color.FromRgb(200, 200, 200);
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = FormatNames.Text)]
    [Name(FormatNames.Text)]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class HtmlTextFormatDefinition : ClassificationFormatDefinition
    {
        public HtmlTextFormatDefinition()
        {
            DisplayName = "HTML Text (JS String Literal)";
            ForegroundColor = ThemeColorHelper.IsThemeLight ? Colors.Black :
                                                              Color.FromRgb(214, 157, 153);
        }
    }
}
