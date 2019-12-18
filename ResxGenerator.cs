using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

namespace resxgen
{
    class ResxGenerator
    {
        private static string _templateName = "Template.resx";
        private XmlDocument _doc;
        private XmlNode _root;

        public ResxGenerator()
        {
            var processPath = Process.GetCurrentProcess().MainModule.FileName;
            var templatePath = Path.Combine(Path.GetDirectoryName(processPath), _templateName);
            _doc = new XmlDocument();
            _doc.Load(templatePath);
            _root = _doc.SelectSingleNode("root");
        }

        public ResxGenerator(string filepath)
        {
            _doc = new XmlDocument();
            _doc.Load(filepath);
            _root = _doc.SelectSingleNode("root");
        }

        public void AddRecords(List<Data> data, int emptyType = 0)
        {
            foreach (var rec in data)
            {
                var value = rec.Value;
                if (string.IsNullOrEmpty(value))
                {
                    if (emptyType == 1)
                    {
                        // use Key as empty text
                        value = $"[{rec.Name}]";
                    }
                    else if (emptyType == 2)
                    {
                        // use Empty as empty text
                        value = "[EMPTY]";
                    }
                    // otherwise use the default "" value
                }
                AddRecord(rec.Name, value, rec.Comment);
            }
        }

        public List<Data> ReadRecords()
        {
            var records = new List<Data>();
            try
            {
                foreach (XmlNode node in _root.ChildNodes)
                {
                    if (node.Name == "data")
                    {
                        var name = ((XmlElement)node).GetAttribute("name");
                        string value = "", comment = "";
                        foreach (XmlNode n in node.ChildNodes)
                        {
                            if (n.Name == "value")
                            {
                                value = n.InnerText;
                            }
                            else if (n.Name == "comment")
                            {
                                comment = n.InnerText;
                            }
                        }
                        records.Add(new Data(name, value, comment));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Parse xaml file failed: {ex.Message}");
                Console.Error.WriteLine($"{ex.StackTrace}");
                return null;
            }
            return records;
        }

        /// <summary>
        /// the template of data is:
        /// <data name="Name" xml:space="preserve">
        ///     <value>Value</value>
        ///     <comment>Comment</comment>
        /// </data>
        /// </summary>
        public void AddRecord(string name, string value, string comment)
        {
            var rec = _doc.CreateElement("data");
            var xelName = _doc.CreateAttribute("name");
            var xelSpace = _doc.CreateAttribute("xml:space");
            xelName.InnerText = name;
            xelSpace.InnerText = "preserve";
            rec.SetAttributeNode(xelName);
            rec.SetAttributeNode(xelSpace);
            AddNode(rec, "value", value);
            if (!string.IsNullOrEmpty(comment))
            {
                AddNode(rec, "comment", comment);
            }
            _root.AppendChild(rec);
        }

        public void Save(string path)
        {
            _doc.Save(path);
        }

        private void AddNode(XmlElement parent, string name, string value)
        {
            var ele = _doc.CreateElement(name);
            ele.InnerText = value;
            parent.AppendChild(ele);
        }
    }
}
