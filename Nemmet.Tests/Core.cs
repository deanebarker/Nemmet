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
        /*

        A note on testing methodology --

        These tests generate HTML from Nemmet code, then parse that HTML using AngleSharp and test for the elements.
        The tests are not against the resulting string, but rather the resulting DOM.

        Also, we add a root tag called "root" to everything, so we can accurately test for elements on the top level.        
        Therefore, AngleSharp paths have to be from the "root" tag, down through every child.
        
        */

        [TestMethod]
        public void Element()
        {
            Assert.IsTrue(CheckForElement("div", "div"));
        }

        [TestMethod]
        public void DefaultElement()
        {
            Assert.IsTrue(CheckForElement(".class", "div"));
        }

        [TestMethod]
        public void DefaultElementsFromParent()
        {
            // Note: this one can be tricky, because AngleSharp adds an assumed TBODY inside the TABLE
            Assert.IsTrue(CheckForElement("table.table>.row>.cell", "table>tbody>tr>td"));
        }

        [TestMethod]
        public void LowerCaseElementName()
        {
            // This is the default
            var elementNameShouldBeLowerCase = NemmetTag.Parse("DIV");
            Assert.AreEqual("div", elementNameShouldBeLowerCase[0].Name);

            // This is after setting the option
            NemmetParsingOptions.AlwaysLowerCaseTagName = false;
            var elementNameShouldBeUpperCase = NemmetTag.Parse("DIV");
            Assert.AreEqual("DIV", elementNameShouldBeUpperCase[0].Name);
            NemmetParsingOptions.ResetToDefaults();
        }

        [TestMethod]
        public void SiblingOperator()
        {
            Assert.IsTrue(CheckForElement("one+two", "one+two"));
        }

        [TestMethod]
        public void ChildOperator()
        {
            Assert.IsTrue(CheckForElement("parent>child", "parent>child"));
        }

        [TestMethod]
        public void ClimbUpOperator()
        {
            Assert.IsTrue(CheckForElement("parent>child1>grandchild^child2", "parent>child2"));
        }


        [TestMethod]
        public void ConsecutiveClimbUpOperators()
        {
            Assert.IsTrue(CheckForElement("parent1>child>grandchild^^parent2", "parent2"));
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
        public void MultipleClassAndIdAttributes()
        {
            var code1 = "div#id.class1.class2";
            Assert.IsTrue(CheckForElement(code1, "div#id"));
            Assert.IsTrue(CheckForElement(code1, "div.class1"));
            Assert.IsTrue(CheckForElement(code1, "div.class2"));

            var code2 = "div.class1.#id.class2";
            Assert.IsTrue(CheckForElement(code2, "div#id"));
            Assert.IsTrue(CheckForElement(code2, "div.class1"));
            Assert.IsTrue(CheckForElement(code2, "div.class2"));
        }

        [TestMethod]
        public void Content()
        {
            Assert.IsTrue(GetTrimmedElementContent("div{content}", "div") == "content");
        }

        [TestMethod]
        public void ContentWithOperators()
        {
            Assert.IsTrue(CheckForElement("parent{2+2>3}>child", "parent>child"));
        }

        [TestMethod]
        public void SingleAttribute()
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
            Assert.IsTrue(GetTrimmedElementContent(code, "div.parent1>div.child1>div.grandchild1") == "content");
            Assert.IsTrue(GetElement(code, "div.grandchild2").Attributes["key"].Value == "value");
            Assert.IsTrue(CheckForElement(code, "div.parent2>div#child3"));
        }

        //[TestMethod]
        //public void Parenthetical()
        //{
        //    var code = "parent1>(child1>child2)+parent2";
        //    Assert.IsTrue(CheckForElement(code, "parent1>parent2"));
        //    Assert.IsTrue(CheckForElement(code, "parent1>child1>child2"));
        //}

        private IHtmlDocument GetParsedDoc(string code)
        {
            var html = NemmetParser.GetHtml(string.Concat("root>", code));
            Debug.WriteLine(html);
            var parser = new HtmlParser();
            return parser.Parse(html);
        }

        private string GetTrimmedElementContent(string code, string path)
        {
            return GetParsedDoc(code).QuerySelector(string.Concat("root>", path)).InnerHtml.Trim();
        }

        private IHtmlElement GetElement(string code, string path)
        {
            return (IHtmlElement)GetParsedDoc(code).QuerySelector(path);
        }

        private bool CheckForElement(string code, string path)
        {
            return GetParsedDoc(code).QuerySelectorAll(string.Concat("root>", path)).Length == 1;

        }

    }
}
