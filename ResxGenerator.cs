using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Resources.Tools;
using System.Xml;

namespace resxgen
{
    class ResxGenerator
    {
        private Dictionary<string, Data> _data;

        public ResxGenerator()
        {
            _data = new Dictionary<string, Data>();
        }

        public static List<Data> ReadRecords(string filepath)
        {
            ITypeResolutionService typeres = null;
            var data = new List<Data>();
            using (var rr = new ResXResourceReader(filepath))
            {
                rr.UseResXDataNodes = true;
                foreach (DictionaryEntry d in rr)
                {
                    var node = (ResXDataNode)d.Value;
                    data.Add(new Data(node.Name, (string)node.GetValue(typeres), node.Comment));
                }
            }
            return data;
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
                if (_data.ContainsKey(rec.Name))
                {
                    Console.WriteLine($"[Warning] IGNORED resource key \"{rec.Name}\" since already exists!!");
                }
                else
                {
                    _data.Add(rec.Name, new Data(rec.Name, value, rec.Comment));
                }
            }
        }

        public void Save(string resxPath)
        {
            using (var rw = new ResXResourceWriter(resxPath))
            {
                foreach (var item in _data.Values)
                {
                    rw.AddResource(new ResXDataNode(item.Name, item.Value) { Comment = item.Comment });
                }
                rw.Generate();
            }
        }

        public void SaveDesignerClass(string outdir, string className, string ns)
        {
            var designerCsPath = Path.Combine(outdir, $"{className}.Designer.cs");
            string[] errors;
            var codeProvider = new Microsoft.CSharp.CSharpCodeProvider();
            var data = _data.ToDictionary(d => d.Key, d => d.Value.Value);
            var code = StronglyTypedResourceBuilder.Create(data, className, ns, codeProvider, false, out errors);
            if (errors.Length > 0)
            {
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }
            }
            using (StreamWriter writer = new StreamWriter(designerCsPath, false, System.Text.Encoding.UTF8))
            {
                codeProvider.GenerateCodeFromCompileUnit(code, writer, new CodeGeneratorOptions());
            }
        }
    }
}
