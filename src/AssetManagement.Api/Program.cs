using AssetManagement.Api.Extensions;
using AssetManagement.API.Middleware;
using AssetManagement.Utility.Logging;


var builder = WebApplication.CreateBuilder(args);


builder.Logging.AddProvider(new FileLoggerProvider(
    Path.Combine(builder.Environment.ContentRootPath, "Logs", $"app-{DateTime.Now:yyyy-MM-dd}.txt")
));

builder.AddStartUpServices();


var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
await app.RunAsync();
