# Nemmet

Nemmet is an homage to [Emmet](http://emmet.io/) (Nemmet = "Not Emmet"...get it?), the HTML expansion language.

## How to Use

    var code = "div#my-panel.panel > div.heading{Title} + div.content{Content} + div.footer";

    // To get the HTML as a string (which just concats the results of ToHtml())
    var html = NemmetParser.TotHtml(code)

    // To get a root tag object with recursive children (each of which has a ToHtml method)
    var html = NemmetParser.Parse(code)

Result:

    <div id="my-panel" class="panel">
      <div class="heading">
        Title
      </div>
      <div class="content">
        Content
      </div>
      <div class="footer"></div>
    </div>

## What Works

Read the [Emmet syntax guide](http://docs.emmet.io/abbreviations/syntax/) for the basics.  Here the subset that Nemmet supports.

* Simple elements: `div`
* Nested elements: `parent > child`
* Sibling elements: `div1 + div2`
* The "climb up" operator: `parent1 > child ^ parent2`
* IDs: `div#id`
* Classes: `div.class1`
* Attributes: `div[key='value']`
* Multiple attributes: `div[key1='value1' key2='value2']`
* Content `div{Some text}`

## What Doesn't Work

* Repeating elements and auto-numbering (why would you need this at runtime?)
* Parentheticals/grouping
* Style abbreviations
* Default tag naming
* Multiple class names
* Attibute values have to be quoted (single or double)

## Processors

There are several function mounting points available to change how the engine functions.

### OutputProcessor(Tag, Tag)

This executes on every tag immediately before it runs `ToHtml`. The tag which is about to rendered is passed in. It can be modified because generating HTML.

```
// Replace FONT tags with styled SPANs
NemmetParser.OutputProcessor = (t) =>
{
  // A font tag? Really...?
  if (t.Name == "font")
  {
    var face = t.Attributes.FirstOrDefault(a => a.Key == "face")?.Value ?? string.Empty;

    t.Name = "span";
    t.Attributes.Clear();
    t.Attributes.Add(new Attribute("style", $"font-family: {face}"));
  }
  return t;
};

```

### ContentProcessor(string,string)

These execute before content is added. The content from the code is passed in. It can be modified before being added.

```
// Get content from a dictionary of values
// Something like html>body>main{@body}
NemmetParser.ContentProccesor = c =>
{  
  if(c == null)
  {
    // This is not a placeholder
    return c;
  }

  var placeholderParser = Literals.Char('@').SkipAnd(Literals.NonWhiteSpace());
  var placeholder = placeholderParser.Parse(c);

  if (placeholder.Length > 0)
  {
    return dictionaryOfContentValues(placeholder)
  }

    // Didn't find anything in the dictionary
  return c;
};
```

### PreProcessor

This executes before code is executed. It's used to filter the code before execution. Below is the built-in default, which trims and joins on newlines. If you re-implement, you'll need to handle these manually.

```
private static string DefaultPreprocessor(string code)
{
    code = code.Trim();
    code = code.Replace(Environment.NewLine, string.Empty);
    return code;
}
```
