using System;
using System.Collections.Generic;
using System.IO;

namespace resxgen
{
    class TextParser
    {
        public static List<Data> Parse(string filepath, string sep = "=")
        {
            var dict = new List<Data>();
            var lines = File.ReadAllLines(filepath);
            string comment = null;
            for (int lineno = 0; lineno < lines.Length; lineno++)
            {
                var line = lines[lineno];
                if (line.StartsWith("#:", StringComparison.Ordinal))
                {
                    // add comment
                    comment = line.Substring(2);
                }
                else if (line.StartsWith("#", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(line))
                {
                    // skip the comments and empty lines
                    comment = null;
                }
                else
                {
                    var idx = line.IndexOf(sep, StringComparison.Ordinal);
                    if (idx < 0)
                    {
                        Console.Error.WriteLine($"Invalid format in line {lineno+1}: {line}");
                        return null;
                    }
                    dict.Add(new Data(line.Substring(0, idx), line.Substring(idx + 1), comment));
                    comment = null;
                }
            }
            return dict;
        }

        public static void Dump(List<Data> records, string filepath, string sep = "=", bool extraLine = false)
        {
            var lines = new List<string>();
            foreach (var rec in records)
            {
                if (!string.IsNullOrEmpty(rec.Comment))
                {
                    // add comment
                    lines.Add($"#:{rec.Comment}");
                }
                lines.Add($"{rec.Name}{sep}{rec.Value}{(extraLine ? "\n" : "")}");
            }
            File.WriteAllLines(filepath, lines);
        }
    }
}
