using System.Collections.Generic;

namespace Asteroids.Content {
    public abstract class AssetCompiler {
        public abstract void Compile(string inFile, string outFile, Compiler compiler, out IEnumerable<string> errors);
    }
}