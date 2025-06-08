# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY *.sln .

# Copy project files for Migration Tool
COPY src/Tools/LittleSoftChat.Database.Migration/*.csproj src/Tools/LittleSoftChat.Database.Migration/

# Copy shared project files
COPY src/Shared/LittleSoftChat.Shared.Domain/*.csproj src/Shared/LittleSoftChat.Shared.Domain/
COPY src/Shared/LittleSoftChat.Shared.Infrastructure/*.csproj src/Shared/LittleSoftChat.Shared.Infrastructure/

# Copy User and Chat domain projects for migrations
COPY src/Services/User/LittleSoftChat.User.Domain/*.csproj src/Services/User/LittleSoftChat.User.Domain/
COPY src/Services/User/LittleSoftChat.User.Infrastructure/*.csproj src/Services/User/LittleSoftChat.User.Infrastructure/
COPY src/Services/Chat/LittleSoftChat.Chat.Domain/*.csproj src/Services/Chat/LittleSoftChat.Chat.Domain/
COPY src/Services/Chat/LittleSoftChat.Chat.Infrastructure/*.csproj src/Services/Chat/LittleSoftChat.Chat.Infrastructure/
COPY src/Services/Notification/LittleSoftChat.Notification.Domain/*.csproj src/Services/Notification/LittleSoftChat.Notification.Domain/
COPY src/Services/Notification/LittleSoftChat.Notification.Infrastructure/*.csproj src/Services/Notification/LittleSoftChat.Notification.Infrastructure/

# Restore dependencies
RUN dotnet restore src/Tools/LittleSoftChat.Database.Migration/LittleSoftChat.Database.Migration.csproj

# Copy source code
COPY src/ src/

# Build application
WORKDIR /src/src/Tools/LittleSoftChat.Database.Migration
RUN dotnet build -c Release -o /app/build

# Publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app

# Install mysql client for connection testing
RUN apt-get update && apt-get install -y default-mysql-client && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "LittleSoftChat.Database.Migration.dll"]
