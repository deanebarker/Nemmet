# Nemmet

Nemmet is an homage to [Emmet](http://emmet.io/) (Nemmet = "Not Emmet"...get it?), the HTML expansion language.

I wanted something in (1) a single file, (2) pure C#, and (3) source that I could debug through.  It will never be as full-featured as Emmet. I'm hoping for maybe 75% on a good day.

## How to Use

    var code = "#my-panel.panel>.heading{Title}+.content{Content}+.footer";

	// To get a nested List<NemmetTag>
    // NemmetTag has a recursive ToHtml() method
	var tags = NemmetTag.Parse(code);

    // To get the HTML as a string (which just concats the results of ToHtml())
    var html = NemmetParser.GetHtml(code)

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
* Nested elements: `parent>child`
* Sibling elements: `div1+div2`
* The "climb up" operator: `parent1>child^parent2`
* IDs: `div#id`
* Classes: `div.class1.class2`
* Attributes: `div[key=value]`
* Multiple attributes: `div[key1=value1 key2=value2]`
* Content `div{Some text}`
* Default tag naming (though, the defaults need more definition)

## What Doesn't Work

* Repeating elements and auto-numbering (why would you need this at runtime?)
* Parentheticals/grouping (though, this is likely not far off -- I have a theory for it)
* Style abbreviations (not hard, but low on the priority list)
