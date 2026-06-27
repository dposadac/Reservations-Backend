# =============================================================================
# Imagen de la API (multi-stage). Contexto de build = raíz del repositorio.
# =============================================================================

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# 1) Copia solo los .csproj para aprovechar la caché de restore.
COPY src/Domain/Domain.csproj            src/Domain/
COPY src/Application/Application.csproj   src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/Api/Api.csproj                   src/Api/
RUN dotnet restore src/Api/Api.csproj

# 2) Copia el código y publica en Release.
COPY src/ src/
RUN dotnet publish src/Api/Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# =============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Cloud Run inyecta PORT=8080 por defecto; la API escucha ahí.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Ejecuta como usuario no root (presente en las imágenes oficiales de .NET).
USER $APP_UID

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Ceiba.LiveEvent.Reservations.Api.dll"]
