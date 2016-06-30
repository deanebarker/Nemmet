# Nemmet

Nemmet is an homage to [Emmet](http://emmet.io/) (Nemmet = "Not Emmet"...get it?), the HTML expansion language.

I wanted something in (1) a single file, (2) pure C#, and (3) source that I could debug through.  It will never be as full-featured as Emmet. I'm hoping for maybe 75% on a good day.

As of this writing (June 30, 2016), it's alpha. There are a dozen unit tests, which pass.

Use with caution.

## How to Use

    var code = "div.panel>div.heading{Title}+div.content{Content}+div.footer";

	// To get a nested List<NemmetTag>
	var tags = NemmetTag.Parse(code);

    // To get the HTML as a string
    var html = NemmetTag.GetHtml(code)

Result:

    <div class="panel">
      <div class="heading">
        Title
      </div>
      <div class="content">
        Content
      </div>
      <div class="footer"></div>
    </div>
