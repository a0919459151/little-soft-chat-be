using LittleSoftChat.Gateway.API.DI;

var builder = WebApplication.CreateBuilder(args);

// 註冊所有 Gateway 服務
builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

// 配置中間件管道
app.UseGatewayMiddleware();

app.Run();
