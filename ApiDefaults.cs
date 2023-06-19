
namespace ApiGenerator {
    internal class ApiDefaults {
        public ApiInfo? ApiInfo { get; set; }
        public string[]? Consumes { get; set; }
        public string[]? Produces { get; set; }
        public ApiOperationResponse[] Responses { get; set; } = Array.Empty<ApiOperationResponse>();
        public TagDescription[]? Tags { get; set; }
        public string[]? Schemes { get; set; }
        public SecurityScheme[]? SecuritySchemes { get; set; }
        public Documentation? ExternalDocs { get; set; }
    }
}
