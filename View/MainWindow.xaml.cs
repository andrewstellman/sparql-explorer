using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SparqlExplorer.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModel.MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = FindResource("viewModel") as ViewModel.MainWindowViewModel;
            _viewModel.ColumnNamesUpdated += _viewModel_ColumnNamesUpdated;

            foreach (string templateName in _viewModel.TemplateNames)
            {
                templateSelector.Items.Add(templateName);
            }

            Title += " v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        void _viewModel_ColumnNamesUpdated(object sender, ViewModel.ColumnNamesUpdatedEventArgs e)
        {
            UpdateColumnNames(e.ColumnNames);
            UpdateListViewContextMenu(e.ColumnNames);
        }

        private void UpdateListViewContextMenu(IEnumerable<string> columnNames)
        {
            listViewContextMenu.Items.Clear();
            if (columnNames != null)
            {
                foreach (string columnName in columnNames)
                {
                    var menuItem = new MenuItem() { Header = columnName };
                    listViewContextMenu.Items.Add(menuItem);
                    var menuSubItem = new MenuItem() { Header = "Copy value to clipboard" };
                    menuItem.Items.Add(menuSubItem);
                    menuSubItem.Click += CopyValueToClipboardClick;
                }
            }
        }

        private void CopyPrefixesToClipboardClick(object sender, RoutedEventArgs e)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            foreach (string prefix in _viewModel.Prefixes)
            {
                sb.AppendLine(prefix.ToString());
                i++;
            }
            System.Windows.Clipboard.SetText(sb.ToString());
            System.Windows.Forms.MessageBox.Show(i + " prefixes copied to the clipboard", "Copying prefixes to the clipboard");
        }

        private void CopyValueToClipboardClick(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null && menuItem.Header != null
                && !String.IsNullOrWhiteSpace(menuItem.Header.ToString()))
            {
                MenuItem parent = menuItem.Parent as MenuItem;
                if (parent != null && parent.Header != null
                    && !String.IsNullOrWhiteSpace(parent.Header.ToString()))
                {
                    string column = parent.Header.ToString();
                    System.Windows.Clipboard.SetText(GetSelectedItemValue(column));
                }
            }
        }

        private string GetSelectedItemValue(string column)
        {
            IDictionary<string, object> container = listView.SelectedItem as IDictionary<string, object>;
            if (container != null)
            {
                if (container.ContainsKey(column))
                {
                    var value = container[column];
                    if (value != null)
                        return value.ToString();
                }
            }
            return String.Empty;
        }

        private void UpdateColumnNames(IEnumerable<string> columnNames)
        {
            listViewView.Columns.Clear();
            if (columnNames != null)
            {
                foreach (string columnName in columnNames)
                {
                    listViewView.Columns.Add(
                        new GridViewColumn()
                        {
                            Header = columnName,
                            DisplayMemberBinding = new Binding(columnName)
                            {
                                TargetNullValue = "[unbound]"
                            },
                            Width = listView.ActualWidth / columnNames.Count(),
                        }
                        );
                }
            }
        }

        private async void QueryRestApiButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor cursor = this.Cursor;
            this.Cursor = Cursors.Wait;
            this.IsEnabled = false;

            await _viewModel.ExecuteRestApiQuery();

            this.IsEnabled = true;
            this.Cursor = cursor;
        }

        private void LoadGraphFromFileButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor cursor = this.Cursor;
            this.Cursor = Cursors.Wait;
            this.IsEnabled = false;

            string filename = WindowHelper.ShowOpenDialog("Choose a file to load the graph from",
                "RDF XML|*.xml|Turtle|*.ttl|NTriples|*.nt|SPARQL JSON|*.json|All Files (*.*)|*.*",
                "*.xml");
            if (!string.IsNullOrWhiteSpace(filename))
            {
                _viewModel.LoadGraphFromFile(filename);
            }

            this.IsEnabled = true;
            this.Cursor = cursor;
        }

        private void ExecuteSparqlButton_Click(object sender, RoutedEventArgs e)
        {
            DoWorkEventHandler work = delegate { _viewModel.ExecuteSparqlQuery(); };
            WindowHelper.DisableWindowAndDoWork(this, work);
        }

        private void SaveGraphClick(object sender, RoutedEventArgs e)
        {
            string filename = WindowHelper.ShowSaveDialog(
                "Save entire graph to a file",
                "CSV|*.csv|Turtle|*.ttl|NTriples|*.nt|Tab-delimited|*.tsv|HTML|*.html|SPARQL JSON|*.json|GraphViz DOT|*.dot",
                "ttl"
                );
            if (!String.IsNullOrWhiteSpace(filename))
            {
                DoWorkEventHandler work = delegate { 
                    string message = _viewModel.SaveGraph(filename);
                    System.Windows.MessageBox.Show(message, "Saving entire graph to a file");
                };
                WindowHelper.DisableWindowAndDoWork(this, work);
            }
        }

        private void SaveSparqlQueryResultsClick(object sender, RoutedEventArgs e)
        {
            string filename = WindowHelper.ShowSaveDialog(
                "Save SPARQL query results to a file",
                "CSV|*.csv|Tab-delimited|*.tsv|HTML|*.html|RDF|*.rdf|SPARQL JSON|*.json|RDF/XML|*.xml",
                "rdf"
                );
            if (!String.IsNullOrWhiteSpace(filename))
            {
                DoWorkEventHandler work = delegate { 
                    string message = _viewModel.SaveSparqlQueryResults(filename);
                    System.Windows.MessageBox.Show(message, "Saving SPARQL query results to a file");

                };
                WindowHelper.DisableWindowAndDoWork(this, work);
            }
        }

        private void AboutClick(object sender, RoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.ShowDialog();
        }

        private void NavigateLinkFromButton(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.Content != null) {
                try
                {
                    Uri uri = new Uri(button.Content.ToString());
                    System.Diagnostics.Process.Start(uri.ToString());
                }
                catch
                {
                    // we're only navigating from page links, so bury failures
                }
            }
        }

        private void TemplateSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string templateName = e.AddedItems[0].ToString();
            templateSelector.Clear();
            templateEditor.TemplateText = _viewModel.GetTemplateText(templateName);
            templateEditor.Visibility = Visibility.Visible;
            _viewModel.EditingQueryTemplate = true;
        }

        private void TemplateEditor_Ok(object sender, EventArgs e)
        {
            _viewModel.SparqlQuery = templateEditor.Text;
            templateEditor.Visibility = Visibility.Hidden;
            _viewModel.EditingQueryTemplate = false;
        }

        private void TemplateEditor_Cancel(object sender, EventArgs e)
        {
            templateEditor.Visibility = Visibility.Hidden;
            _viewModel.EditingQueryTemplate = false;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count >= 1)
            {
                IDictionary<string, object> row = e.AddedItems[0] as IDictionary<string, object>;
                if (row != null)
                {
                    List<string> items = new List<string>();
                    foreach (object item in row.Values)
                    {
                        if (item != null)
                            items.Add(item.ToString());
                    }
                    templateEditor.Items = items;
                }
            }
        }

        private void X509CertificateLinkClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show(_viewModel.X509CertificateMessage, "X.509 Certificates");
        }

    }
}
