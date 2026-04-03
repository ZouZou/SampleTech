# ==============================================================================
# Stage 1: Build
# ==============================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore dependencies first (layer-cache friendly)
COPY backend/InsurancePlatform.Domain/InsurancePlatform.Domain.csproj             InsurancePlatform.Domain/
COPY backend/InsurancePlatform.Application/InsurancePlatform.Application.csproj   InsurancePlatform.Application/
COPY backend/InsurancePlatform.Infrastructure/InsurancePlatform.Infrastructure.csproj InsurancePlatform.Infrastructure/
COPY backend/InsurancePlatform.Api/InsurancePlatform.Api.csproj                   InsurancePlatform.Api/
COPY backend/InsurancePlatform.slnx                                               ./

RUN dotnet restore InsurancePlatform.slnx

# Copy full source and publish
COPY backend/ .
RUN dotnet publish InsurancePlatform.Api/InsurancePlatform.Api.csproj \
    -c Release \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false

# ==============================================================================
# Stage 2: Runtime
# ==============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

# Security: run as non-root
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
WORKDIR /app

COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

# ASP.NET Core listens on 8080 by default (non-privileged port)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Liveness check — requires /health endpoint in the API
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "InsurancePlatform.Api.dll"]
