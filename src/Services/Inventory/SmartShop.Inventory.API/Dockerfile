# Build context = thư mục gốc repo (xem docker-compose.yml). Chỉ copy Contracts + Inventory,
# KHÔNG copy src/SmartShop.* của Core — chứng minh service deploy độc lập.

# ── Stage 1: Restore (chỉ copy csproj để cache layer) ─────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /app

COPY src/Shared/SmartShop.Contracts/SmartShop.Contracts.csproj                                            src/Shared/SmartShop.Contracts/
COPY src/Services/Inventory/SmartShop.Inventory.Domain/SmartShop.Inventory.Domain.csproj                  src/Services/Inventory/SmartShop.Inventory.Domain/
COPY src/Services/Inventory/SmartShop.Inventory.Application/SmartShop.Inventory.Application.csproj        src/Services/Inventory/SmartShop.Inventory.Application/
COPY src/Services/Inventory/SmartShop.Inventory.Infrastructure/SmartShop.Inventory.Infrastructure.csproj  src/Services/Inventory/SmartShop.Inventory.Infrastructure/
COPY src/Services/Inventory/SmartShop.Inventory.API/SmartShop.Inventory.API.csproj                        src/Services/Inventory/SmartShop.Inventory.API/

RUN dotnet restore src/Services/Inventory/SmartShop.Inventory.API/SmartShop.Inventory.API.csproj

# ── Stage 2: Publish ──────────────────────────────────────────────────────────
FROM restore AS publish
COPY src/Shared/SmartShop.Contracts/ src/Shared/SmartShop.Contracts/
COPY src/Services/Inventory/         src/Services/Inventory/
RUN dotnet publish src/Services/Inventory/SmartShop.Inventory.API/SmartShop.Inventory.API.csproj \
    -c Release --no-restore -o /app/publish

# ── Stage 3: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

# gRPC chạy h2c (HTTP/2 không TLS) — Kestrel:EndpointDefaults:Protocols=Http2 nằm trong appsettings.json
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SmartShop.Inventory.API.dll"]
