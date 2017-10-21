using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparqlExplorer.ViewModel
{
    class ColumnNamesUpdatedEventArgs : EventArgs
    {
        public IEnumerable<string> ColumnNames { get; private set; }
        public ColumnNamesUpdatedEventArgs(IEnumerable<string> columnNames)
        {
            ColumnNames = columnNames;
        }
    }
}
