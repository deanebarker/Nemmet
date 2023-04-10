/*
Add this: Nemmet.OutputProcessor = TagsAreHard.MyFingersHurtFromTypingSoMuch;

Simplifies CSS and JS

html > head > js{/scripts/deane} + css{/scripts/deane}

*/

namespace DeaneBarker.Nemmet
{
    public class TagsAreHard
    {
        public static Tag MyFingersHurtFromTypingSoMuch(Tag t)
        {
            if (t.Content == null)
                return t; // We have to have content or have nothing to reference...

            // css{'deane.css'}
            if (t.Name == "css")
            {
                if(!t.Content.EndsWith(".css"))
                {
                    t.Content += ".css";
                }

                t.Name = "link";
                t.Attributes.Add(new Attribute("type", "text/css"));
                t.Attributes.Add(new Attribute("rel", "stylesheet"));
                t.Attributes.Add(new Attribute("href", t.Content));
                t.Content = null;
            }

            // js{'deane.js'}
            if (t.Name == "js")
            {
                if (!t.Content.EndsWith(".js"))
                {
                    t.Content += ".js";
                }

                t.Name = "script";
                t.Attributes.Add(new Attribute("src", t.Content));
                t.Content = null;
            }

            return t;
        }
    }

}
