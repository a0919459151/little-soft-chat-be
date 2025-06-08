# LittleSoftChat Gateway API

## Overview

The LittleSoftChat Gateway API serves as the unified entry point for all client applications to interact with the LittleSoftChat microservices ecosystem. This gateway implements the API Gateway pattern, providing a single point of access that handles authentication, routing, and communication with multiple backend microservices.

## Architecture

```
[Frontend Apps] -> [Gateway API] -> [User Service | Chat Service | Notification Service]
                                         |             |              |
                                      [gRPC]       [gRPC]        [HTTP + SignalR]
```

### Key Features

- **Unified API Interface**: Single endpoint for all frontend applications
- **JWT Authentication**: Centralized authentication and authorization
- **Service Orchestration**: Coordinates calls to multiple microservices
- **Auto-generated Documentation**: Swagger/OpenAPI documentation
- **Real-time Support**: SignalR integration for live notifications
- **CORS Configuration**: Cross-origin support for web applications
- **Health Checks**: Monitoring endpoints for service health

## Prerequisites

- .NET 8.0 SDK
- Running instances of:
  - User Service (gRPC on port 7101)
  - Chat Service (gRPC on port 7102)
  - Notification Service (HTTP on port 7103)

## Getting Started

### 1. Configuration

Update `appsettings.json` with your environment-specific settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-database-connection-string"
  },
  "JwtSettings": {
    "SecretKey": "your-secret-key-at-least-32-characters",
    "Issuer": "LittleSoftChat.Gateway",
    "Audience": "LittleSoftChat.Client",
    "ExpirationMinutes": 60
  },
  "ServiceUrls": {
    "UserService": "https://localhost:7101",
    "ChatService": "https://localhost:7102",
    "NotificationService": "https://localhost:7103"
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:3001"
    ]
  }
}
```

### 2. Installation

```bash
# Clone the repository (if not already done)
cd src/Gateway/LittleSoftChat.Gateway.API

# Restore dependencies
dotnet restore

# Build the project
dotnet build
```

### 3. Running the Gateway

```bash
# Run in development mode
dotnet run

# Or run with specific environment
dotnet run --environment Development
```

The Gateway will be available at:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5001`
- Swagger UI: `https://localhost:7001/swagger`

## API Documentation

### Interactive Documentation
Visit `https://localhost:7001/swagger` for interactive API documentation powered by Swagger UI.

### Static Documentation
See [api-documentation.md](./api-documentation.md) for comprehensive API documentation.

### Postman Collection
Import [postman-collection.json](./postman-collection.json) into Postman for easy API testing.

## Project Structure

```
LittleSoftChat.Gateway.API/
├── Controllers/           # API Controllers
│   ├── AuthController.cs
│   ├── ChatController.cs
│   ├── FriendsController.cs
│   └── NotificationsController.cs
├── Models/               # Request/Response DTOs
│   ├── AuthModels.cs
│   ├── ChatModels.cs
│   ├── FriendModels.cs
│   └── NotificationModels.cs
├── Services/             # Service Layer
│   ├── UserService.cs
│   ├── ChatService.cs
│   └── NotificationService.cs
├── Program.cs           # Application entry point
├── appsettings.json     # Configuration
└── LittleSoftChat.Gateway.API.csproj
```

## Service Integration

The Gateway communicates with microservices using different protocols:

### User Service (gRPC)
- User authentication and management
- JWT token generation and validation
- User profile operations

### Chat Service (gRPC)
- Message management
- Conversation handling
- Chat history retrieval

### Notification Service (HTTP + SignalR)
- Push notifications
- Real-time updates
- SignalR hub for live communication

## Authentication Flow

1. Client sends login credentials to `/api/auth/login`
2. Gateway validates credentials via User Service (gRPC)
3. Gateway generates JWT token and returns to client
4. Client includes JWT token in subsequent requests
5. Gateway validates token and forwards requests to appropriate services

## Development

### Adding New Endpoints

1. **Create/Update Models**: Add request/response DTOs in `Models/`
2. **Implement Service**: Add service methods in `Services/`
3. **Add Controller**: Create controller actions in `Controllers/`
4. **Update Documentation**: Add XML comments for Swagger

### Testing

```bash
# Run unit tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Code Quality

```bash
# Format code
dotnet format

# Analyze code
dotnet build --verbosity normal
```

## Deployment

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Gateway/LittleSoftChat.Gateway.API/LittleSoftChat.Gateway.API.csproj", "Gateway/"]
RUN dotnet restore "Gateway/LittleSoftChat.Gateway.API.csproj"
COPY . .
WORKDIR "/src/Gateway"
RUN dotnet build "LittleSoftChat.Gateway.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LittleSoftChat.Gateway.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LittleSoftChat.Gateway.API.dll"]
```

### Environment Variables

For production deployment, set these environment variables:

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:443;http://+:80
ConnectionStrings__DefaultConnection="production-db-connection"
JwtSettings__SecretKey="production-secret-key"
ServiceUrls__UserService="https://user-service-url"
ServiceUrls__ChatService="https://chat-service-url"
ServiceUrls__NotificationService="https://notification-service-url"
```

## Monitoring and Health Checks

### Health Check Endpoints

- `/health`: Overall health status
- `/health/ready`: Readiness probe
- `/health/live`: Liveness probe

### Logging

The Gateway uses structured logging with Serilog. Logs include:
- Request/response details
- Service communication logs
- Authentication events
- Error tracking

### Metrics

Integration with monitoring tools:
- Application Insights (Azure)
- Prometheus metrics
- Custom telemetry

## Security Considerations

### JWT Configuration
- Use strong secret keys (minimum 256 bits)
- Set appropriate token expiration times
- Implement refresh token rotation
- Store secrets in secure key vaults

### CORS Policy
- Configure allowed origins restrictively
- Avoid wildcard origins in production
- Set appropriate headers and methods

### Rate Limiting
- Implement rate limiting per endpoint
- Configure different limits for auth vs. data endpoints
- Use distributed rate limiting in multi-instance deployments

## Troubleshooting

### Common Issues

1. **Service Connection Errors**
   - Verify microservice URLs in configuration
   - Check network connectivity
   - Ensure services are running and healthy

2. **Authentication Issues**
   - Verify JWT secret key configuration
   - Check token expiration settings
   - Validate issuer/audience claims

3. **CORS Errors**
   - Update allowed origins in configuration
   - Check frontend URL configuration
   - Verify HTTP vs HTTPS protocol matching

### Debug Mode

Enable detailed logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Grpc": "Debug"
    }
  }
}
```

## Performance Optimization

### Caching
- Implement response caching for static data
- Use Redis for distributed caching
- Cache user authentication data

### Connection Pooling
- Configure gRPC channel pooling
- Optimize HTTP client connections
- Implement circuit breaker pattern

### Load Balancing
- Deploy multiple Gateway instances
- Use sticky sessions for SignalR
- Implement health-aware load balancing

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Update documentation
6. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the troubleshooting section above
