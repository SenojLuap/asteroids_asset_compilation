using System;
using System.Reflection;

namespace Asteroids.Content {

    [AttributeUsage(AttributeTargets.Class)]
    public class AssetCompilerAttribute : Attribute {

        public string InputFileTypes { get; set; }
        public string OutputFileType { get; set; }

        public AssetCompilerAttribute(string inputFileTypes, string outputFileType) {
            InputFileTypes = inputFileTypes;
            OutputFileType = outputFileType;
        }
    }

}