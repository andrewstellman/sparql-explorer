using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparqlExplorer.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System.Windows.Controls;
    using View;

    [TestClass]
    public class SparqlTemplateEditorTests
    {
        SparqlTemplateEditor _templateEditor;

        [TestInitialize]
        public void TestInitialize()
        {
            _templateEditor = new SparqlTemplateEditor();
        }

        [TestMethod]
        public void CheckTemplateText()
        {
            _templateEditor.TemplateText = "abc ~~def~~ ghi";
            Assert.AreEqual("abc  ghi" + Environment.NewLine, _templateEditor.Text);

            _templateEditor.TemplateText = "~~only_one~~";
            Assert.AreEqual(Environment.NewLine, _templateEditor.Text);

            _templateEditor.TemplateText = "~~first~~ ghi ~~last~~";
            Assert.AreEqual(" ghi " + Environment.NewLine, _templateEditor.Text);

            _templateEditor.TemplateText = @"~~first~~ line
second ~~line~~
third ~~and~~ last ~~line~~";
            Assert.AreEqual(@" line
second 
third  last 
", _templateEditor.Text);
        }

        [TestMethod]
        public void CheckStackPanelItems()
        {
            string templateText = @"SELECT ?p ?o
{
    ~~subject~~ ~~predicate~~ ?bNode .
    ?bNode ?p ?o .
}";
            _templateEditor.TemplateText = templateText;

            Assert.AreEqual(5, _templateEditor.TemplateStackPanel.Children.Count);
            Assert.IsTrue(_templateEditor.TemplateStackPanel.Children[0] is StackPanel);
            Assert.IsTrue(_templateEditor.TemplateStackPanel.Children[1] is StackPanel);
            Assert.IsTrue(_templateEditor.TemplateStackPanel.Children[2] is StackPanel);
            Assert.IsTrue(_templateEditor.TemplateStackPanel.Children[3] is StackPanel);
            Assert.IsTrue(_templateEditor.TemplateStackPanel.Children[4] is StackPanel);

            StackPanel stackPanel = _templateEditor.TemplateStackPanel.Children[0] as StackPanel;
            Assert.AreEqual(1, stackPanel.Children.Count);

            stackPanel = _templateEditor.TemplateStackPanel.Children[2] as StackPanel;
            Assert.AreEqual(5, stackPanel.Children.Count);

            Assert.IsTrue(stackPanel.Children[0] is TextBlock);
            Assert.IsTrue(stackPanel.Children[1] is ComboBoxWithCueBanner);
            Assert.IsTrue(stackPanel.Children[2] is TextBlock);
            Assert.IsTrue(stackPanel.Children[3] is ComboBoxWithCueBanner);
            Assert.IsTrue(stackPanel.Children[4] is TextBlock);

            ComboBoxWithCueBanner cb1 = stackPanel.Children[1] as ComboBoxWithCueBanner;
            ComboBoxWithCueBanner cb3 = stackPanel.Children[3] as ComboBoxWithCueBanner;
            Assert.AreEqual(0, cb1.Items.Count);
            Assert.AreEqual(0, cb3.Items.Count);

            _templateEditor.Items = new string[] { "one", "two", "three", "http://www.example.com/four", "five" };
            Assert.AreEqual(5, cb1.Items.Count);
            Assert.AreEqual(5, cb3.Items.Count);

            cb1.Text = "cb1 cb1 cb1";
            cb3.Text = "b3b3b3b3";
            Assert.AreEqual(@"SELECT ?p ?o
{
    cb1 cb1 cb1 b3b3b3b3 ?bNode .
    ?bNode ?p ?o .
}
",
            _templateEditor.Text);
        }
    }
}
