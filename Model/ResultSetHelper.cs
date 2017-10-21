using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparqlExplorer.Model
{
    using System.Dynamic;
    using VDS.RDF;
    using VDS.RDF.Parsing;
    using VDS.RDF.Query;
    using VDS.RDF.Query.Datasets;

    static class ResultSetHelper
    {
        public static VDS.RDF.Query.SparqlResultSet ExecuteQuery(string query, IGraph graph, out IEnumerable<string> columnNames)
        {
            InMemoryDataset ds = new InMemoryDataset(graph);
            ISparqlQueryProcessor processor = new LeviathanQueryProcessor(ds);
            SparqlQueryParser sparqlParser = new SparqlQueryParser();
            SparqlParameterizedString queryString = new SparqlParameterizedString();
            queryString.CommandText = query;
            queryString.Namespaces = graph.NamespaceMap;
            SparqlResultSet results = graph.ExecuteQuery(queryString) as SparqlResultSet;
            columnNames = results.Variables.ToList();
            return results;
        }

        public static IEnumerable<object> ResultSetToExpandoObjects(SparqlResultSet resultSet, INamespaceMapper namespaceMapper, IEnumerable<string> columnNames)
        {
            List<object> rows = new List<object>();
            foreach (SparqlResult result in resultSet)
            {
                dynamic row = new ExpandoObject();
                IDictionary<string, object> dict = (IDictionary<string, object>)row;
                foreach (string column in columnNames)
                {
                    if (result.Variables.Contains(column))
                    {
                        string value = result[column].ToString();
                        string qname;
                        if (namespaceMapper.ReduceToQName(value, out qname))
                        {
                            value = qname;
                        }
                        dict.Add(column, value);
                    }
                    else
                    {
                        dict.Add(column, null);
                    }
                }
                rows.Add(row);
            }
            return rows;
        }

        internal static IEnumerable<object> CreateErrorRows(Exception ex)
        {
            List<object> rows = new List<object>();
            dynamic row = new ExpandoObject();
            IDictionary<string, object> dict = (IDictionary<string, object>)row;
            StringBuilder sb = new StringBuilder();
            if (ex is RdfParseException)
            {
                sb.AppendLine("Error parsing RDF");
                sb.AppendLine("Specific error:");
                sb.AppendLine(ex.Message);
            }
            else
            {
                sb.AppendLine("An error occurred");
                sb.AppendLine(ex.Message);
            }
            dict.Add("Error", sb.ToString());
            rows.Add(row);
            return rows;
        }
    }
}
