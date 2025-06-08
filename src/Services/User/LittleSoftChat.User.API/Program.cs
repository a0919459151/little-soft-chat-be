using LittleSoftChat.User.Application;
using LittleSoftChat.User.Infrastructure;
using LittleSoftChat.Shared.Infrastructure;
using LittleSoftChat.User.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add application layers
builder.Services.AddUserApplication();
builder.Services.AddUserInfrastructure();
builder.Services.AddSharedInfrastructure(builder.Configuration);

// Add gRPC services
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<UserGrpcService>();

app.MapGet("/", () => "User Service is running!");

app.Run();
