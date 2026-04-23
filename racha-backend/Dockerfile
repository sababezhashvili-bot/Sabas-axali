# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/Racha629.Api/Racha629.Api.csproj src/Racha629.Api/
RUN dotnet restore src/Racha629.Api/Racha629.Api.csproj

COPY . .
RUN dotnet publish src/Racha629.Api/Racha629.Api.csproj \
    -c Release -o /app/publish --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Racha629.Api.dll"]
