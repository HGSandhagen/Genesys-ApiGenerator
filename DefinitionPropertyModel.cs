using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class DefinitionPropertyModel {
        public DefinitionPropertyModel(string name, string jsonName, string typeName, bool isCollection = false, bool isDictionary = false) { 
            Name = name;
            JsonName = jsonName;
            TypeName = typeName;
            IsCollection = isCollection;
            IsDictionary = isDictionary;
        }
        public string Name { get; set; }
        public string JsonName { get; set; }
        public string TypeName { get; set; }
        public bool IsCollection { get; set; }
        public bool IsDictionary { get; set; }
        public bool IsRequired { get; set; }
        public string? Summary { get; set; }
        public string? Description { get; set; }
    }
}
