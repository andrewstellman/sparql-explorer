using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparqlExplorer.Model
{
    class GraphReadException : Exception
    {
        public GraphReadException(string message, Exception innerException) : base(message, innerException) { }
    }
}
