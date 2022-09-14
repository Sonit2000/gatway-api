using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using gatway_clone.Utils;

namespace gatway_clone.Examples;

public class ExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var parameterDescription in context.ApiDescription.ParameterDescriptions)
        {
            var examples = GetExampleForType(parameterDescription.Type);
            if (examples == null || operation.RequestBody?.Content == null)
            {
                continue;
            }
            foreach (var content in operation.RequestBody.Content)
            {
                content.Value.Examples = examples;
            }
        }
        foreach (var response in context.ApiDescription.SupportedResponseTypes)
        {
            var examples = GetExampleForType(response.Type);
            if (examples == null)
            {
                continue;
            }

            var operationResponse = operation.Responses.FirstOrDefault(r => r.Key == response.StatusCode.ToString());
            if (operationResponse.Equals(default(KeyValuePair<string, OpenApiResponse>)) || operationResponse.Value == null)
            {
                continue;
            }
            foreach (var content in operationResponse.Value.Content)
            {
                content.Value.Examples = examples;
            }
        }
    }

     public Dictionary<string, OpenApiExample> GetExampleForType(Type type)
     {
            Type t = typeof(MultipleExamplesProvider<>).MakeGenericType(type);
            Type exampleProviderType = GetAssembly(type).GetTypes(t).FirstOrDefault();
            if (exampleProviderType == null)
                return null;
            object exampleProviderObject = Activator.CreateInstance(exampleProviderType);
            return t.GetMethod("GetExamples").Invoke(exampleProviderObject, null) as Dictionary<string, OpenApiExample>;
     }
     private Assembly GetAssembly(Type type) => type.GenericTypeArguments.Length > 0 ? GetAssembly(type.GenericTypeArguments[0]) : type.Assembly;
}
