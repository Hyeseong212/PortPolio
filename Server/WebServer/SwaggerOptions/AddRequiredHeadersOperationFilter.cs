using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

public class AddRequiredHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var excludedEndpoints = new[] { "Account/Create", "Account/Login" };

        // 엔드포인트가 제외 대상인지 확인
        if (context.ApiDescription.RelativePath != null &&
            excludedEndpoints.Any(e => context.ApiDescription.RelativePath.EndsWith(e, System.StringComparison.InvariantCultureIgnoreCase)))
        {
            return;
        }

        if (operation.Parameters == null)
            operation.Parameters = new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "SessionId",
            In = ParameterLocation.Header,
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "AccountId",
            In = ParameterLocation.Header,
            Required = true,
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
}
