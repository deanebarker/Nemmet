using System.Text;

/*

Add this: Nemmet.PreProcessor = PleaseHaml__DontHurtEm.Stop__HamlTime;
Now you can just use multi-line strings with semantic indents in place of >, +, and ^

html  
  head  
    title{Deane was here}  
    link[href='deane.css']  
  body  
    header  
       h1.title{Deane was here} 
       main{...and here}

Next line has more leading whitespace: ">"
Next line has same leading whitespace: "+"
Next line has less leading whitespace: "^"

And yes, I know this isn't actual HAML, but it's HAML-ish. 

*/

namespace DeaneBarker.Nemmet
{
    public static class PleaseHaml__DontHurtEm
    {
        public static string Stop__HamlTime(string code)
        {
            var lines = code.Trim().Split(Environment.NewLine);
            var stack = new Queue<string>(lines);

            var sb = new StringBuilder();
            while (true)
            {
                var line = stack.Dequeue();
                sb.Append(line.Trim());

                if (stack.Count == 0)
                {
                    return sb.ToString();
                }

                var nextLine = stack.Peek();

                var lineIndent = GetWhiteSpaceCount(line);
                var nextLineIndent = GetWhiteSpaceCount(nextLine);

                if (lineIndent == nextLineIndent)
                {
                    sb.Append('+');
                }

                if (nextLineIndent > lineIndent)
                {
                    sb.Append('>');
                }

                if (lineIndent > nextLineIndent)
                {
                    sb.Append('+');
                }
            }
        }

        private static int GetWhiteSpaceCount(string input)
        {
            return input.TakeWhile(i => Char.IsWhiteSpace(i)).Count();
        }
    }
}
