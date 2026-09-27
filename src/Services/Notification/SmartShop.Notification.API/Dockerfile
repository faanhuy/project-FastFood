# Build context = thư mục gốc repo (xem docker-compose.yml). Chỉ copy Contracts + Notification,
# KHÔNG copy src/SmartShop.* của Core — chứng minh service deploy độc lập.

# ── Stage 1: Restore (chỉ copy csproj để cache layer) ─────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /app

COPY src/Shared/SmartShop.Contracts/SmartShop.Contracts.csproj                                                src/Shared/SmartShop.Contracts/
COPY src/Services/Notification/SmartShop.Notification.Domain/SmartShop.Notification.Domain.csproj             src/Services/Notification/SmartShop.Notification.Domain/
COPY src/Services/Notification/SmartShop.Notification.Application/SmartShop.Notification.Application.csproj   src/Services/Notification/SmartShop.Notification.Application/
COPY src/Services/Notification/SmartShop.Notification.Infrastructure/SmartShop.Notification.Infrastructure.csproj  src/Services/Notification/SmartShop.Notification.Infrastructure/
COPY src/Services/Notification/SmartShop.Notification.API/SmartShop.Notification.API.csproj                   src/Services/Notification/SmartShop.Notification.API/

RUN dotnet restore src/Services/Notification/SmartShop.Notification.API/SmartShop.Notification.API.csproj

# ── Stage 2: Publish ──────────────────────────────────────────────────────────
FROM restore AS publish
COPY src/Shared/SmartShop.Contracts/ src/Shared/SmartShop.Contracts/
COPY src/Services/Notification/      src/Services/Notification/
RUN dotnet publish src/Services/Notification/SmartShop.Notification.API/SmartShop.Notification.API.csproj \
    -c Release --no-restore -o /app/publish

# ── Stage 3: Runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser \
    && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SmartShop.Notification.API.dll"]
