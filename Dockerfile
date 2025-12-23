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

# bind to port 3000 inside container
# the app will listen on port 3000 inside the container; map it to any host port you like
ENV ASPNETCORE_URLS=http://+:3000
EXPOSE 3000

# Copy published output
COPY --from=build /app/publish .

# Important: app reads configuration from appsettings.json and environment variables.
# To supply MongoDB Atlas connection and settings at runtime, set environment variables:
#   MongoSettings__ConnectionString
#   MongoSettings__DatabaseName
#   MongoSettings__CollectionName
# Example when running the container:
#   docker run -e MongoSettings__ConnectionString="<your-conn>" -e MongoSettings__DatabaseName=TodoDb -e MongoSettings__CollectionName=Todos -p 8080:80 yourimage

ENTRYPOINT ["dotnet", "TodoApp.dll"]
