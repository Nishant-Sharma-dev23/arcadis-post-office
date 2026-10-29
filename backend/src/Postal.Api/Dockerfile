# Build context is the backend/ folder (see docker-compose.yml).

# Stage 1: build and publish with the full SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, so this layer is cached until project files change
COPY Directory.Build.props ./
COPY src/Postal.Api/Postal.Api.csproj src/Postal.Api/
RUN dotnet restore src/Postal.Api/Postal.Api.csproj

COPY src/Postal.Api/ src/Postal.Api/
RUN dotnet publish src/Postal.Api/Postal.Api.csproj -c Release -o /app --no-restore

# Stage 2: run on the smaller ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Postal.Api.dll"]
