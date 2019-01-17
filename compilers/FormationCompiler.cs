using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json;

using Microsoft.Xna.Framework;

using Asteroids.Common;

using static System.Console;

namespace Asteroids.Content {

    [AssetCompiler("formation", "fdat")]
    public class FormationCompiler : AssetCompiler {

        public override void Compile(string inFile, string outFile, out IEnumerable<string> errorMsgs) {
            var errors = new List<string>();
            errorMsgs = errors;
            var res = ParseJsonFormation(inFile, errors);
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


        #region Json Parsing

        public static JsonFormation ParseJsonFormation(string fileUri, IList<string> outErrors) {
            try {
                JsonFormation res = new JsonFormation();
                using (var fileReader = File.OpenText(fileUri)) {
                    JObject obj = JObject.Parse(fileReader.ReadToEnd());
                    IList<string> errors = new List<string>();
                    obj.IsValid(Schemas.GetFormationSchema(), out errors);
                    if (errors.Any()) {
                        foreach (var error in errors) {
                            outErrors.Add(String.Format("Parse Error: {0}", error));
                        }
                        return null;
                    }
                    JArray version = (JArray)obj["version"];
                    (int major, int minor) = ((int)version[0], (int)version[1]);
                    // TODO: Check formation file version
                    res.Key = (string)obj["key"];
                    foreach (var item in (JArray)obj["schedule"]) {
                        int startTime = (int)item["time"];
                        var posObj = (JArray)item["position"];
                        Vector2 pos = new Vector2((float)posObj[0], (float)posObj[1]);
                        res.schedule.Add(new Tuple<int, Vector2>(startTime, pos));
                    }
                    res.schedule = res.schedule.OrderBy(item => item.Item1).ToList();
                }
                return res;
            } catch (Exception ex) {
                outErrors.Add(String.Format("Exception: {0}", ex.Message));
                return null;
            }
        }

        #endregion
    }

}