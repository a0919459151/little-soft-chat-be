using LittleSoftChat.Chat.Application;
using LittleSoftChat.Chat.Infrastructure;
using LittleSoftChat.Chat.API.Services;
using LittleSoftChat.Shared.Infrastructure;
using LittleSoftChat.Shared.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// Add JWT Authentication - handled by SharedInfrastructure

// Add gRPC
builder.Services.AddGrpc();

// Register application dependencies
builder.Services.AddChatApplication();
builder.Services.AddChatInfrastructure();
builder.Services.AddSharedInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Use shared infrastructure middleware (includes validation exception handling)
app.UseSharedInfrastructure();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<ChatGrpcService>();

app.Run();
