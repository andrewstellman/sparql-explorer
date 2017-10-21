using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparqlExplorer.Model
{
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Security.Cryptography.X509Certificates;
    using VDS.RDF;
    using VDS.RDF.Parsing;
    using VDS.RDF.Query;
    using VDS.RDF.Writing;

    public class GraphManager
    {
        public int GraphNodeCount { get { return (_graph == null) ? 0 : _graph.Nodes.Count(); } }

        public int GraphPrefixCount
        {
            get
            {
                return
                    (_graph == null || _graph.NamespaceMap == null || _graph.NamespaceMap.Prefixes == null) 
                    ? 0 
                    : _graph.NamespaceMap.Prefixes.Count();
            }
        }

        public IEnumerable<string> Prefixes
        {
            get
            {
                var prefixes = new List<string>();
                if (!(_graph == null || _graph.NamespaceMap == null || _graph.NamespaceMap.Prefixes == null))
                {
                    foreach (string prefix in _graph.NamespaceMap.Prefixes)
                    {
                        prefixes.Add("PREFIX " + prefix + ": <" + _graph.NamespaceMap.GetNamespaceUri(prefix) + ">");
                    }
                }
                return prefixes;
            }
        }


        private IGraph _graph;
        private SparqlResultSet _latestQueryResults;

        protected readonly string _cachePath; // protected for unit testing

        private const string DEFAULT_CACHE_FOLDER_NAME = "cache";

        public X509Certificate2 X509Certificate;

        public GraphManager() : this(null) { }

        public GraphManager(string folder)
        {
            if (String.IsNullOrWhiteSpace(folder))
            {
                string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                _cachePath = assemblyLocation + "\\" + DEFAULT_CACHE_FOLDER_NAME;
            }
            else
            {
                if (!Directory.Exists(folder))
                {
                    throw new DirectoryNotFoundException("Unable to create cache directory, folder does not exist: " + folder);
                }
                _cachePath = folder + "\\" + DEFAULT_CACHE_FOLDER_NAME;
            }
        }

        public async Task ExecuteRestApiQuery(string url, bool executeAsync = true)
        {
            string rdf;
            try
            {
                WebResponse webResponse;
                HttpWebRequest request = HttpWebRequest.CreateHttp(url);
                request.Accept = "*/*";

                if (X509Certificate != null)
                    request.ClientCertificates.Add(X509Certificate);

                if (executeAsync)
                    webResponse = await request.GetResponseAsync();
                else
                    webResponse = request.GetResponse();

                using (Stream responseStream = webResponse.GetResponseStream())
                {
                    StreamReader readStream = new StreamReader(responseStream, Encoding.UTF8);
                    rdf = readStream.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                string message = "Unable to read REST API data from " + url;
                throw new GraphReadException(message, ex);
            }

            //TODO: Add a facility that allows tweaking for errors like this
            if (rdf.Contains("http:www.bridgedb.org"))
            {
                rdf = rdf.Replace("http:www.bridgedb.org", "http://www.bridgedb.org");
            }

            LoadGraphFromString(rdf);
        }


        public void LoadGraphFromFile(string filename)
        {
            string rdf = File.ReadAllText(filename);
            LoadGraphFromString(rdf);

        }
        public void LoadGraphFromString(string rdf)
        {
            _graph = new Graph();
            try
            {
                _graph.LoadFromString(rdf);
            }
            catch (Exception ex)
            {
                string message = "Unable to parse graph data";
                throw new GraphReadException(message, ex);
            }
        }

        public IEnumerable<object> ExecuteSparqlQuery(string query, out IEnumerable<string> columnNames)
        {
            _latestQueryResults = ResultSetHelper.ExecuteQuery(query, _graph, out columnNames);
            return ResultSetHelper.ResultSetToExpandoObjects(_latestQueryResults, _graph.NamespaceMap, columnNames);
        }

        public string SaveGraph(string filename, string outputFormat = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(outputFormat))
                {
                    outputFormat = Path.GetExtension(filename).Substring(1);
                }
                IRdfWriter rdfWriter = GetRdfWriter(outputFormat);
                _graph.SaveToFile(filename, rdfWriter);
                return "Graph written to " + Path.GetFileName(filename);
            }
            catch (Exception ex)
            {
                return "Unable to write graph" + Environment.NewLine + ex.Message;
            }
        }

        public void StreamGraph(TextWriter textWriter, string outputFormat)
        {
            IRdfWriter rdfWriter = GetRdfWriter(outputFormat);
            rdfWriter.Save(_graph, textWriter);
        }

        private static IRdfWriter GetRdfWriter(string outputFormat)
        {
            IRdfWriter rdfWriter;
            switch (outputFormat)
            {
                case "ttl":
                    CompressingTurtleWriter compressingTurtleWriter = new CompressingTurtleWriter();
                    compressingTurtleWriter.PrettyPrintMode = true;
                    compressingTurtleWriter.CompressionLevel = WriterCompressionLevel.High;
                    rdfWriter = compressingTurtleWriter;
                    break;
                case "nt":
                    rdfWriter = new NTriplesWriter();
                    break;
                case "tsv":
                    rdfWriter = new TsvWriter();
                    break;
                case "html":
                    rdfWriter = new HtmlWriter();
                    break;
                case "json":
                    rdfWriter = new RdfJsonWriter();
                    break;
                case "dot":
                    rdfWriter = new GraphVizWriter();
                    break;
                case "csv":
                default:
                    rdfWriter = new CsvWriter();
                    break;
            }
            return rdfWriter;
        }

        public string SaveSparqlQueryResults(string filename, string outputFormat = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(outputFormat))
                {
                    outputFormat = Path.GetExtension(filename).Substring(1);
                }
                ISparqlResultsWriter resultsWriter = GetSparqlResultsWriter(outputFormat);
                resultsWriter.Save(_latestQueryResults, filename);
                return "Query results written to " + Path.GetFileName(filename);
            }
            catch (Exception ex)
            {
                return "Unable to write query results" + Environment.NewLine + ex.Message;
            }
        }

        public void StreamSparqlQueryResults(TextWriter output, string outputFormat)
        {
            ISparqlResultsWriter resultsWriter = GetSparqlResultsWriter(outputFormat);
            resultsWriter.Save(_latestQueryResults, output);
        }

        private static ISparqlResultsWriter GetSparqlResultsWriter(string outputType)
        {
            ISparqlResultsWriter resultsWriter;
            switch (outputType)
            {
                case "json":
                    resultsWriter = new SparqlJsonWriter();
                    break;
                case "rdf":
                    resultsWriter = new SparqlRdfWriter();
                    break;
                case "tsv":
                    resultsWriter = new SparqlTsvWriter();
                    break;
                case "html":
                    resultsWriter = new SparqlHtmlWriter();
                    break;
                case "xml":
                    resultsWriter = new SparqlXmlWriter();
                    break;
                case "csv":
                default:
                    resultsWriter = new SparqlCsvWriter();
                    break;
            }
            return resultsWriter;
        }
    }
}
