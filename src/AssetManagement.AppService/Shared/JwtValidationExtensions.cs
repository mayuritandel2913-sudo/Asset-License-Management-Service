using AssetManagement.Utility;
using AssetManagement.Utility.Resource;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using System.Text.Json;
 
namespace AssetManagement.AppService.Shared
{
    public static class JwtValidationExtensions
    {
        public static void AddJwtValidation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSection = configuration.GetSection("Jwt");
                var key = jwtSection["Key"];
                var issuer = jwtSection["Issuer"];
                var audience = jwtSection["Audience"];
 
                if (string.IsNullOrEmpty(key))
                {
                    throw new InvalidOperationException("Jwt Key is not configured in appsettings.json.");
                }
 
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    ClockSkew = TimeSpan.Zero
                };
 

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var response = Envelope.Error(CommonResource.UnAuthServiceorized, StatusCodes.Status401Unauthorized);
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var response = Envelope.Error(CommonResource.UnAuthServiceorized, StatusCodes.Status403Forbidden);
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    }
                };
            });
        }
    }
}