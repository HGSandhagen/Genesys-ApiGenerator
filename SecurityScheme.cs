
namespace ApiGenerator {
    internal class SecurityScheme {
        public SecurityScheme(string type, string? displayName) {
            SchemeType = type;
            DisplayName = displayName;
        }
        /// <summary>
        /// The type of the security scheme.Valid values are "basic", "apiKey" or "oauth2".
        /// </summary>
        public string SchemeType { get; set; }
        /// <summary>
        /// A short description for security scheme.
        /// </summary>
        public string? Description { get; set; }
        public string? DisplayName { get; set; }
    }
    internal class BasicSecurityScheme : SecurityScheme { 
        public BasicSecurityScheme(string displayName) : base("basic", displayName) { }
    }
    internal class ApiKeySecurityScheme : SecurityScheme {
        public ApiKeySecurityScheme(string name, string @in, string displayName) : base("apiKey", displayName) {
            Name = name;
            In = @in;
        }

        /// <summary>
        /// The name of the header or query parameter to be used.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The location of the API key.Valid values are "query" or "header".
        /// </summary>
        public string In { get; set; }
    }
    internal class OAuth2SecurityScheme : SecurityScheme {
        public OAuth2SecurityScheme(string flow, string authorizationUrl, string? tokenUrl, IEnumerable<Scope> scopes, string displayName) : base("oauth2", displayName) {
            Flow = flow;
            AuthorizationUrl = authorizationUrl;
            TokenUrl = tokenUrl;
            Scopes = scopes;
        }

        /// <summary>
        /// The flow used by the OAuth2 security scheme. Valid values are "implicit", "password", "application" or "accessCode".
        /// </summary>
        public string Flow { get; set; }
        /// <summary>
        /// The authorization URL to be used for this flow.This SHOULD be in the form of a URL.
        /// </summary>
        public string AuthorizationUrl { get; set; }
        /// <summary>
        /// The token URL to be used for this flow.This SHOULD be in the form of a URL.
        /// </summary>
        public string? TokenUrl { get; set; }
        /// <summary>
        /// The available scopes for the OAuth2 security scheme.
        /// </summary>
        public IEnumerable<Scope> Scopes { get; set; }
    }
    public class Scope {
        public Scope(string name, string description) {
            Name = name;
            Description = description;
        }

        public string Name { get; set; }
        public string Description { get; set; }
    }
}
