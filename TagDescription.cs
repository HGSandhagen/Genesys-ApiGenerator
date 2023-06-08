
namespace ApiGenerator {
    internal class TagDescription {
        public TagDescription(string name) {
            Name = name;
        }
        public string Name { get; set; }
        public string? Description { get; set; }
        public Documentation? ExternalDocumentation { get; set; }
    }
}
