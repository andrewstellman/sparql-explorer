using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sparqlex
{
    using CommandLine;
    using CommandLine.Parsing;
    using SparqlExplorer.Model;
    using System.IO;
    using System.Reflection;

    class Program
    {
        private static readonly GraphManager _graphManager;
        private static readonly string _assemblyFolder;

        static Program()
        {
            _assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _graphManager = new GraphManager(_assemblyFolder);
        }

        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                // consume Options instance properties
                if (options.Input != null && options.Input.Count == 1 && !String.IsNullOrEmpty(options.Input[0]))
                {
                    ReadData(options);

                    GetX509Certificates(options);

                    WriteOutput(options);
                }
                else
                    Console.Error.WriteLine(options.GetUsage());
            }

        }

        private static void GetX509Certificates(Options options)
        {
            if (string.IsNullOrWhiteSpace(options.X509CertificateFile) && string.IsNullOrWhiteSpace(options.X509CertificateKey))
                return;

            if (string.IsNullOrWhiteSpace(options.X509CertificateFile) || !File.Exists(options.X509CertificateFile))
            {
                Console.Error.WriteLine("WARNING: If specifying an X.509 certificate file, you must also specify a valid key file");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(options.X509CertificateKey) || !File.Exists(options.X509CertificateKey))
            {
                Console.Error.WriteLine("WARNING: If specifying an X.509 key file, you must also specify a valid certificate file");
                return;
            }

            if (File.Exists(options.X509CertificateFile) && File.Exists(options.X509CertificateKey))
            {
                _graphManager.X509Certificate = X509CertificateFileReader.ReadX509Certificate(options.X509CertificateFile, options.X509CertificateKey);
            }
        }

        private static void WriteOutput(Options options)
        {
            if (!String.IsNullOrWhiteSpace(options.SparqlQuery))
            {
                ExecuteQuery(options);
            }
            else
            {
                WriteGraph(options);
            }
        }

        private static void ReadData(Options options)
        {
            string input = options.Input[0];
            Uri result;
            if (Uri.TryCreate(input, UriKind.Absolute, out result))
            {
                _graphManager.ExecuteRestApiQuery(result.ToString(), false);
            }
            else
            {
                _graphManager.LoadGraphFromFile(input);
            }
        }

        private static void ExecuteQuery(Options options)
        {
            IEnumerable<string> columnNames;
            IEnumerable<object> results = _graphManager.ExecuteSparqlQuery(options.SparqlQuery, out columnNames);
            if (String.IsNullOrWhiteSpace(options.OutputFilename))
            {
                StringBuilder sb = new StringBuilder();
                using (TextWriter textWriter = new StringWriter(sb))
                {
                    _graphManager.StreamSparqlQueryResults(textWriter, options.OutputFormat);
                }
                Console.WriteLine(sb.ToString());
            }
            else
            {
                _graphManager.SaveSparqlQueryResults(options.OutputFilename, options.OutputFormat);
            }
        }

        private static void WriteGraph(Options options)
        {
            if (String.IsNullOrWhiteSpace(options.OutputFilename))
            {
                StringBuilder sb = new StringBuilder();
                using (TextWriter textWriter = new StringWriter(sb))
                {
                    _graphManager.StreamGraph(textWriter, options.OutputFormat);
                }
                Console.WriteLine(sb.ToString());
            }
            else
            {
                _graphManager.SaveGraph(options.OutputFilename, options.OutputFormat);
                Console.WriteLine(_graphManager.GraphNodeCount + " nodes written to " + options.OutputFilename);
            }
        }
    }
}