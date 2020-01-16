using System.Collections.Generic;
using System.IO;

namespace resxgen
{
    class ArgumentsParser
    {
        public string Sep = "=";
        public string Dir = "";
        public string Outdir = "";
        public bool Inverse = false;
        public bool ExtraLine = false;
        public int EmptyType = 0;
        public List<string> Dicts = new List<string>();
        public string Namespace = "";

        public string Error { get; private set; }

        public static string Usage(string appName)
        {
            return $"Usage: {appName} [-inv] [-extra] [-empty [type]] [-ns <namespace>] [-dir <source dir>] [-outdir <target dir>] [-sep <seperate character>] <dictionaries...>";
        }

        public ArgumentsParser()
        {

        }

        public ArgumentsParser(string[] args)
        {
            Parse(args);
        }

        public void Parse(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg == "-sep")
                {
                    if (i == args.Length - 1)
                    {
                        Error = "Seperator missing";
                        return;
                    }
                    Sep = args[++i];
                }
                else if (arg.StartsWith("-i"))
                {
                    Inverse = true;
                }
                else if (arg.StartsWith("-d"))
                {
                    if (i == args.Length - 1)
                    {
                        Error = "Directory missing";
                        return;
                    }
                    Dir = args[++i];
                }
                else if (arg.StartsWith("-ns"))
                {
                    if (i == args.Length - 1)
                    {
                        Error = "Namespace missing";
                        return;
                    }
                    Namespace = args[++i];
                }
                else if (arg.StartsWith("-em"))
                {
                    if (i == args.Length - 1)
                    {
                        Error = "Empty text missing";
                        return;
                    }
                    EmptyType = int.Parse(args[++i]);
                }
                else if (arg.StartsWith("-o"))
                {
                    if (i == args.Length - 1)
                    {
                        Error = "Output directory missing";
                        return;
                    }
                    Outdir = args[++i];
                }
                else if (arg.StartsWith("-e"))
                {
                    ExtraLine = true;
                }
                else
                {
                    var dir = Path.GetDirectoryName(arg);
                    if (arg.ToLowerInvariant().EndsWith(".txt") || arg.ToLowerInvariant().EndsWith(".resx"))
                    {
                        // remove the extension
                        arg = Path.Combine(dir, Path.GetFileNameWithoutExtension(arg));
                    }
                    else
                    {
                        arg = Path.Combine(dir, Path.GetFileName(arg));
                    }
                    Dicts.Add(arg);
                }
            }
        }
    }
}
