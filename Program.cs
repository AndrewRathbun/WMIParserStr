using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace WMIParserStr
{
    class Program
    {
        public static Arguments CommandLine;
        public static string entry;
        public static List<Details> LConsumers = new List<Details>();
        public static List<Details> LEventFilters = new List<Details>();
        public static List<Details> LBindings = new List<Details>();

        static void Main(string[] args)
        {
            CommandLine = new Arguments(args);
            if ((!string.IsNullOrEmpty(CommandLine["input"])) && (File.Exists(CommandLine["input"])))
                entry = CommandLine["input"];
            else
                DisplayHelp();

            string data = File.ReadAllText(entry);

            Regex getNames = new Regex(@"(.*).Name=""(.*)""", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex strings = new Regex(@"[ -~]{3,}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var matchesStrings = strings.Matches(data);

            int longObjects = matchesStrings.Count;

            ProcessBindings(getNames, matchesStrings, longObjects);
            ProcessConsumers(getNames, matchesStrings, longObjects);
            ProcessEventFilters(getNames, matchesStrings, longObjects);

            OutputToConsole();

            if ((!string.IsNullOrEmpty(CommandLine["strings"])) && (Directory.Exists(CommandLine["strings"])))
            {
                WriteStrings(matchesStrings);
            }

            if ((!string.IsNullOrEmpty(CommandLine["output"])) && (Directory.Exists(CommandLine["output"])))
            {
                OutputToFile();
            }
        }

        private static void ProcessBindings(Regex getNames, MatchCollection matchesStrings, int longObjects)
        {
            for (int i = 0; i < longObjects; i++)
            {
                Match m = matchesStrings[i];
                if (m.Value.Contains(@"EventConsumer.Name="""))
                {
                    var nameConsumer = getNames.Matches(m.Value);
                    i++;
                    m = matchesStrings[i];
                    var nameEventFilter = getNames.Matches(m.Value);
                    LBindings.Add(new Details("Binding", nameConsumer[0].Groups[1].Value, nameConsumer[0].Groups[2].Value, nameEventFilter[0].Groups[2].Value, false));
                }
            }
        }

        private static void ProcessConsumers(Regex getNames, MatchCollection matchesStrings, int longObjects)
        {
            for (int i = 0; i < longObjects; i++)
            {
                Match m = matchesStrings[i];
                string type = "";
                string name = "";
                string content = "";
                string other = "";
                if (m.Value == "ActiveScriptEventConsumer")
                {
                    bool valid = false;
                    type = "ActiveScriptEventConsumer";
                    i++;
                    name = matchesStrings[i].Value;
                    i++;
                    if ((matchesStrings[i].Value.ToLower() == "vbscript") || (matchesStrings[i].Value.ToLower() == "jscript"))
                    {
                        valid = true;
                        other = matchesStrings[i].Value;
                        i++;
                        content = matchesStrings[i].Value;
                    }
                    else if ((matchesStrings[i + 1].Value.ToLower() == "vbscript") || (matchesStrings[i + 1].Value.ToLower() == "jscript"))
                    {
                        valid = true;
                        content = matchesStrings[i].Value;
                        i++;
                        other = matchesStrings[i].Value;
                    }
                    if (valid)
                    {
                        var match = LBindings.Find(item => item.Content.Equals(name));
                        if (match != null)
                            LConsumers.Add(new Details(type, name, content, other, false));
                        else
                            LConsumers.Add(new Details(type, name, content, other, true));
                    }
                    else
                    {
                        i -= 2;
                    }
                }
                else if (m.Value == "CommandLineEventConsumer")
                {
                    type = "CommandLineEventConsumer";
                    i++;
                    content = matchesStrings[i].Value;
                    i++;
                    string temp = matchesStrings[i].Value;
                    var match = LBindings.Find(item => item.Content.Equals(temp));
                    if (match != null)
                    {
                        name = temp;
                        LConsumers.Add(new Details(type, name, content, "", false));
                    }
                    else if (temp.ToLower().EndsWith(".exe"))
                    {
                        other = temp;
                        i++;
                        name = matchesStrings[i].Value;
                        match = LBindings.Find(item => item.Content.Equals(name));
                        if (match != null)
                            LConsumers.Add(new Details(type, name, content, "", false));
                        else
                            LConsumers.Add(new Details(type, name, content, "", true));
                    }
                    else
                    {
                        name = temp;
                        LConsumers.Add(new Details(type, name, content, "", true));
                    }
                }
            }
        }

        private static void ProcessEventFilters(Regex getNames, MatchCollection matchesStrings, int longObjects)
        {
            for (int i = 0; i < longObjects; i++)
            {
                Match m = matchesStrings[i];
                string type = "";
                string name = "";
                string content = "";
                string other = "";
                if (m.Value == "__EventFilter")
                {
                    type = "__EventFilter";
                    i++;
                    other = matchesStrings[i].Value;
                    i++;
                    name = matchesStrings[i].Value;
                    i++;
                    content = matchesStrings[i].Value;
                    for (int n = 0; n < 7; n++)
                    {
                        i++;
                        string temp = matchesStrings[i].Value;
                        if ((temp.ToLower() == "wql") || (n == 6))
                        {
                            var match = LBindings.Find(item => item.Other.Equals(name));
                            if (match != null)
                                LEventFilters.Add(new Details(type, name, content, other, false));
                            else
                                LEventFilters.Add(new Details(type, name, content, other, true));
                            break;
                        }
                        else
                        {
                            content += temp;
                        }
                    }
                }
            }
        }

        private static void OutputToConsole()
        {
            Console.WriteLine("Total Bindings: {0}", LBindings.Count);
            foreach (var b in LBindings)
            {
                Console.WriteLine("[{0}]-[{1}]-[{2}]-[{3}]-[{4}]", b.Type, b.Name, b.Content, b.Other, b.Orphan);
            }
            Console.WriteLine("Total Consumers: {0}", LConsumers.Count);
            foreach (var b in LConsumers)
            {
                Console.WriteLine("[{0}]-[{1}]-[{2}]-[{3}]-[{4}]", b.Type, b.Name, b.Content, b.Other, b.Orphan);
            }
            Console.WriteLine("Total EventFilters: {0}", LEventFilters.Count);
            foreach (var b in LEventFilters)
            {
                Console.WriteLine("[{0}]-[{1}]-[{2}]-[{3}]-[{4}]", b.Type, b.Name, b.Content, b.Other, b.Orphan);
            }
        }

        private static void WriteStrings(MatchCollection matchesStrings)
        {
            string outputPath = Path.Combine(CommandLine["strings"], "WMIParserStr-OBJECTS_strings.txt");

            using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                foreach (var line in matchesStrings)
                {
                    writer.WriteLine(line.ToString());
                }
            }
        }

        private static void OutputToFile()
        {
            string outputPath = Path.Combine(CommandLine["output"], "WMIParserStr-output.tsv");

            using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", "Type", "Name", "Content", "Other", "Orphan");
                foreach (var item in LBindings)
                {
                    writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", item.Type.ToString(), item.Name.ToString(), item.Content.ToString(), item.Other.ToString(), item.Orphan.ToString());
                }
                foreach (var item in LConsumers)
                {
                    writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", item.Type.ToString(), item.Name.ToString(), item.Content.ToString(), item.Other.ToString(), item.Orphan.ToString());
                }
                foreach (var item in LEventFilters)
                {
                    writer.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", item.Type.ToString(), item.Name.ToString(), item.Content.ToString(), item.Other.ToString(), item.Orphan.ToString());
                }
            }
        }

        private static void DisplayHelp()
        {
            var help = "Author: Ignacio J. Pérez Jiménez\r\n" +
                       "github.com/ignacioj\r\n\r\n" +
                       "-input: Input file (OBJECTS.DATA)\r\n" +
                       "-output: Output directory for analysis results\r\n" +
                       "-strings: Output directory to save the strings (not Unicode) of OBJECTS.DATA";
            Console.WriteLine(help);
            Environment.Exit(0);
        }
    }

    public class Details
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Other { get; set; }
        public bool Orphan { get; set; }

        public Details(string type, string name, string content, string other, bool orphan)
        {
            Type = type;
            Name = name;
            Content = content;
            Other = other;
            Orphan = orphan;
        }
    }
}
