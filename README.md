# Nemmet

Nemmet is an homage to [Emmet](http://emmet.io/) (Nemmet = "Not Emmet"...get it?), the HTML expansion language.

I wanted something in (1) a single file, (2) pure C#, and (3) source that I could debug through.

It will never be as full-featured as Emmet. I'm hoping for maybe 75%.

As of this writing (June 30, 2016), it's alpha. It works under very controlled conditions (there is exactly one unit test...)

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
