using AssetManagement.Infrastructure.Extensions;
using AssetManagement.AppService.Extensions;
using AssetManagement.AppService.Shared;
using Microsoft.AspNetCore.Mvc;
using AssetManagement.Utility;
using AssetManagement.Utility.Converters;
using AssetManagement.Infrastructure.Contracts;
using AssetManagement.Infrastructure.Repository;

namespace AssetManagement.Api.Extensions;

public static class StartUp
{
    public static void AddStartUpServices(this WebApplicationBuilder builder)
    { 
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                options.JsonSerializerOptions.Converters.Add(new CustomDateTimeConverter());
                options.JsonSerializerOptions.Converters.Add(new CustomNullableDateTimeConverter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .SelectMany(e => e.Value!.Errors.Select(x => x.ErrorMessage))
                        .ToList();

                    var firstError = errors.FirstOrDefault() ?? "Validation failed";
                    var envelope = Envelope.Error(firstError, StatusCodes.Status400BadRequest);
                    return new BadRequestObjectResult(envelope);
                };
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddService(builder.Configuration);
  
        builder.Services.AddJwtValidation(builder.Configuration);
        builder.Services.AddAuthentication();
        
        builder.Services.AddAuthorization();
        builder.Services.AddSwaggerGen();
    }
}
