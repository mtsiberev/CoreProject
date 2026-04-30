# См. https://aka.ms/customizecontainer, чтобы узнать, как Visual Studio использует этот Dockerfile для ускорения отладки.

# Стадия base: используем aspnet, чтобы был доступен фреймворк Microsoft.AspNetCore.App
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

# Стадия build: используем nightly SDK для .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyStore.Warehouse/MyStore.Warehouse.csproj", "MyStore.Warehouse/"]
RUN dotnet restore "./MyStore.Warehouse/MyStore.Warehouse.csproj"
COPY . .
WORKDIR "/src/MyStore.Warehouse"
RUN dotnet build "./MyStore.Warehouse.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Стадия publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MyStore.Warehouse.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальная стадия
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyStore.Warehouse.dll"]
