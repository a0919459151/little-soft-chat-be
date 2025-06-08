using LittleSoftChat.Gateway.Presentation.DI;

// 在 Docker 環境中啟用 HTTP/2 over HTTP 支持
AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

// 註冊所有 Gateway 服務
builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

// 配置中間件管道
app.UseGatewayMiddleware();

app.Run();
