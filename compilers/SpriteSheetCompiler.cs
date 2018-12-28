using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using Asteroids.Common;

using static System.Console;

namespace Asteroids.Content {

    [AssetCompiler("spritesheet", "ssdat")]
    public class SpriteSheetCompiler : AssetCompiler {

        public override void Compile(string inFile, string outFile, Compiler compiler, out IEnumerable<string> errorMsgs) {
            var errors = new List<string>();
            errorMsgs = errors;

            var res = ParseSpriteSheet(inFile, errors);
            if (res != null && errors.Count < 1) {
                using (var file = File.OpenWrite(outFile)) {
                    try {
                        if (!res.ToStream(file)) {
                            errors.Add(String.Format("Unknown error while writing {0} to file}", res.GetType().Name));
                        }
                    } catch (Exception ex) {
                        errors.Add(String.Format("While writing {0} to file: <{1}:{2}> {3}", res.GetType().Name, ex.GetType().Name,
                            ex.Source, ex.Message));
                    }
                }
            }
        }


        public static SpriteSheet ParseSpriteSheet(string uri, IList<string> errors) {
            try {
                var res = new SpriteSheet();
                using (var file = File.OpenText(uri)) {
                    var parsedObj = JObject.Parse(file.ReadToEnd());
                    IList<string> validationErrors;
                    parsedObj.IsValid(Schemas.GetSpriteSheetSchema(), out validationErrors);
                    if (validationErrors.Count > 0) {
                        foreach (var error in validationErrors) {
                            errors.Add(String.Format("While validating sprite sheet: {0}", error));
                        }
                        return null;
                    }
                    JArray version = (JArray)parsedObj["version"];
                    // TODO: Check sprite sheet file version
                    res.TextureName = (string)parsedObj["texture_name"];
                    res.Key = (string)parsedObj["key"];
                    res.FrameWidth = (int)parsedObj["frame_width"];
                    res.FrameHeight = (int)parsedObj["frame_height"];
                    if (parsedObj.ContainsKey("offset")) {
                        var offsetArray = (JArray)parsedObj["offset"];
                        var offset = new Point((int)offsetArray[0], (int)offsetArray[1]);
                        res.Offset = offset;
                    }
                    return res;
                }
            } catch (Exception ex) {
                WriteLine("While parsing sprite sheet: <{0}:{1}> {2}", ex.GetType().Name, ex.Source, ex.Message);
            }
            return null;
        }

    }
}