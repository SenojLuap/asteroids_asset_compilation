using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

using static System.Console;

namespace Asteroids.Content {

    public class Compiler {

        public Dictionary<string, AssetCompiler> compilers;

        public HashSet<string> ignoredFileTypes;

        public Compiler() {
            ignoredFileTypes = new HashSet<string>();
        }        

        public void Run(string sourceDir, string outputDir) {
            InitCompilers();
            WriteLine("Compiling:");
            Execute(Path.GetFullPath(sourceDir), Path.GetFullPath(outputDir));
        }


        public void Execute(string sourceDir, string outputDir) {
            WriteLine(" - Read from: {0}", sourceDir);
            WriteLine(" - Write to: {0}", outputDir);
            foreach (var fileName in Directory.EnumerateFiles(sourceDir)) {
                ProcessFile(fileName, outputDir);
            }
            foreach (var dirName in Directory.EnumerateDirectories(sourceDir)) {
                var subDir = dirName.Split(Path.DirectorySeparatorChar).Last();
                var outputSubDir = Path.Combine(outputDir, subDir);
                if (!Directory.Exists(outputSubDir)) {
                    Directory.CreateDirectory(outputSubDir);
                    WriteLine(" - Created Dir: {0}", outputSubDir);
                }
                Execute(dirName, outputSubDir);
            }
        }


        public void ProcessFile(string fileName, string outputDir) {
            var fileKey = Path.GetExtension(fileName).Substring(1).ToLower();
            if (compilers.ContainsKey(fileKey)) {
                var compiler = compilers[fileKey];
                var outputFileType = OutputFileType(compiler);
                var outfile = Path.Combine(outputDir, String.Format("{0}.{1}", Path.GetFileNameWithoutExtension(fileName), outputFileType));
                IEnumerable<string> errors;
                compiler.Compile(fileName, outfile, this, out errors);
                if (errors.Any()) {
                    WriteLine(" - Errors parsing {0}", fileName);
                    foreach (var error in errors)
                        WriteLine(" - - " + error);
                } else {
                    WriteLine(" - Wrote: {0}", outfile);
                }
            } else if (!ignoredFileTypes.Contains(fileKey)) {
                WriteLine(" - Skipping file of type .{0} as no compilers available", fileKey);
                ignoredFileTypes.Add(fileKey);
            }
        }


        public void InitCompilers() {
            WriteLine("Finding asset compilers:");
            var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetExportedTypes()
                where type.IsSubclassOf(typeof(AssetCompiler)) && !type.IsAbstract
                select type;
            compilers = new Dictionary<string, AssetCompiler>();
            foreach (var type in types) {
                var compilerAttribute = (AssetCompilerAttribute)type.GetCustomAttribute(typeof(AssetCompilerAttribute));
                if (compilerAttribute == null) {
                    WriteLine(" - Error: " + type.Name + " must specify a " + typeof(AssetCompilerAttribute).Name);
                    continue;
                }
                var compilerKey = compilerAttribute.InputFileTypes.ToLower();
                var compiler = (AssetCompiler)Activator.CreateInstance(type);
                var fileTypes = compilerKey.Split(';');
                foreach (var fileType in fileTypes) {
                    if (compilers.ContainsKey(fileType)) {
                        var otherType = compilers[fileType].GetType();
                        WriteLine(" - Error: {0} attempting to handle '{1}' files, but already handled by {2}", type.Name, fileType, otherType.Name);
                        continue;
                    }
                
                    compilers[fileType] = compiler;
                    WriteLine(" - Added {0}: {1} -> {2}", type.Name, fileType, compilerAttribute.OutputFileType);
                }
            }
        }


        public static string OutputFileType(AssetCompiler compiler) {
            var attribute = (AssetCompilerAttribute)compiler.GetType().GetCustomAttribute(typeof(AssetCompilerAttribute));
            return attribute.OutputFileType;
        }
    }
}