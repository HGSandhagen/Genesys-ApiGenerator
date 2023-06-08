
namespace ApiGenerator {
    internal class Documentation {
        public Documentation(string url) {
            Url = url;
        }
        public string Url { get; set; }
        public string? Description { get; set; }
    }
}
