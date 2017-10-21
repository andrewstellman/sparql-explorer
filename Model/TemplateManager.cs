using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparqlExplorer.Model
{
    using System.IO;
    using System.Reflection;

    class TemplateManager
    {
        private const string TEMPLATE_FOLDER_NAME = "Templates";
        private const string TEMPLATE_EXTENSION = ".txt";

        private readonly Dictionary<string, string> _templates = new Dictionary<string, string>();

        public IEnumerable<string> TemplateNames
        {
            get { return _templates.Keys; }
        }

        public TemplateManager(string templateFolderName = TEMPLATE_FOLDER_NAME, string templateExtension = TEMPLATE_EXTENSION)
        {
            string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string templateFolder = assemblyPath + "\\" + templateFolderName;
            if (Directory.Exists(templateFolder))
            {
                foreach (string file in Directory.GetFiles(templateFolder, "*" + TEMPLATE_EXTENSION))
                {
                    string templateName = Path.GetFileNameWithoutExtension(file);
                    try
                    {
                        _templates[templateName] = File.ReadAllText(file);
                    }
                    finally { }
                }
            }
        }

        public string GetTemplateText(string templateName)
        {
            if (!_templates.ContainsKey(templateName))
                return String.Empty;
            else
                return _templates[templateName];
        }
    }
}
