# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY *.sln .

# Copy project files for User Service
COPY src/Services/User/LittleSoftChat.User.Presentation/*.csproj src/Services/User/LittleSoftChat.User.Presentation/
COPY src/Services/User/LittleSoftChat.User.Application/*.csproj src/Services/User/LittleSoftChat.User.Application/
COPY src/Services/User/LittleSoftChat.User.Domain/*.csproj src/Services/User/LittleSoftChat.User.Domain/
COPY src/Services/User/LittleSoftChat.User.Infrastructure/*.csproj src/Services/User/LittleSoftChat.User.Infrastructure/

# Copy shared project files
COPY src/Shared/LittleSoftChat.Shared.Contracts/*.csproj src/Shared/LittleSoftChat.Shared.Contracts/
COPY src/Shared/LittleSoftChat.Shared.Domain/*.csproj src/Shared/LittleSoftChat.Shared.Domain/
COPY src/Shared/LittleSoftChat.Shared.Infrastructure/*.csproj src/Shared/LittleSoftChat.Shared.Infrastructure/

# Restore dependencies
RUN dotnet restore src/Services/User/LittleSoftChat.User.Presentation/LittleSoftChat.User.Presentation.csproj

# Copy source code
COPY src/ src/

# Build application
WORKDIR /src/src/Services/User/LittleSoftChat.User.Presentation
RUN dotnet build -c Release -o /app/build

# Publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Expose ports (HTTP API and gRPC)
EXPOSE 80 5011

# Add health check endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
  CMD curl -f http://localhost/health || exit 1

ENTRYPOINT ["dotnet", "LittleSoftChat.User.Presentation.dll"]
