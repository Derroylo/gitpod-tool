using System.Collections.Generic;

namespace Gitpod.Tool.Helper.Internal.Config.Sections.Types
{
    class PersistFileType
    {
        public string Name {get; set;}

        public string File {get; set;}

        public bool? Overwrite {get; set;}

        public string GpVarName {get; set;}

        public string Content {get; set;}

        public static PersistFileType FromDictionary(string name, Dictionary<string, string> data) {
            var newType = new PersistFileType() {
                Name = name
            };

            if (data.TryGetValue("file", out string file)) {
                newType.File = file;
            }

            if (data.TryGetValue("overwrite", out string overwrite)) {
                newType.Overwrite = overwrite == "true";
            }

            if (data.TryGetValue("var", out string var)) {
                newType.GpVarName = var;
            }

            if (data.TryGetValue("content", out string content)) {
                newType.Content = content;
            }

            return newType;
        }

        public Dictionary<string, string> ToDictionary(bool excludeName = true)
        {
            var typeDictionary = new Dictionary<string, string>();

            if (!excludeName && Name != null) {
                typeDictionary.Add("name", Name);
            }

            if (File != null) {
                typeDictionary.Add("file", File);
            }

            if (Overwrite != null) {
                typeDictionary.Add("overwrite", Overwrite.ToString());
            }

            if (GpVarName != null) {
                typeDictionary.Add("var", GpVarName);
            }

            if (Content != null) {
                typeDictionary.Add("content", Content);
            }

            return typeDictionary;
        }

        public override string ToString()
        {
            var typeString = "";

            if (Name != null) {
                typeString += "Name: " + Name;
            } else {
                typeString += "Name: undefined";
            }

            if (File != null) {
                typeString += ", File: " + File;
            } else {
                typeString += ", File: undefined";
            }

            if (Overwrite != null) {
                typeString += ", Overwrite: " + Overwrite.ToString();
            }  else {
                typeString += ", Overwrite: undefined";
            }

            if (GpVarName != null) {
                typeString += ", GpVarName: " + GpVarName;
            } else {
                typeString += ", GpVarName: undefined";
            }

            if (Content != null) {
                typeString += ", Content: " + Content;
            } else {
                typeString += ", Content: undefined";
            }

            return typeString;
        }
    }
}