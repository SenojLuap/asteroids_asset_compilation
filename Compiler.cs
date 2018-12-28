using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

using paujo.Debug;

namespace Asteroids.Content {

    public static class Compiler {

        public static Dictionary<string, AssetCompiler> compilers;

        public static HashSet<string> ignoredFileTypes;

        public static Logger logger;


        public static void Run(string sourceDir, string outputDir) {
            ignoredFileTypes = new HashSet<string>();
            logger = new Logger();
            logger.AddConsoleTarget();

            InitCompilers();
            logger.Info("Compiling:");
            Execute(Path.GetFullPath(sourceDir), Path.GetFullPath(outputDir));
        }


        public static void Execute(string sourceDir, string outputDir) {
            logger.Info(String.Format("Read from: {0}", sourceDir));
            logger.Info(String.Format("Write to: {0}", outputDir));
            foreach (var fileName in Directory.EnumerateFiles(sourceDir)) {
                ProcessFile(fileName, outputDir);
            }
            foreach (var dirName in Directory.EnumerateDirectories(sourceDir)) {
                var subDir = dirName.Split(Path.DirectorySeparatorChar).Last();
                var outputSubDir = Path.Combine(outputDir, subDir);
                if (!Directory.Exists(outputSubDir)) {
                    Directory.CreateDirectory(outputSubDir);
                    logger.Info(String.Format(" - Created Dir: {0}", outputSubDir));
                }
                Execute(dirName, outputSubDir);
            }
        }


        public static void ProcessFile(string fileName, string outputDir) {
            var fileKey = Path.GetExtension(fileName).Substring(1).ToLower();
            if (compilers.ContainsKey(fileKey)) {
                var compiler = compilers[fileKey];
                var outputFileType = OutputFileType(compiler);
                var outfile = Path.Combine(outputDir, String.Format("{0}.{1}", Path.GetFileNameWithoutExtension(fileName), outputFileType));
                IEnumerable<string> errors;
                compiler.Compile(fileName, outfile, out errors);
                if (errors.Any()) {
                    logger.Error(String.Format("Errors parsing {0}", fileName));
                    foreach (var error in errors)
                        logger.Error(" - " + error);
                } else {
                    logger.Info(String.Format("Wrote: {0}", outfile));
                }
            } else if (!ignoredFileTypes.Contains(fileKey)) {
                logger.Warning(String.Format(" - Skipping file of type .{0} as no compilers available", fileKey));
                ignoredFileTypes.Add(fileKey);
            }
        }


        public static void InitCompilers() {
            logger.Info("Finding asset compilers:");
            var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                from type in assembly.GetExportedTypes()
                where type.IsSubclassOf(typeof(AssetCompiler)) && !type.IsAbstract
                select type;
            compilers = new Dictionary<string, AssetCompiler>();
            foreach (var type in types) {
                var compilerAttribute = (AssetCompilerAttribute)type.GetCustomAttribute(typeof(AssetCompilerAttribute));
                if (compilerAttribute == null) {
                    logger.Error(" - Error: " + type.Name + " must specify a " + typeof(AssetCompilerAttribute).Name);
                    continue;
                }
                var compilerKey = compilerAttribute.InputFileTypes.ToLower();
                var compiler = (AssetCompiler)Activator.CreateInstance(type);
                var fileTypes = compilerKey.Split(';');
                foreach (var fileType in fileTypes) {
                    if (compilers.ContainsKey(fileType)) {
                        var otherType = compilers[fileType].GetType();
                        logger.Error(String.Format("{0} attempting to handle '{1}' files, but already handled by {2}", type.Name, fileType, otherType.Name));
                        continue;
                    }
                
                    compilers[fileType] = compiler;
                    logger.Info(String.Format("Added {0}: {1} -> {2}", type.Name, fileType, compilerAttribute.OutputFileType));
                }
            }
        }


        public static string OutputFileType(AssetCompiler compiler) {
            var attribute = (AssetCompilerAttribute)compiler.GetType().GetCustomAttribute(typeof(AssetCompilerAttribute));
            return attribute.OutputFileType;
        }
    }
}