using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SparqlExplorer.ViewModel
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event EventHandler<ColumnNamesUpdatedEventArgs> ColumnNamesUpdated;

        private readonly Model.SparqlExplorerModel _model = new Model.SparqlExplorerModel();

        public string QueryUrl { get; set; }

        private string _graphLoadErrorMessage = null;
        public string GraphLoadedMessage
        {
            get
            {
                if (!string.IsNullOrEmpty(_graphLoadErrorMessage))
                    return _graphLoadErrorMessage;
                else if (_model.GraphNodeCount > 0)
                    return "Loaded " + _model.GraphNodeCount + " nodes (" + _model.GraphPrefixCount + " prefixes)";
                else
                    return "No graph loaded";
            }
        }

        public IEnumerable<string> Prefixes { get { return _model.Prefixes; } }

        public bool CanRunSparqlQuery { 
            get { return _model.GraphNodeCount > 0 && !EditingQueryTemplate; } 
        }

        private string _sparqlQuery;
        public string SparqlQuery {
            get { return _sparqlQuery; }
            set
            {
                _sparqlQuery = value;
                OnPropertyChanged("SparqlQuery");
            }
        }

        private bool _editingQueryTemplate = false;
        public bool EditingQueryTemplate 
        {
            get { return _editingQueryTemplate; }
            set
            {
                _editingQueryTemplate = value;
                OnPropertyChanged("CanRunSparqlQuery");
            }
        }

        public IEnumerable<object> QueryResults { get; set; }
        public bool ResultsLoaded { get { return QueryResults != null && QueryResults.Count() > 0; } }
        public string RowsShowing
        {
            get
            {
                if (ResultsLoaded)
                    return String.Format("({0:d} rows)", QueryResults.Count());
                else
                    return "(no rows showing)";
            }
        }

        public string X509CertificateLinkText { get { return _model.X509CertificatesAvailable ? "Using X.509 certificate" : "No X.509 certificate found"; } }

        public string X509CertificateMessage { get { return _model.X509CertificateInfo; } }

        public IEnumerable<string> TemplateNames { get { return _model.TemplateNames; } }



        public MainWindowViewModel()
        {
            QueryUrl = "http://stellman-greene.com/sample-rdf.ttl";
            SparqlQuery = @"SELECT *
{
    ?s ?p ?o.
}
LIMIT 100";
            OnPropertyChanged("GraphLoadedMessage");
        }

        public async Task ExecuteRestApiQuery()
        {
            try
            {
                _graphLoadErrorMessage = null;
                await _model.ExecuteRestApiQuery(QueryUrl);
                OnPropertyChanged("GraphLoadedMessage");
                OnPropertyChanged("CanRunSparqlQuery");
            }
            catch (Model.GraphReadException ex)
            {
                System.Windows.MessageBox.Show(ex.Message
                    + Environment.NewLine + ex.InnerException.Message, 
                    "Invalid RDF returned from REST API");
                _graphLoadErrorMessage = "Invalid RDF: " + ex.Message;
                OnPropertyChanged("GraphLoadedMessage");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Unable to read data from REST API " + QueryUrl
                    + Environment.NewLine + ex.Message, "Web request error");
                _graphLoadErrorMessage = "Web error: " + ex.Message;
                OnPropertyChanged("GraphLoadedMessage");
            }
        }

        public void LoadGraphFromFile(string filename)
        {
            _model.LoadGraphFromFile(filename);
            OnPropertyChanged("CanRunSparqlQuery");
            OnPropertyChanged("GraphLoadedMessage");
        }

        public void ExecuteSparqlQuery()
        {
            IEnumerable<string> columnNames;
            QueryResults = _model.ExecuteSparqlQuery(SparqlQuery, out columnNames);
            OnPropertyChanged("QueryResults");
            OnPropertyChanged("ResultsLoaded");
            OnPropertyChanged("RowsShowing");

            if (ColumnNamesUpdated != null)
            {
                EventHandler<ColumnNamesUpdatedEventArgs> columnNamesUpdated = ColumnNamesUpdated;
                Application.Current.Dispatcher.Invoke(
                    new Action(() =>
                        {
                            columnNamesUpdated(this, new ColumnNamesUpdatedEventArgs(columnNames));
                        })
                    );
            }
        }

#region Property changed event handler
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler propertyChangedEvent = PropertyChanged;
            if (propertyChangedEvent != null)
            {
                propertyChangedEvent(this, new PropertyChangedEventArgs(propertyName));
            }
        }
#endregion

        public string SaveSparqlQueryResults(string filename)
        {
            return _model.SaveSparqlQueryResults(filename);
        }

        public string SaveGraph(string filename)
        {
            return _model.SaveGraph(filename);
        }

        public string GetTemplateText(string templateName)
        {
            return _model.GetTemplateText(templateName);
        }
    }

}
