using Auth0.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NeoHearts_API.Services;

var builder = WebApplication.CreateBuilder(args);

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
        builder.WithOrigins("http://localhost:5173")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
Console.WriteLine(builder.Configuration["Auth0:Domain"]);
//builder.Services
//    .AddAuth0WebAppAuthentication(options => {
//        options.Domain = builder.Configuration["Auth0:Domain"];
//        options.ClientId = builder.Configuration["Auth0:ClientId"];
//        options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
//        options.Scope = "openid profile email"; // Add required scopes
//    });

builder.Services.AddHttpClient();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth0:Domain"];  // Auth0 Domain (e.g., https://dev-yourapp.auth0.com)
        options.Audience = builder.Configuration["Auth0:Audience"];  // Your Auth0 ClientId (used as the Audience in the JWT)
        options.RequireHttpsMetadata = false;  // For development. Set to true in production!
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
