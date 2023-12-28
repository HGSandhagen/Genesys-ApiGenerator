using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiGenerator {
    public class EnumModel {
        [JsonConstructor]
        public EnumModel(string name, Dictionary<string, string> enumValues) {
            Name = name;
            EnumValues = enumValues;
        }
        public EnumModel(string name, IEnumerable<string> enumValues) {
            Name = name;
            EnumValues = enumValues.ToDictionary(p => p, p => ApiGenerator.CreateName(p));
        }

        public string Name { get; set; }
        //public IEnumerable<string> EnumValues { get; set; }
        public Dictionary<string, string> EnumValues { get; private set; }
    }
}
