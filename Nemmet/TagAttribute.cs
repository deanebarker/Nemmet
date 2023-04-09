using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeaneBarker
{
    public class Tag
    {
        private static string[] singular = new[] { "br", "hr", "img", "meta", "link" };

        public string Source { get; set; }

        public Tag Parent { get; set; }
        public List<Tag> Children = new();

        public List<Attribute> Attributes = new();

        public string Name { get; set; }
        public string Content { get; set; }

        public NextType ExitPath { get; set; }

        public override string ToString()
        {
            // Process the tag
            var tag = Nemmet.OutputProcessor != null ? Nemmet.OutputProcessor(this) : this;

            var childrenHtml = string.Join(string.Empty, tag.Children.Select(t => t.ToString()));
            if (string.IsNullOrWhiteSpace(tag.Name))
            {
                // If there's no name, then this is just an outer container tag
                // We effectively return the innerHtml
                return childrenHtml;
            }

            var sb = new StringBuilder();
            sb.Append("<");
            sb.Append(tag.Name.ToLower());

            if (tag.Attributes.Count > 0)
            {
                sb.Append(" ");
            }

            sb.Append(string.Join(' ', tag.Attributes.Select(a => a.ToString())));

            if (singular.Contains(tag.Name))
            {
                sb.Append("/>");
                return sb.ToString();
            }

            sb.Append(">");

            sb.Append(childrenHtml);

            // Process the content
            var content = Nemmet.ContentProccesor != null ? Nemmet.ContentProccesor(this.Content) : this.Content;
            sb.Append(content ?? string.Empty);

            sb.Append("</");
            sb.Append(tag.Name.ToLower());
            sb.Append(">");

            return sb.ToString();
        }
    }
}
