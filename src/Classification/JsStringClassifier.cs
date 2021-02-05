using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace HtmlForJavascript
{
    #region Classifier

    internal class HtmlClassifier : IClassifier
    {
        private readonly IClassificationType _htmlDelimiterType;
        private readonly IClassificationType _htmlElementType;
        private readonly IClassificationType _htmlAttributeNameType;
        private readonly IClassificationType _htmlQuoteType;
        private readonly IClassificationType _htmlAttributeValueType;
        private readonly IClassificationType _htmlTextType;
        private readonly IClassifier _classifier;

        internal HtmlClassifier(IClassificationTypeRegistryService registry, IClassifier classifier)
        {
            _htmlDelimiterType = registry.GetClassificationType(FormatNames.Delimiter);
            _htmlElementType = registry.GetClassificationType(FormatNames.Element);
            _htmlAttributeNameType = registry.GetClassificationType(FormatNames.AttributeName);
            _htmlQuoteType = registry.GetClassificationType(FormatNames.Quote);
            _htmlAttributeValueType = registry.GetClassificationType(FormatNames.AttributeValue);
            _htmlTextType = registry.GetClassificationType(FormatNames.Text);

            _classifier = classifier;
        }

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var result = new List<ClassificationSpan>();


            foreach (ClassificationSpan cs in _classifier.GetClassificationSpans(span))
            {
                var csClass = cs.ClassificationType.Classification.ToLower();

                // Only apply our rules if we found a string literal
                if (csClass == "string")
                {
                    if (cs.Span.Length > 2)
                    {
                        var sspan = new SnapshotSpan(cs.Span.Start.Add(1), cs.Span.End.Subtract(1)); // exclude quote

                        List<ClassificationSpan> classification = ScanLiteral(sspan);

                        if (classification != null)
                        {
                            result.AddRange(classification);
                        }
                        else
                        {
                            result.Add(cs);
                        }
                    }
                    else
                    {
                        result.Add(cs);
                    }
                }
                else
                {
                    result.Add(cs);
                }

            }

            return result;
        }

        private enum State
        {
            Default,
            AfterOpenAngleBracket,
            ElementName,
            InsideAttributeList,
            AttributeName,
            AfterAttributeName,
            AfterAttributeEqualSign,
            AfterOpenDoubleQuote,
            AfterOpenSingleQuote,
            AttributeValue,
            InsideElement,
            AfterCloseAngleBracket,
            AfterOpenTagSlash,
            AfterCloseTagSlash,
        }

        private bool IsNameChar(char c)
        {
            return c == '_' || char.IsLetterOrDigit(c);
        }

        private List<ClassificationSpan> ScanLiteral(SnapshotSpan span)
        {
            State state = State.Default;

            var result = new List<ClassificationSpan>();

            var literal = span.GetText();
            var currentCharIndex = 0;

            int? continuousMark = null;
            var insideSingleQuote = false;
            var insideDoubleQuote = false;

            while (currentCharIndex < literal.Length)
            {
                var c = literal[currentCharIndex];

                switch (state)
                {
                    case State.Default:
                        {
                            if (c != '<')
                            {
                                return null;
                            }
                            else
                            {
                                state = State.AfterOpenAngleBracket;
                                continuousMark = null;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            break;
                        }
                    case State.AfterOpenAngleBracket:
                        {
                            if (IsNameChar(c))
                            {
                                continuousMark = currentCharIndex;
                                state = State.ElementName;
                            }
                            else if (c == '/')
                            {
                                state = State.AfterCloseTagSlash;
                                continuousMark = null;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else
                            {
                                return null;
                            }
                            break;
                        }
                    case State.ElementName:
                        {
                            if (IsNameChar(c))
                            {

                            }
                            else if (char.IsWhiteSpace(c))
                            {
                                if (continuousMark.HasValue)
                                {
                                    var length = currentCharIndex - continuousMark.Value;
                                    result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + continuousMark.Value, length), _htmlElementType));
                                    continuousMark = null;
                                }
                                state = State.InsideAttributeList;
                            }
                            else if (c == '>')
                            {
                                if (continuousMark.HasValue)
                                {
                                    var length = currentCharIndex - continuousMark.Value;
                                    result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + continuousMark.Value, length), _htmlElementType));
                                    continuousMark = null;
                                }

                                state = State.AfterCloseAngleBracket;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else if (c == '/')
                            {
                                if (continuousMark.HasValue)
                                {
                                    var length = currentCharIndex - continuousMark.Value;
                                    result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + continuousMark.Value, length), _htmlElementType));
                                    continuousMark = null;
                                }

                                state = State.AfterOpenTagSlash;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else
                            {
                                return null;
                            }
                            break;
                        }
                    case State.InsideAttributeList:
                        {
                            if (char.IsWhiteSpace(c))
                            {

                            }
                            else if (IsNameChar(c))
                            {
                                continuousMark = currentCharIndex;
                                state = State.AttributeName;
                            }
                            else if (c == '>')
                            {
                                state = State.AfterCloseAngleBracket;
                                continuousMark = null;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else if (c == '/')
                            {
                                state = State.AfterOpenTagSlash;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else
                            {
                                return null;
                            }
                            break;
                        }
                    case State.AttributeName:
                        {
                            if (char.IsWhiteSpace(c))
                            {
                                if (continuousMark.HasValue)
                                {
                                    var length = currentCharIndex - continuousMark.Value;
                                    result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + continuousMark.Value, length), _htmlAttributeNameType));
                                    continuousMark = null;
                                }
                                state = State.AfterAttributeName;
                            }
                            else if (IsNameChar(c))
                            {

                            }
                            else if (c == '=')
                            {
                                if (continuousMark.HasValue)
                                {
                                    var attrNameStart = continuousMark.Value;
                                    var attrNameLength = currentCharIndex - attrNameStart;
                                    result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + attrNameStart, attrNameLength), _htmlAttributeNameType));
                                }

                                state = State.AfterAttributeEqualSign;
                                continuousMark = null;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else if (c == '>')
                            {
                                if (continuousMark.HasValue)
                                {
                                    var attrNameStart = continuousMark.Value;
                                    var attrNameLength = currentCharIndex - attrNameStart;
                                    result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + attrNameStart, attrNameLength), _htmlAttributeNameType));
                                }

                                state = State.AfterCloseAngleBracket;
                                continuousMark = null;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else if (c == '/')
                            {
                                if (continuousMark.HasValue)
                                {
                                    var attrNameStart = continuousMark.Value;
                                    var attrNameLength = currentCharIndex - attrNameStart;
                                    result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + attrNameStart, attrNameLength), _htmlAttributeNameType));
                                }

                                state = State.AfterOpenTagSlash;
                                continuousMark = null;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else
                            {
                                return null;
                            }
                            break;
                        }
                    case State.AfterAttributeName:
                        {
                            if (char.IsWhiteSpace(c))
                            {

                            }
                            else if (IsNameChar(c))
                            {
                                continuousMark = currentCharIndex;
                                state = State.AttributeName;
                            }
                            else if (c == '=')
                            {
                                state = State.AfterAttributeEqualSign;
                                continuousMark = null;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else if (c == '/')
                            {
                                state = State.AfterOpenTagSlash;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else if (c == '>')
                            {
                                state = State.AfterCloseAngleBracket;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else
                            {
                                return null;
                            }
                            break;
                        }
                    case State.AfterAttributeEqualSign:
                        {
                            if (char.IsWhiteSpace(c))
                            {

                            }
                            else if (IsNameChar(c))
                            {
                                continuousMark = currentCharIndex;
                                state = State.AttributeValue;
                            }
                            else if (c == '\"')
                            {
                                state = State.AfterOpenDoubleQuote;
                                insideDoubleQuote = true;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlQuoteType));
                            }
                            else if (c == '\'')
                            {
                                state = State.AfterOpenSingleQuote;
                                insideSingleQuote = true;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlQuoteType));
                            }
                            else
                            {
                                return null;
                            }
                            break;
                        }
                    case State.AfterOpenDoubleQuote:
                        {
                            if (c == '\"')
                            {
                                state = State.InsideAttributeList;
                                insideDoubleQuote = false;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlQuoteType));
                            }
                            else
                            {
                                continuousMark = currentCharIndex;
                                state = State.AttributeValue;
                            }
                            break;
                        }
                    case State.AfterOpenSingleQuote:
                        {
                            if (c == '\'')
                            {
                                state = State.InsideAttributeList;
                                insideSingleQuote = false;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlQuoteType));
                            }
                            else
                            {
                                continuousMark = currentCharIndex;
                                state = State.AttributeValue;
                            }
                            break;
                        }
                    case State.AttributeValue:
                        {
                            if (c == '\'')
                            {
                                if (insideSingleQuote)
                                {
                                    state = State.InsideAttributeList;
                                    insideSingleQuote = false;
                                    result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlQuoteType));

                                    if (continuousMark.HasValue)
                                    {
                                        var start = continuousMark.Value;
                                        var length = currentCharIndex - start;
                                        continuousMark = null;

                                        result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + start, length), _htmlAttributeValueType));
                                    }
                                }
                            }
                            else if (c == '\"')
                            {
                                if (insideDoubleQuote)
                                {
                                    state = State.InsideAttributeList;
                                    insideDoubleQuote = false;
                                    result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlQuoteType));

                                    if (continuousMark.HasValue)
                                    {
                                        var start = continuousMark.Value;
                                        var length = currentCharIndex - start;
                                        continuousMark = null;

                                        result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + start, length), _htmlAttributeValueType));
                                    }
                                }
                            }
                            else
                            {

                            }

                            break;
                        }
                    case State.AfterCloseAngleBracket:
                        {
                            if (c == '<')
                            {
                                state = State.AfterOpenAngleBracket;
                                continuousMark = null;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else
                            {
                                continuousMark = currentCharIndex;
                                state = State.InsideElement;
                            }
                            break;
                        }
                    case State.InsideElement:
                        {
                            if (c == '<')
                            {
                                state = State.AfterOpenAngleBracket;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));

                                if (continuousMark.HasValue)
                                {
                                    var start = continuousMark.Value;
                                    var length = currentCharIndex - start;
                                    continuousMark = null;

                                    result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + start, length), _htmlTextType));
                                }
                            }
                            else
                            {

                            }

                            break;
                        }
                    case State.AfterCloseTagSlash:
                        {
                            if (char.IsWhiteSpace(c))
                            {

                            }
                            else if (IsNameChar(c))
                            {
                                continuousMark = currentCharIndex;
                                state = State.ElementName;
                            }
                            else
                            {
                                return null;
                            }
                            break;
                        }
                    case State.AfterOpenTagSlash:
                        {
                            if (c == '>')
                            {
                                state = State.AfterCloseAngleBracket;
                                continuousMark = null;
                                result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + currentCharIndex, 1), _htmlDelimiterType));
                            }
                            else
                            {
                                return null;
                            }
                            break;
                        }
                    default:
                        break;
                }

                ++currentCharIndex;
            }

            // if the continuous span is stopped because of end of literal,
            // the span was not colored, handle it here
            if (currentCharIndex >= literal.Length)
            {
                if (continuousMark.HasValue)
                {
                    if (state == State.ElementName)
                    {
                        var start = continuousMark.Value;
                        var length = literal.Length - start;
                        result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + start, length), _htmlElementType));
                    }
                    else if (state == State.AttributeName)
                    {
                        var attrNameStart = continuousMark.Value;
                        var attrNameLength = literal.Length - attrNameStart;
                        result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + attrNameStart, attrNameLength), _htmlAttributeNameType));
                    }
                    else if (state == State.AttributeValue)
                    {
                        var start = continuousMark.Value;
                        var length = literal.Length - start;
                        result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + start, length), _htmlAttributeValueType));
                    }
                    else if (state == State.InsideElement)
                    {
                        var start = continuousMark.Value;
                        var length = literal.Length - start;
                        result.Add(new ClassificationSpan(new SnapshotSpan(span.Start + start, length), _htmlTextType));
                    }
                }
            }

            return result;
        }
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
    }
    #endregion //Classifier
}
