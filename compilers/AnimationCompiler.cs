using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using Asteroids.Common;

using static System.Console;

namespace Asteroids.Content {

    [AssetCompiler("animation", "adat")]
    public class AnimationCompiler : AssetCompiler {

        public override void Compile(string inFile, string outFile, out IEnumerable<string> errors) {
            var outErrors = new List<string>();
            errors = outErrors;

            var res = ParseAnimation(inFile, outErrors);
            if (outErrors.Count == 0) {
                using (var file = File.OpenWrite(outFile)) {
                    try {
                        if (!res.ToStream(file)) {
                            outErrors.Add(String.Format("Unknown error while writing {0} to file}", res.GetType().Name));
                        }
                    } catch (Exception ex) {
                        outErrors.Add(String.Format("While writing {0} to file: <{1}:{2}> {3}", res.GetType().Name, ex.GetType().Name,
                            ex.Source, ex.Message));
                    }
                }
            }
        }


        /// <summary>
        /// Parse an Animation from file
        /// </summary>
        /// <param name="fileUri">The file to parse from</param>
        /// <param name="errors">Any resulting errors</param>
        /// <returns>The parsed Animation</returns>
        public static JsonSpriteAnimation ParseAnimation(string fileUri, IList<string> errors) {
            try {
                var res = new JsonSpriteAnimation();
                using (var file = File.OpenText(fileUri)) {
                    var parsedObject = JObject.Parse(file.ReadToEnd());
                    IList<string> parseErrors;
                    parsedObject.IsValid(Schemas.GetAnimationSchema(), out parseErrors);
                    if (parseErrors.Count > 0) {
                        foreach (var error in parseErrors) {
                            errors.Add(String.Format("Parsing animation: {0}", error));
                        }
                        return null;
                    }
                    JArray version = (JArray)parsedObject["version"];
                    (int major, int minor) = ((int)version[0], (int)version[1]);
                    // TODO: Check animation file version
                    res.Key = (string)parsedObject["key"];
                    res.spriteSheetKey = (string)parsedObject["spritesheet"];
                    bool bounce = false;
                    if (parsedObject.ContainsKey("bounce"))
                        bounce = (bool)parsedObject["bounce"];
                    if (parsedObject.ContainsKey("frame_length")) {
                        int frameStart = (int)parsedObject["frame_start"];
                        int frameStop = (int)parsedObject["frame_stop"];
                        int count = Math.Abs(frameStop - frameStart) + 1;
                        double frameLength = (double)parsedObject["frame_length"];
                        IEnumerable<int> range = Enumerable.Range(Math.Min(frameStart, frameStop), count);
                        if (frameStop < frameStart)
                            range = range.Reverse();
                        var frames = new List<(int, double)>();
                        foreach (int frame in range) {
                            frames.Add((frame, frameLength));
                        }
                        res.Frames = JsonSpriteAnimation.CreateFrames(bounce, frames.ToArray());
                    } else {
                        var frames = new List<(int, double)>();
                        var frameArray = (JArray)parsedObject["frames"];
                        foreach (var frameNode in frameArray) {
                            var frameObj = frameNode as JObject;
                            int frame = (int)frameObj["frame"];
                            double length = (double)frameObj["length"];
                            frames.Add((frame, length));
                        }
                        res.Frames = JsonSpriteAnimation.CreateFrames(bounce, frames.ToArray());
                    }
                }
                return res;
            } catch (Exception ex) {
                errors.Add(String.Format("Exception parsing animation: <{0}:{2}> {1}", ex.GetType().Name, ex.Message, ex.Source));
                return null;
            }
        }

    }



}