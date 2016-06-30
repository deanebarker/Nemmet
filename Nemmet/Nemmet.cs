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

        public static List<NemmetTag> Parse(string code)
        {
            // This returns a List<NemmetTag> because there could be multiple top-level tags ("tag1+tag2+tag3"). We can't assume this HTML fragment will be well-formed.

            // This is a "fake" tag, to which we'll add other tags. At the end of the parse, we'll return the children of this.
            var activeTag = new NemmetTag(null, string.Empty);

            // Keep a separate reference to the starting tag, so we can find our way back to it easily (we could crawl up from whatever tag we ended on, but this is easier...)
            var root = activeTag;

            // We track the last tag added, so we can climb-up to its parent, if we need to
            NemmetTag lastTag = null;

            // Iterate through each character
            var buffer = new StringBuilder();
            foreach (var character in string.Concat(code, SIBLING_OPERATOR))
            {
                // Is this an operator?
                if (string.Concat(SIBLING_OPERATOR,CHILD_OPERATOR,CLIMBUP_OPERATOR).Contains(character))
                {
                    // We have encountered an operator, which means whatever is in the buffer represents a single tag
                    // We need to...
                    // 1. Create this tag
                    // 2. Evaluate the operator to determine where the NEXT tag will go (by resetting the activeTag)

                    // If there's anything in the buffer, process it as a new child of the context tag
                    // (If you're climbing up more than one level at a time ("^^") there might not be anything in the buffer.)
                    if (buffer.Length > 0)
                    {
                        var tag = new NemmetTag(activeTag, buffer.ToString());
                        activeTag.Children.Add(tag);
                        lastTag = tag;

                        // We empty the buffer so we can start accumulating the next tag
                        buffer.Clear();
                    }

                    // Now, what do we do with the NEXT tag?

                    // The next tag should be added to the same active tag as the last one.
                    if(character == SIBLING_OPERATOR)
                    {
                        // Do nothing. This is just for clarity.
                    }

                    // Climbing up. The next tag should be added to the parent of the last tag.
                    if (character == CLIMBUP_OPERATOR)
                    {
                        activeTag = activeTag.Parent;
                    }

                    // Descending. The next tag should be added as a child of the last tag we added.
                    if (character == CHILD_OPERATOR)
                    {
                        activeTag = lastTag;
                    }
                }
                else
                {
                    buffer.Append(character);
                }
            }

            // The root tag is empty -- remember, we just added it as a placeholder. We want to return the top-level children of it.
            return root.Children;
        }
        
        public NemmetTag(NemmetTag parent, string token)
        {
            // The incoming text string should represent THIS TAG ONLY.  The string should NOT have any operators in it. It should be the configuration this tag only.

            Parent = parent;

            Classes = new List<string>();
            Children = new List<NemmetTag>();
            Attributes = new Dictionary<string, string>();

            Name = token.GetBefore(NONWORD_PATTERN);

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
                name = string.Concat(name, ".", Classes.JoinOn(SPACE));
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