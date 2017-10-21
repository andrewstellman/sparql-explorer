using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SparqlExplorer.View
{
    class WindowHelper
    {
        public static void DisableWindowAndDoWork(Window window, DoWorkEventHandler work)
        {
            Cursor cursor = window.Cursor;
            BackgroundWorker backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += work;
            backgroundWorker.RunWorkerCompleted +=
            delegate
            {
                window.Cursor = cursor;
                window.IsEnabled = true;
            };
            window.IsEnabled = false;
            window.Cursor = Cursors.Wait;
            backgroundWorker.RunWorkerAsync();
        }

        public static string ShowSaveDialog(string title, string filter, string defaultExt)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog();
            saveDialog.Title = title;
            saveDialog.Filter = filter;
            saveDialog.CheckFileExists = false;
            saveDialog.CheckPathExists = true;
            saveDialog.OverwritePrompt = true;
            saveDialog.DefaultExt = defaultExt;
            if (saveDialog.ShowDialog() == true)
            {
                return saveDialog.FileName;
            }
            else
            {
                return null;
            }
        }

        public static string ShowOpenDialog(string title, string filter, string defaultExt)
        {
            var openDialog = new Microsoft.Win32.OpenFileDialog();
            openDialog.Title = title;
            openDialog.Filter = filter;
            openDialog.CheckFileExists = true;
            openDialog.CheckPathExists = true;
            openDialog.DefaultExt = defaultExt;
            if (openDialog.ShowDialog() == true)
            {
                return openDialog.FileName;
            }
            else
            {
                return null;
            }
        }
    }
}
