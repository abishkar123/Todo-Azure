# Multi-stage Dockerfile for TaskFlow (ASP.NET Core 9)
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj and restore as distinct layers
COPY ["TodoApp.csproj", "./"]
RUN dotnet restore "./TodoApp.csproj"

# copy everything else and publish
COPY . .
RUN dotnet publish "TodoApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Bind to port 3000 inside container on all interfaces (0.0.0.0)
# IMPORTANT: When running with 'docker run', you MUST use -p flag to map ports:
#   Example: docker run -p 3000:3000 ... (maps host port 3000 to container port 3000)
#   Then access the app at: http://localhost:3000 or http://127.0.0.1:3000
ENV ASPNETCORE_URLS=http://+:3000
EXPOSE 3000

# Copy published output
COPY --from=build /app/publish .

# Important: app reads configuration from appsettings.json and environment variables.
# To supply MongoDB Atlas connection and settings at runtime, set environment variables:
#   MongoSettings__ConnectionString
#   MongoSettings__DatabaseName
#   MongoSettings__CollectionName
#
# BUILD AND RUN INSTRUCTIONS:
#   1. Build the image:
#      docker build -t todoapp:latest .
#
#   2. Run the container (MUST include -p flag for port mapping):
#      docker run -p 3000:3000 \
#        -e MongoSettings__ConnectionString="<your-connection-string>" \
#        -e MongoSettings__DatabaseName=TodoDb \
#        -e MongoSettings__CollectionName=Todos \
#        todoapp:latest
#
#   3. Access the application at: http://localhost:3000 or http://127.0.0.1:3000
#
#   Note: The -p 3000:3000 flag maps container port 3000 to host port 3000.
#         Without this flag, you will get "connection refused" errors.

ENTRYPOINT ["dotnet", "TodoApp.dll"]