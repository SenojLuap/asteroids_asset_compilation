using System;
using System.IO;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

using static System.Console;

namespace Asteroids.Content {
    public class Schemas {

        private static JSchema VersionSchema;
        private static JSchema FormationSchema;
        private static JSchema SpriteSheetSchema;
        private static JSchema AnimationSchema;

        private static string SchemaDirectory = Directory.GetCurrentDirectory() + "/json_schemas/";


        public static JSchema GetVersionSchema() {
            if (VersionSchema == null) {
                VersionSchema = LoadSchema("version");
            }
            return VersionSchema;
        }


        public static JSchema GetFormationSchema() {
            if (FormationSchema == null) {
                FormationSchema = LoadSchema("formation", new JSchema[] {GetVersionSchema()});
            }
            return FormationSchema;
        }


        public static JSchema GetSpriteSheetSchema() {
            if (SpriteSheetSchema == null) {
                SpriteSheetSchema = LoadSchema("spritesheet", new JSchema[] {GetVersionSchema()});
            }
            return SpriteSheetSchema;
        }


        public static JSchema GetAnimationSchema() {
            if (AnimationSchema == null) {
                AnimationSchema = LoadSchema("animation", new JSchema[] {GetVersionSchema()});
            }
            return AnimationSchema;
        }


        private static JSchema LoadSchema(string name, IEnumerable<JSchema> toResolve=null) {
            var resolver = new JSchemaPreloadedResolver();
            if (toResolve != null) {
                foreach (var schema in toResolve) {
                    resolver.Add(schema.Id, schema.ToString());
                }
            }
            string uri = SchemaDirectory + name + ".schema.json";
            try {
                using (var fileReader = File.OpenText(uri)) {
                    using (var jsonReader = new JsonTextReader(fileReader)) {
                        return JSchema.Load(jsonReader, resolver);
                    }
                }
            } catch (IOException ex) {
                // TODO: Handle this with application error handling
                System.Console.WriteLine(name.ToUpper() + " SCHEMA [" + ex.GetType().Name + "]: " + ex.Message);
            } catch (Exception ex) {
                // TODO: Handle this with application error handling
                System.Console.WriteLine(name.ToUpper() + " SCHEMA [" + ex.GetType().Name + "]: " + ex.Message);
            }
            return null;
        }
    }
}