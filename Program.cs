using System;
using CommandLine;

using paujo.Stream.Compression;
using System.IO;
using System.Text;

namespace Asteroids.Content
 {
    public class Program {
        static int Main(string[] args) {

            var results = Parser.Default.ParseArguments<ProgramArguments>(args) as Parsed<ProgramArguments>;
            if (results == null) {
                return 1;
            }
            try {
                var parsedArgs = results.Value;
                if (String.IsNullOrEmpty(parsedArgs.OutputDirectory))
                    parsedArgs.OutputDirectory = parsedArgs.InputDirectory;
                (new Compiler()).Run(parsedArgs.InputDirectory, parsedArgs.OutputDirectory);
            } catch (Exception ex) {
                System.Console.WriteLine("Exception: {0}", ex.Message);
                return 1;
            }
            return 0;
        }
    }

    public class ProgramArguments {

        [Option('i', "input", Required = true, HelpText = "Directory for which to compile assets (recursive)")]
        public string InputDirectory { get; set; }

        [Option('o', "output", Required = false, HelpText = "Directory to which results should be added (respects original folder hierarchy")]
        public string OutputDirectory { get; set; }
    }
}
