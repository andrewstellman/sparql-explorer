using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparqlExplorer.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Model;
    using System.IO;

    [TestClass]
    public class GraphManagerTests
    {
        private GraphManager _graphManager;
        private string _cachePath;
        private string _assemblyLocation;
        private string _testDataFolder;

        public GraphManagerTests()
        {
            _assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _testDataFolder = _assemblyLocation + "\\Tests\\Data\\";
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _cachePath = Path.GetTempPath();
            _graphManager = new GraphManager(_cachePath);
        }

        private class CachePathTester : GraphManager
        {
            public CachePathTester() : base() { }
            public CachePathTester(string path) : base(path) { }
            public string CachePath { get { return _cachePath; } }
        }

        [TestMethod]
        public void CheckDefaultCachePath()
        {
            CachePathTester cachePathTester = new CachePathTester();
            Assert.AreEqual(_assemblyLocation + "\\cache", cachePathTester.CachePath);
        }

        [TestMethod]
        public void CheckValidCachePath()
        {
            string tempPath = Path.GetTempPath();
            CachePathTester cachePathTester = new CachePathTester(tempPath);
            Assert.AreEqual(tempPath + "\\cache", cachePathTester.CachePath);
        }

        [TestMethod]
        [ExpectedException(typeof(System.IO.DirectoryNotFoundException), "Expected an uncreatable cache path to throw an exception")]
        public void CheckInvalidCachePath()
        {
            string invalidPath = Path.GetTempPath() + "\\this_folder_does_not_exist";
            Assert.IsFalse(Directory.Exists(invalidPath));
            CachePathTester cachePathTester = new CachePathTester(invalidPath);
        }

        [TestMethod]
        [ExpectedException(typeof(Model.GraphReadException))]
        public void TryToReadInvalidRdf()
        {
            _graphManager.LoadGraphFromString("this is invalid data that should not parse");
        }

        [TestMethod]
        [ExpectedException(typeof(Model.GraphReadException))]
        public void TryToReadInvalidFile()
        {
            _graphManager.LoadGraphFromFile(_testDataFolder + "invalid.txt");
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void TryToReadNonexistentFile()
        {
            _graphManager.LoadGraphFromFile(_testDataFolder + "this file does not exist");
        }

        [TestMethod]
        public void ReadSaveAndLoadData()
        {
            _graphManager.LoadGraphFromFile(_testDataFolder + "linkedmdb.org_bonnie_palef.xml");
            Assert.AreEqual(11, _graphManager.GraphNodeCount);

            string csvFile = Path.GetTempFileName() + ".csv";
            _graphManager.SaveGraph(csvFile);
            string[] csvData = File.ReadAllLines(csvFile);
            Assert.AreEqual(15, csvData.Length);
            Assert.IsTrue(csvData.Contains("http://data.linkedmdb.org/resource/producer/3937,http://www.w3.org/1999/02/22-rdf-syntax-ns#type,http://data.linkedmdb.org/resource/movie/producer"));
            Assert.IsTrue(csvData.Contains("http://data.linkedmdb.org/resource/producer/3937,http://www.w3.org/2000/01/rdf-schema#label,Bonnie Palef (Producer)"));
            Assert.IsTrue(csvData.Contains("http://data.linkedmdb.org/resource/producer/3937,http://data.linkedmdb.org/resource/movie/producer_name,Bonnie Palef"));
            Assert.IsTrue(csvData.Contains("http://data.linkedmdb.org/resource/producer/3937,http://data.linkedmdb.org/resource/movie/producer_producerid,3937"));
            Assert.IsTrue(csvData.Contains("http://www.freebase.com/view/guid/9202a8c04000641f800000000110de31,http://www.w3.org/2000/01/rdf-schema#seeAlso,http://data.linkedmdb.org/sparql?query=DESCRIBE+%3Chttp%3A%2F%2Fwww.freebase.com%2Fview%2Fguid%2F9202a8c04000641f800000000110de31%3E"));
            Assert.IsTrue(csvData.Contains("http://data.linkedmdb.org/resource/producer/3937,http://xmlns.com/foaf/0.1/page,http://www.freebase.com/view/guid/9202a8c04000641f800000000110de31"));
            Assert.IsTrue(csvData.Contains("http://data.linkedmdb.org/resource/film/7358,http://data.linkedmdb.org/resource/movie/producer,http://data.linkedmdb.org/resource/producer/3937"));
            Assert.IsTrue(csvData.Contains("http://data.linkedmdb.org/data/producer/3937,http://www.w3.org/2000/01/rdf-schema#comment,\"Contents of this file may include content from "));
            Assert.IsTrue(csvData.Contains("\t\t      FreeBase (http://www.freebase.com) or Wikipedia "));
            Assert.IsTrue(csvData.Contains("\t\t      (http://www.wikipedia.org) licensed under CC-BY"));
            Assert.IsTrue(csvData.Contains("\t\t      (http://www.freebase.com/view/common/license/cc_attribution_25)"));
            Assert.IsTrue(csvData.Contains("\t\t      or GFDL (http://en.wikipedia.org/wiki/Wikipedia:Text_of_the_GNU_Free_Documentation_License). "));
            Assert.IsTrue(csvData.Contains("\t\t      Refer to http://www.linkedmdb.org:8080/Main/Licensing for more details.\""));
            Assert.IsTrue(csvData.Contains("http://data.linkedmdb.org/data/producer/3937,http://www.w3.org/2000/01/rdf-schema#label,RDF Description of Bonnie Palef (Producer)"));
            Assert.IsTrue(csvData.Contains("http://data.linkedmdb.org/data/producer/3937,http://xmlns.com/foaf/0.1/primaryTopic,http://data.linkedmdb.org/resource/producer/3937"));
        }

        [TestMethod]
        public void StreamGraph()
        {
            _graphManager.LoadGraphFromFile(_testDataFolder + "linkedmdb.org_bonnie_palef.xml");
            Assert.AreEqual(11, _graphManager.GraphNodeCount);

            foreach (string outputFormat in new string[] { "ttl", "nt", "ttl", "nt", "ttl", "nt" })
            {
                StringBuilder sb = new StringBuilder();
                using (StringWriter stringWriter = new StringWriter(sb))
                {
                    _graphManager.StreamGraph(stringWriter, outputFormat);
                }
                _graphManager.LoadGraphFromString(sb.ToString());
                Assert.AreEqual(11, _graphManager.GraphNodeCount);
            }
        }

        [TestMethod]
        public void StreamQueryResults()
        {
            _graphManager.LoadGraphFromFile(_testDataFolder + "http___data.linkedmdb.org_data_country_US.ttl");
            Assert.AreEqual(21, _graphManager.GraphNodeCount);

            string query = "PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#> SELECT * { ?s rdfs:label ?o . }";
            IEnumerable<string> columnNames;
            _graphManager.ExecuteSparqlQuery(query, out columnNames);
            Assert.AreEqual(2, columnNames.Count());

            StringBuilder sb = new StringBuilder();
            using (StringWriter stringWriter = new StringWriter(sb))
            {
                _graphManager.StreamSparqlQueryResults(stringWriter, "csv");
            }
            Assert.AreEqual(@"s,o
http://data.linkedmdb.org/data/country/US,RDF Description of United States (Country)
http://data.linkedmdb.org/resource/country/US,United States (Country)
",
                sb.ToString());
        }

        [TestMethod]
        public void ReadAFewFormats()
        {
            _graphManager.LoadGraphFromFile(_testDataFolder + "http___data.linkedmdb.org_data_country_AD.xml");
            Assert.AreEqual(21, _graphManager.GraphNodeCount);

            _graphManager.LoadGraphFromFile(_testDataFolder + "http___data.linkedmdb.org_data_country_US.ttl");
            Assert.AreEqual(21, _graphManager.GraphNodeCount);

            _graphManager.LoadGraphFromFile(_testDataFolder + "linkedmdb.org_bonnie_palef.xml");
            Assert.AreEqual(11, _graphManager.GraphNodeCount);

            _graphManager.LoadGraphFromFile(_testDataFolder + "linkedmdb.org_parents.json");
            Assert.AreEqual(52, _graphManager.GraphNodeCount);
        }

        [TestMethod]
        public void TestCertificates()
        {
            _graphManager.X509Certificate = X509CertificateFileReader.ReadX509Certificate(_testDataFolder + "examplecert.cert", _testDataFolder + "examplecert.key");
            Assert.AreEqual("E=astellman@stellman-greene.com, CN=Andrew Stellman, OU=Example, O=Example Certificate, L=Brooklyn, S=New York, C=US", _graphManager.X509Certificate.Subject);
            Assert.AreEqual("E=astellman@stellman-greene.com, CN=Andrew Stellman, OU=Certificate Authority, O=Stellman and Greene Consulting, L=Brooklyn, S=New York, C=US", _graphManager.X509Certificate.Issuer);
        }
    }
}
