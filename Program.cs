using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using NeoHearts_API.Services;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();

// Get Auth0 secrets from AWS
string secretJson = await SecretsManagerHelper.GetSecretAsync("Neohearts-auth0");
var secretData = JObject.Parse(secretJson);

string auth0Domain = secretData["AUTH0_DOMAIN"]?.ToString();
string auth0Audience = secretData["AUTH0_AUDIENCE"]?.ToString();

builder.Configuration.AddEnvironmentVariables(); // Allow access using Environment.GetEnvironmentVariable

// sets up routing for API endpoints using attributes like [Route] and [HttpGet]
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IFhirBundleService, FhirBundleServices>();
builder.Services.AddScoped<IFhirDataMappingService, FhirDataMappingService>();
builder.Services.AddScoped<IFhirUpdateService, FhirUpdateService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
            .WithOrigins(
                Environment.GetEnvironmentVariable("FRONTEND_URL1"),
                Environment.GetEnvironmentVariable("FRONTEND_URL2"),
                "http://localhost:3000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddHttpClient();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = auth0Domain;
        options.Audience = auth0Audience;
        options.RequireHttpsMetadata = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        logger.LogError(exception, "Global error: {Message}", exception?.Message);
        await context.Response.WriteAsync("An error occurred.");
    });
});
app.Run();
