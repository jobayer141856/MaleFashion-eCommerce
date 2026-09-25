# Multi-stage Dockerfile for MaleFashion.Web (.NET 10)
# Stage 1: Runtime Base
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: SDK Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files for layer caching
COPY ["src/MaleFashion.Domain/MaleFashion.Domain.csproj", "MaleFashion.Domain/"]
COPY ["src/MaleFashion.Application/MaleFashion.Application.csproj", "MaleFashion.Application/"]
COPY ["src/MaleFashion.Infrastructure/MaleFashion.Infrastructure.csproj", "MaleFashion.Infrastructure/"]
COPY ["src/MaleFashion.Web/MaleFashion.Web.csproj", "MaleFashion.Web/"]

# Restore dependencies
RUN dotnet restore "MaleFashion.Web/MaleFashion.Web.csproj"

# Copy source code from ./src directly into /src
COPY ./src/ .

# Build and publish application
WORKDIR "/src/MaleFashion.Web"
RUN dotnet publish "MaleFashion.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 3: Final runtime image
FROM base AS final
WORKDIR /app
COPY --chown=$APP_UID:$APP_UID --from=build /app/publish .

# Ensure required runtime directories exist and grant permissions to non-root app user
USER root
RUN mkdir -p /app/Logs /app/wwwroot/uploads && chown -R $APP_UID:$APP_UID /app/Logs /app/wwwroot/uploads
USER $APP_UID

ENTRYPOINT ["dotnet", "MaleFashion.Web.dll"]

