using System.Text.Json.Serialization;

namespace gatway_clone.Objects
{
    public class ResponseDTO<TResponse> : RequestDTO
    {
        public string ResponseCode { get; set; }
        public string Description { get; set; }
        public TResponse Data { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ResponseStatus Status { get; set; }
    }
}
