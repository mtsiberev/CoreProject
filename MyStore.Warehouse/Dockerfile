# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y libgssapi-krb5-2 && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MyStore.Warehouse/MyStore.Warehouse.csproj", "MyStore.Warehouse/"]
RUN dotnet restore "./MyStore.Warehouse/MyStore.Warehouse.csproj"
COPY . .
WORKDIR "/src/MyStore.Warehouse"
RUN dotnet build "./MyStore.Warehouse.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MyStore.Warehouse.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyStore.Warehouse.dll"]


# Kubernetes
FROM base AS final-k8s
WORKDIR /app

COPY MyStore.Warehouse/out .
ENTRYPOINT ["dotnet", "MyStore.Warehouse.dll"]
