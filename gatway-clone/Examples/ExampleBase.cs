using gatway_clone.Objects;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace gatway_clone.Examples;

public abstract class ExampleBase<TRequest, TResponse> : MultipleExamplesProvider<RequestDTO<TRequest>>
          where TRequest : class, new()
          where TResponse : class, new()
{
    protected SwaggerExample<RequestDTO<TRequest>> CreateExample(string name, TRequest data)
            => Create(name, "", new RequestDTO<TRequest>()
            {
                RequestDateTime = DateTime.UtcNow.ToString(@"yyyy-MM-ddTHH:mm:ssZ"),
                RequestID = Guid.NewGuid().ToString(),
                Data = data,
                Language = "vi"
            });
}
public abstract class ExampleBase<TResponse> : MultipleExamplesProvider<ResponseDTO<TResponse>>
           where TResponse : class, new()
{
    protected SwaggerExample<ResponseDTO<TResponse>> CreateExample(string name, ResponseStatus responseStatus, TResponse data = null)
        => Create(name, "", new ResponseDTO<TResponse>()
        {
            RequestDateTime = DateTime.UtcNow.ToString(@"yyyy-MM-ddTHH:mm:ssZ"),
            RequestID = Guid.NewGuid().ToString(),
            Data = data,
            ResponseCode = responseStatus.GetResponseCode(),
            //Description = responseStatus.GetDescription("vi"),
            Status = responseStatus
        });
}
public abstract class MultipleExamplesProvider<T>
{
    public abstract IEnumerable<SwaggerExample<T>> GetSwaggerExamples();

    public Dictionary<string, OpenApiExample> GetExamples()
        => GetSwaggerExamples().GroupBy(ex => ex.Name, ex => ex.OpenApiExample)
                .ToDictionary(grouping => grouping.Key, grouping => grouping.First());

    protected SwaggerExample<T> Create(string name, string summary, T value)
    {
        return new SwaggerExample<T>()
        {
            Name = name,
            Value = value,
            OpenApiExample = new OpenApiExample()
            {
                Summary = summary,
                Value = new OpenApiString(JsonSerializer.Serialize(value, new JsonSerializerOptions()
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true
                }))
            }
        };
    }
}

public class SwaggerExample<T>
{
    public string Name { get; set; }

    public OpenApiExample OpenApiExample { get; set; }

    public T Value { get; set; }
}