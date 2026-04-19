# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY SaasStarter.Domain/SaasStarter.Domain.csproj             SaasStarter.Domain/
COPY SaasStarter.Application/SaasStarter.Application.csproj   SaasStarter.Application/
COPY SaasStarter.Infra/SaasStarter.Infra.csproj               SaasStarter.Infra/
COPY SaasStarter.Api/SaasStarter.Api.csproj                   SaasStarter.Api/

RUN dotnet restore SaasStarter.Api/SaasStarter.Api.csproj

COPY . .

RUN dotnet publish SaasStarter.Api/SaasStarter.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build --chown=appuser /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SaasStarter.Api.dll"]
