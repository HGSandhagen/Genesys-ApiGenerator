using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class DefinitionModel {
        public DefinitionModel(SwaggerDefinition item) {
            Name = item.Name;
            Description = item.Description;
            IsNotification = false;
            EnumDefinitions = new List<EnumModel>();
            Properties = new List<DefinitionPropertyModel>();
            foreach (var p in item.Properties) {
                var jsonName = p.Name;
                var propName = CreateName(jsonName);
                TypeInfo typeInfo = p.GetTypeInfo(propName);
                if (typeInfo == null) {
                    throw new Exception($"Could not get TypeInfo for {Name}");
                }
                if (typeInfo.EnumModel != null) {
                    EnumDefinitions.Add(typeInfo.EnumModel);
                }

                if (Name == propName) {
                    propName += "_";
                }
                var dp = new DefinitionPropertyModel(propName, jsonName, typeInfo.TypeName, typeInfo.IsCollection, typeInfo.IsDictionary) {
                    Summary = p.Description,
                };
                Properties.Add(dp);
            }
            if (item.Required != null) {
                foreach (var r in item.Required) {
                    var d = Properties.FirstOrDefault(p => p.JsonName == r);
                    if (d != null) {
                        d.IsRequired = true;
                    }
                }
            }
        }
        public DefinitionModel(NotificationSwaggerDefinition item) {
            Id = item.Id;
            Name = item.Name;
            Description = item.Description;
            IsNotification = true;
            TopicParameters = item.TopicParameters;
            EnumDefinitions = new List<EnumModel>();
            Properties = new List<DefinitionPropertyModel>();
            if (item.Properties.Any() == false) {
                Alias = item.Type;
            }
            foreach (var p in item.Properties) {
                var jsonName = p.Name;
                var propName = CreateName(jsonName);
                TypeInfo typeInfo = p.GetTypeInfo(propName);
                if (typeInfo == null) {
                    throw new Exception($"Could not get TypeInfo for {Name}");
                }
                if (typeInfo.EnumModel != null) {
                    EnumDefinitions.Add(typeInfo.EnumModel);
                }

                if (Name == propName) {
                    propName += "_";
                }
                var dp = new DefinitionPropertyModel(propName, jsonName, typeInfo.TypeName, typeInfo.IsCollection, typeInfo.IsDictionary) {
                    Summary = p.Description,
                };
                Properties.Add(dp);
            }
            if (item.Required != null) {
                foreach (var r in item.Required) {
                    var d = Properties.FirstOrDefault(p => p.JsonName == r);
                    if (d != null) {
                        d.IsRequired = true;
                    }
                }
            }
        }
        public string? Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsNotification { get; set; }
        public string? Alias { get; set; }
        public string[]? TopicParameters { get; set; }
        public List<DefinitionPropertyModel> Properties { get; set; } = new List<DefinitionPropertyModel>();
        public List<EnumModel> EnumDefinitions { get; set; } = new List<EnumModel>();

        //string GetArrayType(string propName, SwaggerArray items) {
        //    if (items.Ref != null) {
        //        return items.Ref.Replace("#/definitions/", "");
        //    }
        //    else if (items.Type == "integer") {
        //        if (items.Format == "int64") {
        //            return "long";
        //        }
        //        else {
        //            return "int";
        //        }
        //    }
        //    else if (items.Type == "number") {
        //        if (items.Format == "float") {
        //            return "float";
        //        }
        //        else {
        //            return "double";
        //        }
        //    }
        //    else if (items.Type == "string") {
        //        if (items.EnumValues != null) {
        //            var typeName = propName + "Constant";
        //            EnumDefinitions.Add(new EnumModel(typeName, items.EnumValues));
        //            return typeName;
        //        }
        //        return "string";
        //    }
        //    else if (items.Type == "object") {
        //        return "object";
        //    }
        //    return "";
        //}

        string CreateName(string jsonName, bool lower = false) {
            string name = Regex.Replace(jsonName, "(-[A-Za-z])", (p) => p.Value[1..].ToUpper());
            name = Regex.Replace(name, "(-[0-9])", (p) => "_" + p.Value[1..]);
            name = Regex.Replace(name, "(_[A-Za-z])", (p) => p.Value[1..].ToUpper());
            name = Regex.Replace(name, "(/[A-Za-z])", (p) => p.Value[1..].ToUpper());
            name = Regex.Replace(name, "(:[A-Za-z0-9])", (p) => "_" + p.Value[1..].ToUpper());
            name = Regex.Replace(name, "(\\$[A-Za-z0-9])", (p) => p.Value[1..].ToUpper());
            name = Regex.Replace(name, "(\\.[A-Za-z0-9])", (p) => p.Value[1..].ToUpper());
            if (Char.IsNumber(name[0])) {
                name = "_" + name;
            }
            if (name == "override") {
                name = "@override";
            }
            if (!lower) {
                name = string.Concat(name[0].ToString().ToUpper(), name.AsSpan(1)); // FirstCharToUpper();
            }
            if (name == "GetType") {
                name += "_";
            }
            return name;
        }
    }
}
