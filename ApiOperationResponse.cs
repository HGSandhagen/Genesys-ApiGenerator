
namespace ApiGenerator {
    public class ApiOperationResponse {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public string ResponseCode { get; set; }
        public string? Description { get; set; }
        public string TypeName { get; set; }
        public bool IsCollection { get; set; }
        public bool IsDictionary { get; set; }
        public EnumModel? EnumModel { get; set; }
        public Dictionary<string, string>? ErrorCodes { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}