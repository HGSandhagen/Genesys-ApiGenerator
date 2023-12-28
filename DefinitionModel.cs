using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class DefinitionModel {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DefinitionModel() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public DefinitionModel(SwaggerDefinition item) {
            Name = item.Name;
            Description = item.Description;
            IsNotification = false;
            EnumDefinitions = [];
            Properties = [];
            foreach (var p in item.Properties) {
                var jsonName = p.Name;
                var propName = ApiGenerator.CreateName(jsonName);
                TypeInfo typeInfo = p.GetTypeInfo(propName) ?? throw new Exception($"Could not get TypeInfo for {Name}");
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
            EnumDefinitions = [];
            Properties = [];
            if (item.Properties.Length != 0 == false) {
                Alias = item.Type;
            }
            foreach (var p in item.Properties) {
                var jsonName = p.Name;
                var propName = ApiGenerator.CreateName(jsonName);
                TypeInfo typeInfo = p.GetTypeInfo(propName) ?? throw new Exception($"Could not get TypeInfo for {Name}");
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
        public List<DefinitionPropertyModel> Properties { get; set; } = [];
        public List<EnumModel> EnumDefinitions { get; set; } = [];

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

        //static string CreateName(string jsonName, bool lower = false) {
        //    string name = Regex.Replace(jsonName, "(-[A-Za-z])", (p) => p.Value[1..].ToUpper());
        //    name = Regex.Replace(name, "(-[0-9])", (p) => "_" + p.Value[1..]);
        //    name = Regex.Replace(name, "(_[A-Za-z])", (p) => p.Value[1..].ToUpper());
        //    name = Regex.Replace(name, "(/[A-Za-z])", (p) => p.Value[1..].ToUpper());
        //    name = Regex.Replace(name, "(:[A-Za-z0-9])", (p) => "_" + p.Value[1..].ToUpper());
        //    name = Regex.Replace(name, "(\\$[A-Za-z0-9])", (p) => p.Value[1..].ToUpper());
        //    name = Regex.Replace(name, "(\\.[A-Za-z0-9])", (p) => p.Value[1..].ToUpper());
        //    if (Char.IsNumber(name[0])) {
        //        name = "_" + name;
        //    }
        //    if (name == "override") {
        //        name = "@override";
        //    }
        //    if (!lower) {
        //        name = string.Concat(name[0].ToString().ToUpper(), name.AsSpan(1)); // FirstCharToUpper();
        //    }
        //    if (name == "GetType") {
        //        name += "_";
        //    }
        //    return name;
        //}
    }
}
