# ContainerFlow Notification Service — built from repo root context (see docker-compose.yml)
# Service source: backend/src/Services/Notification/ContainerFlow.Notification.Api/

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the whole repo (trimmed by .dockerignore), then restore + publish only Notification.
COPY . .
RUN dotnet restore backend/src/Services/Notification/ContainerFlow.Notification.Api/ContainerFlow.Notification.Api.csproj
RUN dotnet publish backend/src/Services/Notification/ContainerFlow.Notification.Api/ContainerFlow.Notification.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl is required by the compose healthcheck (GET /health).
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ContainerFlow.Notification.Api.dll"]