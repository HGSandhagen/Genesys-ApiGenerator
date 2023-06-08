namespace ApiGenerator {
    public class ApiOperationParameter {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private string _name;
        private string _pName;
        public enum ParameterKind {
            Path, Query, Body, Header
        }

        public string Name {
            get => _name; set {
                _name = value;
                _pName = FormatParameterName(_name);
            }
        }
        public string PName { get => _pName; }
        public string TypeName { get; set; }
        public bool IsCollection { get; set; }
        public bool IsRequired { get; set; }
        public ParameterKind Position { get; set; }
        public string? Description { get; set; }
        public object Default { get; set; }
        public IEnumerable<string> EnumValues { get; set; }
        public bool IsMultiCollection { get; set; }
        private string FormatParameterName(string name) {
            string pName = name.Replace("-", "_");
            if (name.Contains('.')) {
                string[] s = name.Split('.');
                pName = s[0] + s[1].Substring(0, 1).ToUpper() + s[1].Substring(1);
            }
            if (pName == "override") {
                pName = pName + "_";
            }
            return pName;
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    }
}
