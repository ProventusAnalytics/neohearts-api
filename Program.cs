using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NeoHearts_API.Services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load from .env
DotNetEnv.Env.Load();

builder.Configuration
    .AddEnvironmentVariables(); // Allow access using Environment.GetEnvironmentVariable

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
        builder.WithOrigins("https://main.d2m38mqvlgl9jn.amplifyapp.com")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddHttpClient();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = Environment.GetEnvironmentVariable("AUTH0_DOMAIN");
        options.Audience = Environment.GetEnvironmentVariable("AUTH0_AUDIENCE");
        options.RequireHttpsMetadata = true;  // For dev only
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

app.Run();
