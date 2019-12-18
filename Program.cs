using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace resxgen
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine(ArgumentsParser.Usage(Process.GetCurrentProcess().ProcessName));
                return;
            }

            ArgumentsParser parser = new ArgumentsParser(args);
            if (parser.Error != null)
            {
                Console.Error.WriteLine(parser.Error);
                return;
            }

            Console.WriteLine($"dir: {parser.Dir}");
            Console.WriteLine($"outdir: {parser.Outdir}");
            Console.WriteLine($"Inverse: {parser.Inverse}");
            Console.WriteLine($"ExtraLine: {parser.ExtraLine}");
            Console.WriteLine($"sep: {parser.Sep}");
            Console.WriteLine($"Use empty text type: {parser.EmptyType}\n");
            foreach (var dict in parser.Dicts)
            {
                if (parser.Inverse)
                {
                    Resx2Text(parser.Dir, parser.Outdir, parser.Sep, dict, parser.ExtraLine);
                }
                else
                {
                    Text2Resx(parser.Dir, parser.Outdir, parser.Sep, dict, parser.EmptyType);
                }
            }
        }

        private static List<string> GetAllLanguageDictionaries(string filebase, string dir, string ext)
        {
            var files = new List<string>();
            if (dir == "")
            {
                dir = Path.GetDirectoryName(filebase);
                filebase = Path.GetFileName(filebase);
            }
            // make up source filename
            var path = Path.Combine(dir, $"{filebase}{ext}");
            if (!File.Exists(path))
            {
                Console.WriteLine($"Dictionary not found: {path}");
                return files;
            }
            files.Add(path);
            files.AddRange(Directory.GetFiles(dir, $"{filebase}.*{ext}", SearchOption.TopDirectoryOnly));
            return files;
        }

        private static void Resx2Text(string dir, string outdir, string sep, string dict, bool extraLine)
        {
            var filepaths = GetAllLanguageDictionaries(dict, dir, ".resx");
            foreach (var path in filepaths)
            {
                var resxGen = new ResxGenerator(path);
                var records = resxGen.ReadRecords();
                if (records == null || records.Count == 0)
                {
                    continue;
                }
                string filebase = Path.GetFileNameWithoutExtension(path);
                Console.WriteLine($"Dictionary {filebase} parsed");
                if (outdir == "")
                {
                    outdir = Path.GetDirectoryName(path);
                }
                var textPath = Path.Combine(outdir, $"{filebase}.txt");
                TextParser.Dump(records, textPath, sep, extraLine);
                Console.WriteLine($"{textPath} saved");
            }
        }

        private static void Text2Resx(string dir, string outdir, string sep, string dict, int emptyType)
        {
            var filepaths = GetAllLanguageDictionaries(dict, dir, ".txt");
            foreach (var path in filepaths)
            {
                var resxGen = new ResxGenerator();
                var records = TextParser.Parse(path, sep);
                if (records == null || records.Count == 0)
                {
                    continue;
                }
                string filebase = Path.GetFileNameWithoutExtension(path);
                Console.WriteLine($"Dictionary {filebase} parsed");
                if (outdir == "")
                {
                    outdir = Path.GetDirectoryName(path);
                }
                var resxPath = Path.Combine(outdir, $"{filebase}.resx");
                resxGen.AddRecords(records, emptyType);
                resxGen.Save(resxPath);
                Console.WriteLine($"{resxPath} saved");
            }
        }
    }
}
