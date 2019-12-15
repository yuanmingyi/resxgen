using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace resxgen
{
    class ResxGenerator
    {
        private static string _templatePath = "Template.resx";
        private XmlDocument _doc;
        private XmlNode _root;

        public ResxGenerator()
        {
            _doc = new XmlDocument();
            _doc.Load(_templatePath);
            _root = _doc.SelectSingleNode("root");
        }

        public ResxGenerator(string filepath)
        {
            _doc = new XmlDocument();
            _doc.Load(filepath);
            _root = _doc.SelectSingleNode("root");
        }

        public void AddRecords(Dictionary<string, string> data)
        {
            foreach (var rec in data)
            {
                AddRecord(rec.Key, rec.Value);
            }
        }

        public Dictionary<string, string> ReadRecords()
        {
            var records = new Dictionary<string, string>();
            foreach (XmlElement node in _root.ChildNodes)
            {
                if (node.Name == "data")
                {
                    var key = node.GetAttribute("name");
                    string value = "";
                    foreach (XmlElement n in node.ChildNodes)
                    {
                        if (n.Name == "value")
                        {
                            value = n.InnerText;
                        }
                    }
                    records.Add(key, value);
                }
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
        public void AddRecord(string key, string value, string comment = null)
        {
            var rec = _doc.CreateElement("data");
            var xelName = _doc.CreateAttribute("name");
            var xelSpace = _doc.CreateAttribute("xml:space");
            xelName.InnerText = key;
            xelSpace.InnerText = "preserve";
            rec.SetAttributeNode(xelName);
            rec.SetAttributeNode(xelSpace);
            AddElement(rec, "value", value);
            if (!string.IsNullOrEmpty(comment))
            {
                AddElement(rec, "comment", comment);
            }
            _root.AppendChild(rec);
        }

        public void Save(string path)
        {
            _doc.Save(path);
        }

        private XmlElement AddElement(XmlElement root, string key, string value)
        {
            var ele = _doc.CreateElement(key);
            ele.InnerText = value;
            root.AppendChild(ele);
            return ele;
        }
    }
}
