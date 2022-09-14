using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System;
using gatway_clone.Utils;

namespace gatway_clone.Objects;

public class RequestDTO
{
    [JsonIgnore]
    public string LMID { get; set; } = LogUtil.GenerateLMID();

    [JsonIgnore]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Required]
    public string RequestDateTime { get; set; }

    [Required]
    public string RequestID { get; set; }
}
public  class RequestDTO<TRequest> : RequestDTO where TRequest : class
{
    public string Language { get; set; } = "en";

    public string UserAgent { get; set; }

    [Required]
    public TRequest Data { get; set; }
}
