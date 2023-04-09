using Parlot.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using static Parlot.Fluent.Parsers;

namespace DeaneBarker
{

    public class Nemmet
    {
        private static Parser<List<Tag>> parser;

        // Processors
        public static Func<Tag, Tag> OutputProcessor { get; set; }
        public static Func<string, string> PreProcessor { get; set; } = DefaultPreprocessor;
        public static Func<string, string> ContentProccesor { get; set; }


        static Nemmet()
        {
            // Constants / helpers
            var dot = Literals.Char('.');
            var pound = Literals.Char('#');
            var equals = Literals.Char('=');
            var openBracket = Literals.Char('[');
            var closeBracket = Literals.Char(']');
            var openBrace = Literals.Char('{');
            var closeBrace = Literals.Char('}');
            var plus = Literals.Char('+');
            var closeAngleBracket = Literals.Char('>');
            var caret = Literals.Char('^');

            var lettersAndNumbersOnly = Literals
                .Pattern(v => Char.IsLetterOrDigit(v))
                .Then(v => v.ToString());

            // .className
            var classModifier = dot
                .SkipAnd(lettersAndNumbersOnly)
                .Then(v => new Attribute[] { new Attribute("class", v) });

            // #ID
            var idModifier = pound
                .SkipAnd(lettersAndNumbersOnly)
                .Then(v => new Attribute[] { new Attribute("id", v) });

            // deane="awesome"
            var attribute = lettersAndNumbersOnly
                .AndSkip(equals)
                .And(Literals.String())
                .Then(v => new Attribute(v.Item1, v.Item2.ToString())
            );

            // [deane="awesome" yes="seriously"]
            var attributeModifier = Between(
                openBracket,
                Separated(Literals.Char(' '), attribute),
                closeBracket
            ).Then(v => {
                return v.ToArray();
            });

            var tagAttributes = OneOf(attributeModifier, idModifier, classModifier);

            // {the text of War and Peace}
            var content = Between(
                    openBrace,
                    Literals.Pattern(v => v != '}'),
                    closeBrace
                ).Then(v => v.ToString());

            // Exit paths
            var exitAcross = plus.Then(v => NextType.Across);
            var exitDown = closeAngleBracket.Then(v => NextType.Down);
            var exitUp = caret.Then(v => NextType.Up);
            var next = ZeroOrOne(Literals.WhiteSpace())
                .SkipAnd(OneOf(exitAcross, exitDown, exitUp))
                .AndSkip(ZeroOrOne(Literals.WhiteSpace()));

            var tag = lettersAndNumbersOnly // Item1
                .And(ZeroOrMany(tagAttributes)) // Item2 (list)
                .And(ZeroOrOne(content)) // Item3
                .And(ZeroOrOne(next)) // Item4
                .Then<Tag>(v =>
                {
                    var tag = new Tag()
                    {
                        Name = v.Item1.ToLower(),
                        Attributes = v.Item2.SelectMany(v => v).ToList(),
                        Content = v.Item3,
                        ExitPath = v.Item4
                    };
                    return tag;
                });

            parser = ZeroOrMany(tag);
        }

        public static List<Tag> GetTags(string code)
        {
            return parser.Parse(PreProcessor(code));
        }

        public static Tag Parse(string code)
        {
            return Organize(GetTags(code));
        }

        public static string ToHtml(string code)
        {
            return Organize(GetTags(code)).ToString();
        }

        public static Tag Organize(List<Tag> items, Tag currentTag = null)
        {
            currentTag = currentTag ?? new Tag(); // A tag without a name doesn't render, it just contains

            foreach (var item in items)
            {
                item.Parent = currentTag;
                currentTag.Children.Add(item);

                if (item.ExitPath == NextType.Down)
                {
                    currentTag = item;
                }

                if (item.ExitPath == NextType.Up)
                {
                    if (item.Parent?.Parent != null)
                    {
                        currentTag = item.Parent.Parent;
                    }
                }

                if (item.ExitPath == NextType.Across)
                {
                    // Do nothing; just here for consistency
                }
            }

            // Climb back up to the top to return the root tag
            while (currentTag.Parent != null)
            {
                currentTag = currentTag.Parent;
            }

            return currentTag;
        }

        private static string DefaultPreprocessor(string code)
        {
            code = code.Trim();
            code = code.Replace(Environment.NewLine, string.Empty);
            return code;
        }
    }

}