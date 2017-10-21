using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparqlExplorer.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Model;

    [TestClass]
    public class TemplateManagerTests
    {
        TemplateManager _templateManager;

        [TestInitialize]
        public void TestInitialize()
        {
            _templateManager = new TemplateManager("Tests\\Templates");
        }

        [TestMethod]
        public void CheckTemplateNames()
        {
            Assert.AreEqual(3, _templateManager.TemplateNames.Count());
            Assert.IsTrue(_templateManager.TemplateNames.Contains("A file"));
            Assert.IsTrue(_templateManager.TemplateNames.Contains("A text file with ~~~ characters"));
            Assert.IsTrue(_templateManager.TemplateNames.Contains("A text file with spaces"));
        }

        [TestMethod]
        public void ReadTemplate()
        {
            Assert.AreEqual(@"this 
is
a
template ~~~ template", _templateManager.GetTemplateText("A text file with ~~~ characters"));
        }

        [TestMethod]
        public void ReadInvalidTemplate()
        {
            Assert.AreEqual(String.Empty, _templateManager.GetTemplateText("xyz"));
        }
    }
}
