using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Web.UI;

public class NemmetTag
{
    public string Name { get; set; }
    public List<string> Classes { get; set; }
    public string Id { get; set; }
    public string InnerHtml { get; set; }
    public List<NemmetTag> Children { get; set; }
    public NemmetTag Parent { get; set; }
    public Dictionary<string, string> Attributes { get; set; }


    public static List<NemmetTag> Parse(string code)
    {
        var contextTag = new NemmetTag(null, string.Empty);

        // Kept a separate reference to the starting tag, so we can find our way back to it easily
        var root = contextTag;

        NemmetTag lastTag = null;
        var buffer = new StringBuilder();

        // Iterate through every character
        foreach (var character in code + "+")
        {
            // Is this a control character?
            if ("+>^".ToCharArray().Contains(character))
            {
                // If there's anything in the buffer, process it as a new child of the context tag
                if (buffer.Length > 0)
                {
                    var tag = new NemmetTag(contextTag, buffer.ToString());
                    contextTag.Children.Add(tag);
                    lastTag = tag;
                }

                // The next tag should be added to the parent of the context tag
                if (character == '^')
                {
                    contextTag = contextTag.Parent;
                }

                // The next tag should be added to the last tag
                if (character == '>')
                {
                    contextTag = lastTag;
                }

                buffer.Clear();
            }
            else
            {
                buffer.Append(character);
            }
        }

        return root.Children;
    }

    public static string GetHtml(string code)
    {
        return string.Concat(Parse(code).Select(x => x.ToHtml()));
    }



    public NemmetTag(NemmetTag parent, string token)
    {
        Parent = parent;
        Classes = new List<string>();
        Children = new List<NemmetTag>();
        Attributes = new Dictionary<string, string>();

        Name = Regex.Split(token, @"\W").First();

        foreach (Match subtoken in Regex.Matches(token, @"{[^}]*}"))
        {
            InnerHtml = subtoken.Value.Trim("{}".ToCharArray());
            token = token.Replace(subtoken.Value, string.Empty);
        }

        foreach (Match subtoken in Regex.Matches(token, @"\[([^\]]*)\]"))
        {
            foreach (var attribute in subtoken.Groups[1].Value.Split(" ".ToCharArray()))
            {
                var key = attribute.Split("=".ToCharArray()).First();
                var value = attribute.Split("=".ToCharArray()).Last();
                Attributes.Add(key, value);
            }
            token = token.Replace(subtoken.Value, string.Empty);
        }

        foreach (Match subtoken in Regex.Matches(token, @"[#\.][^{#\.]*"))
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

    public override string ToString()
    {
        var name = Name;
        if (Classes.Any())
        {
            name = string.Concat(name, ".", string.Join(" ", Classes));
        }
        return name;
    }

    public string ToHtml()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            return InnerHtml;
        }

        var stringWriter = new StringWriter();
        var tagBuilder = new HtmlTextWriter(stringWriter);

        if (!string.IsNullOrWhiteSpace(Id))
        {
            tagBuilder.AddAttribute("id", Id);
        }

        if (Classes.Any())
        {
            tagBuilder.AddAttribute("class", string.Join(" ", Classes));
        }

        foreach (var attribute in Attributes)
        {
            tagBuilder.AddAttribute(attribute.Key, attribute.Value);
        }

        tagBuilder.RenderBeginTag(Name);
        tagBuilder.Write(InnerHtml);

        if (Children != null)
        {
            foreach (var tag in Children)
            {
                tagBuilder.Write(tag.ToHtml());
            }
        }
        tagBuilder.RenderEndTag();

        return stringWriter.ToString();
    }


}

// Define other methods and classes here