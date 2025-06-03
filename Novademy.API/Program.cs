using System.Reflection;
using System.Threading.RateLimiting;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using Novademy.API.Middlewares;
using Novademy.Application.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Routing;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddRepositories();
builder.Services.AddServices();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddExternalServices(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    #region Swagger Documentation
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    
    #endregion
    
    #region Swagger Authorization
    
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authorization",
        Description = "Enter a valid token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    
    options.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });
    
    #endregion
});

#region Key Vault

string keyVaultName = builder.Configuration["KeyVaultName"]!;
var keyVaultUri = $"https://{keyVaultName}.vault.azure.net/";
builder.Configuration.AddAzureKeyVault(
    new Uri(keyVaultUri),
    new DefaultAzureCredential(),
    new AzureKeyVaultConfigurationOptions());

#endregion

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder => builder
            .SetIsOriginAllowed(_ => true)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

#endregion

#region Rate Limiting

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("FixedWindowPolicy", config =>
    {
        config.PermitLimit = 100;
        config.Window = TimeSpan.FromMinutes(1);
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 10;
    });
    
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            Error = "TooManyRequests",
            Message = "Too many requests. Please try again later."
        }, cancellationToken);
    };
});

#endregion

var app = builder.Build();

// Add route debugging middleware
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request path: {Path}", context.Request.Path);
    logger.LogInformation("Request method: {Method}", context.Request.Method);
    logger.LogInformation("Request headers: {@Headers}", context.Request.Headers);
    
    // Log all registered routes
    var endpointDataSource = context.RequestServices.GetRequiredService<EndpointDataSource>();
    var endpoints = endpointDataSource.Endpoints;
    logger.LogInformation("Registered endpoints: {@Endpoints}", 
        endpoints.Select(e => new { 
            Path = e.DisplayName,
            Metadata = e.Metadata.Select(m => m.GetType().Name)
        }));
    
    await next();
});

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/ping", () => "pong");
app.Run();