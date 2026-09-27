# Multi-stage build for ASP.NET Core 8 Web API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY Backend.csproj ./
RUN dotnet restore Backend.csproj

# Copy remaining source code and publish release
COPY . ./
RUN dotnet publish Backend.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Ensure Data directory exists for JSON storage
RUN mkdir -p /app/Data

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "Backend.dll"]
