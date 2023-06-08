using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGenerator {
    internal class TypeInfo {
        public TypeInfo(string typeName, bool isCollection, bool isDictionary = false, EnumModel? enumModel = null) { 
            TypeName = typeName;
            IsCollection = isCollection;
            IsDictionary = isDictionary;
            EnumModel = enumModel;
        }
        public string TypeName { get; set; }
        public bool IsCollection { get; set; }
        public bool IsDictionary { get; set; }
        public EnumModel? EnumModel { get; set; }
    }
}
