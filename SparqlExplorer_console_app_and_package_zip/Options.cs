using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommandLine;
using CommandLine.Text;

[assembly: AssemblyLicense("This is open source software, licensed under the Apache License 2.0.","http://www.apache.org/licenses/LICENSE-2.0.txt")]

[assembly: AssemblyUsage(@" ","usage:","    sparqlex filename.ttl [options]","    sparqlex url [options]")]

namespace sparqlex
{
    /// <summary>
    /// Class for CommandLine library to process command line arguments
    /// </summary>
    /// <remarks>See https://commandline.codeplex.com/ and https://github.com/gsscoder/commandline/wiki/Quickstart for details</remarks>
    class Options
    {

        [Option('q', "query", Required = false, DefaultValue = null, MutuallyExclusiveSet = "query", 
            HelpText = "SPARQL query to execute.")]
        public string SparqlQuery { get; set; }

        [Option('i', "query-file", Required = false, DefaultValue = null, MutuallyExclusiveSet = "query", 
            HelpText = "File that contains the SPARQL query to execute.")]
        public string SparqlQueryFilename { get; set; }

        [Option('o', "output", Required = false, DefaultValue = null, 
            HelpText = "Output file to write (format is based on file extension; default is to write to stdout).")]
        public string OutputFilename { get; set; }

        [Option('f', "format", Required = false, DefaultValue = null, 
            HelpText = "The format of the output to write to the console (default is csv). Allowed graph formats: ttl, nt, tsv, html, json, dot, csv. Allowed query result formats: json, rdf, tsv, html, xml, csv.")]
        public string OutputFormat { get; set; }

        [Option('c', "x509-certificate", Required = false, DefaultValue = null,
            HelpText = "Filename of the X.509 certificate to use when issuing the request")]
        public string X509CertificateFile { get; set; }

        [Option('k', "x509-key", Required = false, DefaultValue = null,
            HelpText = "Filename of the X.509 key to use when issuing the request")]
        public string X509CertificateKey { get; set; }

        [ValueList(typeof(List<string>), MaximumElements = 1)]
        public IList<string> Input { get; set; }

        [HelpOption('h', "help")]
        public string GetUsage()
        {
            return HelpText.AutoBuild(new Options());
        }
    }

}
