using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace resxgen
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> src = new List<string>();
            string sep = "=";
            string dir = Directory.GetCurrentDirectory();
            string outdir = dir;
            bool inv = false;
            if (args.Length < 1)
            {
                Console.WriteLine($"Usage: {Process.GetCurrentProcess().ProcessName} [-dir <source dir>] [-outdir <target dir>] [-sep <seperate character>] <dictionaries...>");
                return;
            }
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg == "-sep")
                {
                    if (i == args.Length - 1)
                    {
                        Console.Error.WriteLine("Seperator missing");
                        return;
                    }
                    sep = args[++i];
                }
                else if (arg == "-i")
                {
                    inv = true;
                }
                else if (arg == "-dir")
                {
                    if (i == args.Length - 1)
                    {
                        Console.Error.WriteLine("Directory missing");
                        return;
                    }
                    dir = args[++i];
                }
                else if (arg == "-outdir")
                {
                    if (i == args.Length - 1)
                    {
                        Console.Error.WriteLine("Output directory missing");
                        return;
                    }
                    outdir = args[++i];
                } 
                else
                {
                    if (arg.ToLowerInvariant().EndsWith(".txt"))
                    {
                        // remove the extension
                        arg = Path.GetFileNameWithoutExtension(arg);
                    }
                    else
                    {
                        arg = Path.GetFileName(arg);
                    }
                    src.Add(arg);
                }
            }
            Console.WriteLine($"dir: {dir}");
            Console.WriteLine($"outdir: {outdir}");
            Console.WriteLine($"sep: {sep}\n");
            foreach (var dict in src)
            {
                if (inv)
                {
                    Resx2Text(dir, outdir, sep, dict);
                }
                else
                {
                    Text2Resx(dir, outdir, sep, dict);
                }
            }
        }

        private static void Resx2Text(string dir, string outdir, string sep, string dict)
        {
            throw new NotImplementedException();
        }

        private static void Text2Resx(string dir, string outdir, string sep, string dict)
        {
            // make up source filename
            var path = Path.Combine(dir, $"{dict}.txt");
            if (!File.Exists(path))
            {
                Console.WriteLine($"Dictionary not found: {dict}.txt");
                return;
            }
            var files = new List<string>() { path };
            files.AddRange(Directory.GetFiles(dir, $"{dict}.*.txt", SearchOption.TopDirectoryOnly));
            foreach (var file in files)
            {
                var strings = TextParser.Parse(file, sep);
                if (strings == null)
                {
                    continue;
                }
                string filename = Path.GetFileNameWithoutExtension(file);
                Console.WriteLine($"Dictionary {filename} parsed");
                var resxGen = new ResxGenerator();
                resxGen.AddRecords(strings);
                var resxPath = Path.Combine(outdir, $"{filename}.resx");
                resxGen.Save(resxPath);
                Console.WriteLine($"{resxPath} saved");
            }
        }
    }
}
