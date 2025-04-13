using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Backend
{
    public class SchemaTransformer : IOpenApiSchemaTransformer
    {
        private Dictionary<Type, OpenApiSchema> _existing = new();

        public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
        {
            if (!context.JsonTypeInfo.Type.Namespace.StartsWith("Backend.Models")) return Task.CompletedTask;
            if (!_existing.ContainsKey(context.JsonTypeInfo.Type))
                _existing.Add(context.JsonTypeInfo.Type, schema);
            if (schema.Properties is not null)
            {
                Console.WriteLine($"Schema ref: {schema.Reference}");
                foreach (var property in schema.Properties)
                {
                    Console.WriteLine($"Schema property ref: {property.Value.Reference?.Id ?? "No ref"}");
                }
                Console.WriteLine();
            }
            return Task.CompletedTask;
        }
    }
}
