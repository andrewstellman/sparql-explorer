using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SparqlExplorer.View
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Interaction logic for SparqlTemplateEditor.xaml
    /// </summary>
    public partial class SparqlTemplateEditor : UserControl
    {
        public SparqlTemplateEditor()
        {
            InitializeComponent();
        }

        private const int COMBOBOX_WIDTH = 200;
        private const string COMBOBOX_INITIAL_DROPDOWN_ITEM = "Enter a value or click a row in the query results to populate this list";

        public event EventHandler Ok;
        public event EventHandler Cancel;

        private string _templateText = null;
        public string TemplateText
        {
            get { return _templateText; }
            set
            {
                _templateText = value;
                UpdateControlFromTemplateText();
            }
        }

        private void UpdateControlFromTemplateText()
        {
            TemplateStackPanel.Children.Clear();
            if (!String.IsNullOrWhiteSpace(_templateText))
            {
                string[] lines = _templateText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (string line in lines)
                {
                    AddLine(line);
                }
                UpdateComboBoxItems();
            }
        }

        private readonly List<string> _items = new List<string>();
        public IEnumerable<string> Items
        {
            get { return _items.ToList<string>(); }
            set
            {
                _items.Clear();
                _items.AddRange(value);
                UpdateComboBoxItems();
            }
        }

        private void UpdateComboBoxItems()
        {
            foreach (UIElement uiElement in TemplateStackPanel.Children)
            {
                StackPanel lineStackPanel = uiElement as StackPanel;
                if (lineStackPanel != null)
                {
                    foreach (UIElement panelElement in lineStackPanel.Children)
                    {
                        ComboBoxWithCueBanner comboBox = panelElement as ComboBoxWithCueBanner;
                        if (comboBox != null)
                        {
                            string savedText = comboBox.Text;
                            comboBox.Items.Clear();
                            if (_items.Count() > 0)
                            {
                                foreach (string item in _items)
                                    comboBox.Items.Add(item);
                            }
                            else
                            {
                                comboBox.Items.Add(COMBOBOX_INITIAL_DROPDOWN_ITEM);
                            }
                            comboBox.Text = savedText;
                        }
                    }
                }
            }
        }

        private void AddLine(string line)
        {
            StackPanel stackPanelToAdd = new StackPanel();
            stackPanelToAdd.Orientation = Orientation.Horizontal;

            string pattern = @"(.*?)~~(\S+?)~~";
            Match matches = Regex.Match(line, pattern);
            while (matches.Success)
            {
                string textBeforeKeyword = matches.Groups[1].Value;
                if (!string.IsNullOrEmpty(textBeforeKeyword))
                {
                    TextBlock beforeTextBlock = new TextBlock();
                    beforeTextBlock.Text = textBeforeKeyword;
                    stackPanelToAdd.Children.Add(beforeTextBlock);
                }

                string keyword = matches.Groups[2].Value;
                ComboBoxWithCueBanner comboBox = new ComboBoxWithCueBanner();
                comboBox.Width = COMBOBOX_WIDTH;
                comboBox.CueBanner = keyword;
                stackPanelToAdd.Children.Add(comboBox);

                line = line.Substring(matches.Groups[0].Value.Length);
                matches = Regex.Match(line, pattern);
            }

            TextBlock afterTextBlock = new TextBlock();
            afterTextBlock.Text = line;
            stackPanelToAdd.Children.Add(afterTextBlock);

            TemplateStackPanel.Children.Add(stackPanelToAdd);
        }

        public string Text
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                foreach (UIElement uiElement in TemplateStackPanel.Children)
                {
                    StackPanel lineStackPanel = uiElement as StackPanel;
                    if (lineStackPanel != null)
                    {
                        foreach (UIElement panelElement in lineStackPanel.Children)
                        {
                            ConvertPanelElementToText(sb, panelElement);
                        }
                    }
                    sb.AppendLine();
                }
                return sb.ToString();
            }
        }

        private static void ConvertPanelElementToText(StringBuilder sb, UIElement panelElement)
        {
            if (panelElement is TextBlock)
            {
                sb.Append(((TextBlock)panelElement).Text);
            }
            else if (panelElement is ComboBoxWithCueBanner)
            {
                string comboBoxValue = ((ComboBoxWithCueBanner)panelElement).Text;
                Uri uri;
                if (Uri.TryCreate(comboBoxValue, UriKind.Absolute, out uri))
                    comboBoxValue = "<" + comboBoxValue + ">";
                sb.Append(comboBoxValue);
            }
        }

        private void OkButton(object sender, RoutedEventArgs e)
        {
            EventHandler ok = Ok;
            if (ok != null)
                ok(this, EventArgs.Empty);
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            EventHandler cancel = Cancel;
            if (cancel != null)
                cancel(this, EventArgs.Empty);
        }
    }
}
