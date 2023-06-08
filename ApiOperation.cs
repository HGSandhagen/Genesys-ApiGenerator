namespace ApiGenerator {
    public class ApiOperation {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string Id { get; set; }
        public string Method { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Path { get; set; }
        public IEnumerable<ApiOperationParameter>? Parameters { get; set; }
        public IEnumerable<ApiOperationResponse>? Responses { get; set; }
        public IEnumerable<string>? Produces { get; set; }
        public IEnumerable<string>? Consumes { get; set; }
        public IEnumerable<string>? Tags { get; set; }
        public bool IsDeprecated { get; set; }
        public IEnumerable<ApiPermisson>? Permissions { get; set; }
        public string? PurecloudMethodName { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
    public class ApiPermisson {
        public string? PermissionType { get; set; }
        public IEnumerable<string>? Permissions { get; set; }
    }
}
