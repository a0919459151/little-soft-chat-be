using LittleSoftChat.Shared.Infrastructure;
using LittleSoftChat.Shared.Contracts;
using LittleSoftChat.Gateway.API.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "LittleSoftChat Gateway API", 
        Version = "v1",
        Description = "LittleSoftChat 應用程式的 Web Gateway API"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add Shared Infrastructure (JWT, Database, etc.)
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Add gRPC clients
builder.Services.AddGrpcClient<LittleSoftChat.Shared.Contracts.UserService.UserServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcServices:UserService"] ?? "https://localhost:5101");
});

builder.Services.AddGrpcClient<LittleSoftChat.Shared.Contracts.ChatService.ChatServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcServices:ChatService"] ?? "https://localhost:5102");
});

// Add HTTP clients for Notification service
builder.Services.AddHttpClient<INotificationService, LittleSoftChat.Gateway.API.Services.NotificationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["HttpServices:NotificationService"] ?? "https://localhost:5103");
});

// Register Gateway services
builder.Services.AddScoped<IUserService, LittleSoftChat.Gateway.API.Services.UserService>();
builder.Services.AddScoped<IChatService, LittleSoftChat.Gateway.API.Services.ChatService>();
builder.Services.AddScoped<INotificationService, LittleSoftChat.Gateway.API.Services.NotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "LittleSoftChat Gateway API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Use shared infrastructure middleware
app.UseSharedInfrastructure();

app.MapControllers();

app.MapGet("/health", () => "Gateway API is running!");

app.Run();
