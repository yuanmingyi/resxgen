using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace resxgen
{
    class TextParser
    {
        public static Dictionary<string, string> Parse(string filepath, string sep = "=")
        {
            var dict = new Dictionary<string, string>();
            int lineno = 0;
            foreach (var line in File.ReadLines(filepath))
            {
                lineno++;
                if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                {
                    // skip the comments and empty lines
                    continue;
                }
                var idx = line.IndexOf(sep);
                if (idx < 0)
                {
                    Console.Error.WriteLine($"Invalid format in line {lineno}: {line}");
                    return null;
                }
                dict.Add(line.Substring(0, idx), line.Substring(idx+1));
            }
            return dict;
        }

        public static void Dump(Dictionary<string, string> records, string filepath, string sep = "=")
        {
            var lines = new List<string>();
            foreach (var rec in records)
            {
                lines.Add($"{rec.Key}{sep}{rec.Value}\n");
            }
            File.WriteAllLines(filepath, lines);
        }
    }
}
