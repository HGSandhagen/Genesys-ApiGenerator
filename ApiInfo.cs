
using System.Text.Json.Nodes;

namespace ApiGenerator {
    internal class ApiInfo {
        public ApiInfo(JsonObject o) {

            string? title = null;
            string? version = null;

            foreach (var item in o.AsObject()) {

                switch (item.Key) {
                    case "title":
                        title = item.Value?.ToString();
                        break;
                    case "version":
                        version = item.Value?.ToString();
                        break;
                    case "description":
                        Description = item.Value?.ToString();
                        break;
                    case "termsOfService":
                        TermsOfService = item.Value?.ToString();
                        break;
                    case "contact":
                        if (item.Value is JsonObject) {
                            Contact = new ApiContact(item.Value.AsObject());
                        }
                        else {
                            throw new Exception("contact must be a JsonObject");
                        }
                        break;
                    case "license":
                        if (item.Value is JsonObject) {
                            License = new InfoLicense(item.Value.AsObject());
                        }
                        else {
                            throw new Exception("license must be a JsonObject");
                        }
                        break;
                    default:
                        throw new Exception("Unknown parameter \"" + item.Key + "\" in ApiInfo");

                }
            }
            if (string.IsNullOrEmpty(title)) {
                throw new Exception("title of info must not be null or empty");
            }
            if (string.IsNullOrEmpty(version)) {
                throw new Exception("version of info must not be null or empty");
            }

        
            Title = title;
            Version = version;
        } 

        /// <summary>
        /// The title of the application.
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Provides the version of the application API (not to be confused with the specification version).
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// A short description of the application.GFM syntax can be used for rich text representation.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// The Terms of Service for the API.
        /// </summary>
        public string? TermsOfService { get; set; }
        /// <summary>
        /// The contact information for the exposed API.
        /// </summary>
        public ApiContact? Contact { get; set; }
        /// <summary>
        /// The license information for the exposed API.
        /// </summary>
        public InfoLicense? License { get; set; }
    }
    public class ApiContact {
        public ApiContact(JsonObject o) {
            foreach (var item in o.AsObject()) {

                switch (item.Key) {
                    case "name":
                        Name = item.Value?.ToString();
                        break;
                    case "url":
                        Url = item.Value?.ToString();
                        break;
                    case "email":
                        Email = item.Value?.ToString();
                        break;
                    default:
                        throw new Exception("Unknown parameter \"" + item.Key + "\" in ApiContact");
                }
            }

        }
        /// <summary>
        /// The identifying name of the contact person/organization.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// The URL pointing to the contact information.MUST be in the format of a URL.
        /// </summary>
        public string? Url { get; set; }
        /// <summary>
        /// The email address of the contact person/organization.MUST be in the format of an email address.
        /// </summary>
        public string? Email { get; set; }
    }
    public class InfoLicense {
        public InfoLicense(JsonObject o) {
            string? name = null;
            foreach (var item in o.AsObject()) {
                switch (item.Key) {
                    case "name":
                        name = item.Value?.ToString();
                        break;
                    case "url":
                        Url = item.Value?.ToString();
                        break;
                    default:
                        throw new Exception("Unknown parameter \"" + item.Key + "\" in InfoLicense");
                }
            }
            if (string.IsNullOrEmpty(name)) {
                throw new Exception("Name of InfoLicense must not be null");
            }
            Name = name;
        }

        /// <summary>
        /// The license name used for the API.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// A URL to the license used for the API. MUST be in the format of a URL.
        /// </summary>
        public string? Url { get; set; }     
    }
}
