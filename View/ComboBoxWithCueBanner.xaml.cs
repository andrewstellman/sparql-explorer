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
    /// <summary>
    /// Interaction logic for ComboBoxWithCueBanner.xaml
    /// </summary>
    public partial class ComboBoxWithCueBanner : UserControl
    {
        private bool _clearing = false;

        public ComboBoxWithCueBanner()
        {
            InitializeComponent();

            comboBox.GotFocus += GotFocusHandler;
            comboBox.LostFocus += LostFocusHandler;
            comboBox.SelectionChanged += comboBox_SelectionChanged;
            this.GotFocus += GotFocusHandler;
            this.LostFocus += LostFocusHandler;
            this.MouseDown += MouseDownHandler;
        }

        void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_clearing)
                return;

            SelectionChangedEventHandler selectionChanged = SelectionChanged;
            if (selectionChanged != null)
            {
                selectionChanged(this, e);
            }
        }

        public event SelectionChangedEventHandler SelectionChanged;

        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            comboBox.Focus();
        }

        private void GotFocusHandler(object sender, RoutedEventArgs e)
        {
            textBlock.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void LostFocusHandler(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(comboBox.Text))
                textBlock.Visibility = System.Windows.Visibility.Visible;
            else
                textBlock.Visibility = System.Windows.Visibility.Collapsed;
        }

        public string CueBanner
        {
            get { return textBlock.Text; }
            set { textBlock.Text = value; }
        }

        public ItemCollection Items
        {
            get { return comboBox.Items; }
        }

        public string Text
        {
            get { return comboBox.Text; }
            set { comboBox.Text = value; }
        }

        public void Clear()
        {
            _clearing = true;
            comboBox.SelectedIndex = -1;
            _clearing = false;
        }
    }
}
