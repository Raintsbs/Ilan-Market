using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AdvertisementApp.API.Filters
{
    /// <summary>multipart/form-data + IFormFile uçları için Swagger şeması.</summary>
    public sealed class FormFileOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var formFileParams = context.ApiDescription.ParameterDescriptions
                .Where(p =>
                    p.Source.Id.Equals("Form", StringComparison.OrdinalIgnoreCase) &&
                    (p.Type == typeof(IFormFile) ||
                     p.Type == typeof(IFormFile[]) ||
                     (p.Type.IsGenericType &&
                      p.Type.GetGenericTypeDefinition() == typeof(List<>) &&
                      p.Type.GetGenericArguments()[0] == typeof(IFormFile))))
                .ToList();

            if (formFileParams.Count == 0)
                return;

            var properties = new Dictionary<string, OpenApiSchema>();

            foreach (var param in context.ApiDescription.ParameterDescriptions
                         .Where(p => p.Source.Id.Equals("Form", StringComparison.OrdinalIgnoreCase)))
            {
                if (param.Type == typeof(IFormFile))
                {
                    properties[param.Name] = new OpenApiSchema { Type = "string", Format = "binary" };
                    continue;
                }

                if (param.Type == typeof(IFormFile[]) ||
                    (param.Type.IsGenericType &&
                     param.Type.GetGenericTypeDefinition() == typeof(List<>) &&
                     param.Type.GetGenericArguments()[0] == typeof(IFormFile)))
                {
                    properties[param.Name] = new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema { Type = "string", Format = "binary" },
                    };
                    continue;
                }

                if (param.ModelMetadata?.Properties is not { } modelProps)
                    continue;

                foreach (var prop in modelProps)
                {
                    properties[prop.Name] = context.SchemaGenerator.GenerateSchema(
                        prop.ModelType,
                        context.SchemaRepository);
                }
            }

            operation.Parameters?.Clear();
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = properties,
                        },
                    },
                },
            };
        }
    }
}
