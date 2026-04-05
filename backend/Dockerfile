FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore (layer-cache friendly)
COPY SampleTech.Api/SampleTech.Api.csproj SampleTech.Api/
RUN dotnet restore SampleTech.Api/SampleTech.Api.csproj

# Build & publish
COPY SampleTech.Api/ SampleTech.Api/
RUN dotnet publish SampleTech.Api/SampleTech.Api.csproj \
    -c Release \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
WORKDIR /app

COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD wget -qO- http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SampleTech.Api.dll"]
