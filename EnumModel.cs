using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGenerator {
    public class EnumModel {
        public EnumModel(string name, IEnumerable<string> enumValues) {
            Name = name;
            EnumValues = enumValues;
        }

        public string Name { get; set; }
        public IEnumerable<string> EnumValues { get; set; }
    }
}
