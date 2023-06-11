using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiGenerator
    {
    internal class TopicTypeInfo {
        //[JsonConverter(typeof(JsonEnumMemberStringEnumConverter))]
        //public enum TransportsConstant {
        //    [JsonEnumName("All")]
        //    All,
        //    [JsonEnumName("Websocket")]
        //    Websocket,
        //    [JsonEnumName("EventBridge")]
        //    EventBridge,
        //    [JsonEnumName("ProcessAutomation")]
        //    ProcessAutomation,
        //}

        public TopicTypeInfo(string typeName, string[]? topicParameters, string[]? parameterDescriptions, IEnumerable<AvailableTopic.TransportsConstant> transport, string? description) {
            TypeName = typeName;
            TopicParameters = topicParameters;
            ParameterDescriptions = parameterDescriptions;
            Transport = transport;
            Description = description;
        }

        public string TypeName { get; set; }
        public string[]? TopicParameters { get; set; }
        public string[]? ParameterDescriptions { get; set; }
        public IEnumerable<AvailableTopic.TransportsConstant> Transport { get; set; }
        public string? Description { get; set; }
    }
}
