using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngleSharp.Parser.Html;

namespace Nemmet.Tests
{
    [TestClass]
    public class Core
    {
        [TestMethod]
        public void TestMethod1()
        {
            var nemmetCode = "div.panel>div.panel-heading>h2{Title}^div.panel-contents>p>span[style=visibility:hidden]{Deane}+{ }+a[href=http://cnn.com]{was here}^^div.panel-footer";
            var html = NemmetTag.GetHtml(nemmetCode);
            Assert.IsTrue(ElementExists(html, "div.panel div.panel-heading"));
        }


        private bool ElementExists(string html, string path)
        {
            var parser = new HtmlParser();
            var doc = parser.Parse(html);

            return doc.QuerySelectorAll(path).Length > 0;

        }
    }
}
