using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngleSharp.Parser.Html;
using System.Diagnostics;
using AngleSharp.Dom.Html;

namespace Nemmet.Tests
{
    [TestClass]
    public class Core
    {
        [TestMethod]
        public void Element()
        {
            Assert.IsTrue(CheckForElement("div", "div"));
        }

        [TestMethod]
        public void SiblingElements()
        {
            Assert.IsTrue(CheckForElement("one+two", "one+two"));
        }

        [TestMethod]
        public void NestedElements()
        {
            Assert.IsTrue(CheckForElement("parent>child", "parent>child"));
        }

        [TestMethod]
        public void ParentElements()
        {
            Assert.IsTrue(CheckForElement("parent>child1>grandchild^child2", "parent>child2"));
        }

        [TestMethod]
        public void ClassAttribute()
        {
            Assert.IsTrue(CheckForElement("div.class", "div.class"));
        }

        [TestMethod]
        public void MultipleClassAttributes()
        {
            var code = "div.class1.class2";
            Assert.IsTrue(CheckForElement(code, "div.class1"));
            Assert.IsTrue(CheckForElement(code, "div.class2"));
        }

        [TestMethod]
        public void IdAttribute()
        {
            Assert.IsTrue(CheckForElement("div#id", "div#id"));
        }

        [TestMethod]
        public void InnerHtml()
        {
            Assert.IsTrue(GetTrimmedElementContent("div{content}", "div") == "content");
        }

        [TestMethod]
        public void Attribute()
        {
            Assert.IsTrue(CheckForElement("div[key=value]", "div[key=value]"));
        }


        [TestMethod]
        public void MultipleAttributes()
        {
            var code = "div[key1=value1 key2=value2]";
            Assert.IsTrue(CheckForElement(code, "div[key1=value1]"));
            Assert.IsTrue(CheckForElement(code, "div[key2=value2]"));
        }

        [TestMethod]
        public void Abuse()
        {
            var code = "div.parent1>div.child1>div.grandchild1{content}+div.grandchild2[key=value]^^div.parent2>div.child2+div#child3";
            Assert.IsTrue(CheckForElement(code, "div.parent1>div.child1"));
            Assert.IsTrue(CheckForElement(code, "div.parent2"));
            Assert.IsTrue(GetTrimmedElementContent(code, "div.grandchild1") == "content");
            Assert.IsTrue(GetElement(code, "div.grandchild2").Attributes["key"].Value == "value");
            Assert.IsTrue(CheckForElement(code, "div.parent2>div#child3"));
        }


        private IHtmlDocument GetParsedDoc(string code)
        {
            var html = NemmetTag.GetHtml(code);
            Debug.WriteLine(html);
            var parser = new HtmlParser();
            return parser.Parse(html);
        }

        private string GetTrimmedElementContent(string code, string path)
        {
            return GetParsedDoc(code).QuerySelector(path).InnerHtml.Trim();
        }

        private IHtmlElement GetElement(string code, string path)
        {
            return (IHtmlElement)GetParsedDoc(code).QuerySelector(path);
        }

        private bool CheckForElement(string code, string path)
        {
            return GetParsedDoc(code).QuerySelectorAll(path).Length == 1;

        }

    }
}
