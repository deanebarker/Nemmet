using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.UI;

namespace Nemmet
{
    public static class NemmetParser
    {
        public static string GetHtml(string code)
        {
            return NemmetTag.Parse(code).Select(x => x.ToHtml()).JoinOn();
        }
    }

    public static class NemmetParsingOptions
    {
        static NemmetParsingOptions()
        {
            ResetToDefaults();
        }

        public static void ResetToDefaults()
        {
            AlwaysLowerCaseTagName = true; ;
        }

        public static bool AlwaysLowerCaseTagName { get; set; }
    }

    public class NemmetTag
    {
        public string Name { get; set; }
        public List<string> Classes { get; set; }
        public string Id { get; set; }
        public string InnerHtml { get; set; }
        public List<NemmetTag> Children { get; set; }
        public NemmetTag Parent { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        // Regex and delimiter constants
        private const char CHILD_OPERATOR = '>';
        private const char SIBLING_OPERATOR = '+';
        private const char CLIMBUP_OPERATOR = '^';
        private const string CONTENT_PATTERN = @"{([^}]*)}";
        private const string ATTRIBUTES_PATTERN = @"\[([^\]]*)\]";
        private const string ID_CLASS_PATTERN = @"[#\.][^{#\.]*";
        private const string NONWORD_PATTERN = @"\W";
        private const string SPACE = " ";
        private const char OPEN_CURLEY_BRACE = '{';
        private const char CLOSING_CURLEY_BRACE = '}';
        private const char OPEN_PARAN = '(';
        private const char CLOSE_PARAN = ')';

        public static List<NemmetTag> Parse(string code)
        {
            // This returns a List<NemmetTag> because there could be multiple top-level tags ("tag1+tag2+tag3"). We can't assume this HTML fragment will be well-formed.

            // The top tag on this stack represents the tag to which new tags will be added as children. We put an empty placeholder in there to initialize it.
            var tagStack = new Stack<NemmetTag>();
            tagStack.Push(new NemmetTag("root"));

            // We'll keep track if we're in a brace or not to determine whether something is an actual operator or just text content
            var inBraces = false;

            // Iterate through each character
            // We add the sibling operator to the end to make sure the last tag gets added (remember, we only process the buffer when we encouter an operator, so we need to make sure there's a final operator on the end)
            var buffer = new StringBuilder();
            foreach(var character in string.Concat(code, SIBLING_OPERATOR))
            {
                // Toggle the braces indicator so we know if something is an operator of just content
                if(character == OPEN_CURLEY_BRACE || character == CLOSING_CURLEY_BRACE)
                {
                    inBraces = !inBraces;
                }

                // Is this an operator that is NOT contained in a brace?
                if (string.Concat(SIBLING_OPERATOR,CHILD_OPERATOR,CLIMBUP_OPERATOR,OPEN_PARAN).Contains(character) && !inBraces)
                {
                    // We have encountered an operator, which means whatever is in the buffer represents a single tag
                    // We need to...
                    // 1. Create this tag
                    // 2. Evaluate the operator to determine where the NEXT tag will go (by resetting the activeTag)

                    // If there's anything in the buffer, process it as a new child of the context tag
                    // (If you're climbing up more than one level at a time ("^^") there might not be anything in the buffer.)
                    NemmetTag tag = null;
                    if (buffer.Length > 0)
                    {
                        tag = new NemmetTag(buffer.ToString());
                        tagStack.Peek().Children.Add(tag);

                        // We empty the buffer so we can start accumulating the next tag
                        buffer.Clear();
                    }

                    // Now, what do we do with the NEXT tag?

                    // The next tag should be added to the same tag as the last one.
                    if (character == SIBLING_OPERATOR)
                    {
                        // Do nothing. This is just for clarity.
                    }

                    // Climbing up. Remove the top tag, to reveal its parent underneath.
                    if (character == CLIMBUP_OPERATOR)
                    {
                        tagStack.Pop();
                    }

                    // Descending. Add this tag to the stack.
                    if (character == CHILD_OPERATOR)
                    {
                        tagStack.Push(tag);
                    }
                }
                else
                {
                    buffer.Append(character);
                }
            }

            // The base tag in the stack was just a placeholder, remember. We want to return the top-level children of that.
            return tagStack.Last().Children;
        }
        
        public NemmetTag(string token)
        {
            Classes = new List<string>();
            Children = new List<NemmetTag>();
            Attributes = new Dictionary<string, string>();

            // The incoming text string should represent THIS TAG ONLY.  The string should NOT have any operators in it. It should be the configuration this tag only.

            Name = token.GetBefore(NONWORD_PATTERN);
            if(NemmetParsingOptions.AlwaysLowerCaseTagName)
            {
                Name = Name.ToLower();
            }

            // Tag content
            foreach (Match subtoken in Regex.Matches(token, CONTENT_PATTERN))
            {
                InnerHtml = subtoken.Groups[1].Value;
                token = token.Remove(subtoken.Value);
            }

            // Tag attributes
            foreach (Match subtoken in Regex.Matches(token, ATTRIBUTES_PATTERN))
            {
                foreach (var attribute in subtoken.Groups[1].Value.SplitOnAny())
                {
                    var key = attribute.GetBefore("=");
                    var value = attribute.GetAfter("=");
                    Attributes.Add(key, value);
                }
                token = token.Remove(subtoken.Value);
            }

            // Tag ID and class
            foreach (Match subtoken in Regex.Matches(token, ID_CLASS_PATTERN))
            {
                if (subtoken.Value.StartsWith("#"))
                {
                    Id = subtoken.Value.TrimStart("#".ToCharArray());
                }

                if (subtoken.Value.StartsWith("."))
                {
                    Classes.Add(subtoken.Value.TrimStart(".".ToCharArray()));
                }
            }
        }

        // This is a CSS representation of the tag ("div.class"), which is handy for identification
        public override string ToString()
        {
            var name = Name;
            if (Classes.Any())
            {
                name = string.Concat(name, ".", Classes.JoinOn("."));
            }
            return name;
        }

        public string ToHtml()
        {
            // If the tag has no name, then it's not a tag -- it's just text content ("{content}")
            if (string.IsNullOrWhiteSpace(Name))
            {
                return InnerHtml;
            }

            var stringWriter = new StringWriter();
            var tagBuilder = new HtmlTextWriter(stringWriter);

            // Add the ID
            if (!string.IsNullOrWhiteSpace(Id))
            {
                tagBuilder.AddAttribute("id", Id);
            }

            // Add the classes
            if (Classes.Any())
            {
                tagBuilder.AddAttribute("class", Classes.JoinOn(SPACE));
            }

            // Add the attributes
            foreach (var attribute in Attributes)
            {
                tagBuilder.AddAttribute(attribute.Key, attribute.Value);
            }

            // Render
            tagBuilder.RenderBeginTag(Name);
            tagBuilder.Write(InnerHtml);

            // Recurse through the children
            foreach (var tag in Children)
            {
                tagBuilder.Write(tag.ToHtml());
            }
            
            tagBuilder.RenderEndTag();

            return stringWriter.ToString();
        }
    }

    static class NemmetTagExtensions
    {
        public static string[] SplitOn(this string text, string delim = " ")
        {
            if(delim.Length == 1)
            {
                return text.SplitOnAny(delim);
            }

            return Regex.Split(text, delim);
        }

        public static string[] SplitOnAny(this string text, string delim = " ")
        {
            return text.Split(delim.ToCharArray());
        }

        public static string GetBefore(this string text, string delim)
        {
            return text.SplitOn(delim).First();
        }

        public static string GetAfter(this string text, string delim)
        {
            return text.SplitOn(delim).Last();
        }

        public static string GetBeforeAny(this string text, string delim)
        {
            return text.SplitOnAny(delim).First();
        }

        public static string GetAfterAny(this string text, string delim)
        {
            return text.SplitOnAny(delim).Last();
        }

        public static string JoinOn(this IEnumerable<string> collection, string delim = null)
        {
            return string.Join(delim ?? string.Empty, collection);
        }

        public static string Remove(this string text, string target)
        {
            return text.Replace(target, string.Empty);
        }
    }
}