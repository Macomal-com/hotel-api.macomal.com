using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RepositoryModels.Repositories
{
public class AddRequiredHeaderParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<OpenApiParameter>();
            }

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Database",
                In = ParameterLocation.Header,
                Required = false, // Set to true if the header is required
                Schema = new OpenApiSchema
                {
                    Type = "String" // Adjust the type based on your header's type
                }
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "CompanyId",
                In = ParameterLocation.Header,
                Required = false, // Set to true if the header is required
                Schema = new OpenApiSchema
                {
                    Type = "int" // Adjust the type based on your header's type
                }
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "UserId",
                In = ParameterLocation.Header,
                Required = false, // Set to true if the header is required
                Schema = new OpenApiSchema
                {
                    Type = "int" // Adjust the type based on your header's type
                }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "UserRole",
                In = ParameterLocation.Header,
                Required = false, // Set to true if the header is required
                Schema = new OpenApiSchema
                {
                    Type = "string" // Adjust the type based on your header's type
                }
            });

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "FinancialYear",
                In = ParameterLocation.Header,
                Required = false, // Set to true if the header is required
                Schema = new OpenApiSchema
                {
                    Type = "String" // Adjust the type based on your header's type
                }
            });
        }

        //public void Apply(OpenApiOperation operation, OperationFilterContext context)
        //{
        //    throw new NotImplementedException();
        //}
    }

}
