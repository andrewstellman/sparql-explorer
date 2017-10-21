using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparqlExplorer.Model
{
    using System.IO;
    using System.Reflection;

    using VDS.RDF;
    using VDS.RDF.Query;

    class SparqlExplorerModel
    {
        private readonly GraphManager _graphManager = new GraphManager();

        private readonly TemplateManager _templateManager = new TemplateManager();

        public int GraphNodeCount { get { return _graphManager.GraphNodeCount; } }

        public int GraphPrefixCount { get { return _graphManager.GraphPrefixCount; } }

        public IEnumerable<string> Prefixes { get { return _graphManager.Prefixes; } }

        public IEnumerable<string> TemplateNames { get { return _templateManager.TemplateNames; } }

        const string X509_CERT_FILENAME = "sparqlex.x509";
        private readonly string _x509certFilename;
        private readonly string _x509keyFilename;
        public bool X509CertificatesAvailable
        {
            get { return (File.Exists(_x509certFilename) && File.Exists(_x509keyFilename)); }
        }

        public SparqlExplorerModel()
        {
            string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (File.Exists(assemblyLocation + "\\" + X509_CERT_FILENAME + ".cert") && 
                File.Exists(assemblyLocation + "\\" + X509_CERT_FILENAME + ".key"))
            {
                _x509certFilename = assemblyLocation + "\\" + X509_CERT_FILENAME + ".cert";
                _x509keyFilename = assemblyLocation + "\\" + X509_CERT_FILENAME + ".key";
                _graphManager.X509Certificate = X509CertificateFileReader.ReadX509Certificate(_x509certFilename, _x509keyFilename);
            }
        }

        public async Task ExecuteRestApiQuery(string url)
        {
            await _graphManager.ExecuteRestApiQuery(url);
        }

        public IEnumerable<object> ExecuteSparqlQuery(string query, out IEnumerable<string> columnNames)
        {
            IEnumerable<object> rows;
            try
            {
                rows = _graphManager.ExecuteSparqlQuery(query, out columnNames);
            }
            catch (Exception ex)
            {
                columnNames = new List<string>() { "Error" };
                rows = ResultSetHelper.CreateErrorRows(ex);
            }
            return rows;
        }


        public string SaveGraph(string filename)
        {
            return _graphManager.SaveGraph(filename);
        }

        public string SaveSparqlQueryResults(string filename)
        {
            return _graphManager.SaveSparqlQueryResults(filename);
        }

        public string GetTemplateText(string templateName)
        {
            return _templateManager.GetTemplateText(templateName);
        }

        public void LoadGraphFromFile(string filename)
        {
            _graphManager.LoadGraphFromFile(filename);
        }

        public string X509CertificateInfo {
            get
            {
                if (X509CertificatesAvailable)
                {
                    return String.Format(@"X.509 certificate files {0}.cert and {0}.key were found, and will be used when making REST API requests. Certificate info is below.

Certificate subject:
{1}

Issuer subject:
{2}",
                                X509_CERT_FILENAME, _graphManager.X509Certificate.Subject, _graphManager.X509Certificate.Issuer);
                }
                else
                {
                    return String.Format(@"An X.509 certificate and key file was found.
To use X.509 certificates, put files named {0}.cert and {0}.key in the same folder as {1}.",
                            X509_CERT_FILENAME, Path.GetFileName(Assembly.GetExecutingAssembly().Location));
                }
            }
        }
    }
}
