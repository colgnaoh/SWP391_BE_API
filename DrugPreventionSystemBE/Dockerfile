# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
# EXPOSE 8080
# EXPOSE 8081
# EXPOSE 80

# ENV ASPNETCORE_URLS="http://+:80"


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY DrugPreventionSystemBE.csproj .
RUN dotnet restore DrugPreventionSystemBE.csproj
COPY . .
WORKDIR /src

RUN dotnet build DrugPreventionSystemBE.csproj -c $BUILD_CONFIGURATION --no-restore
# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish DrugPreventionSystemBE.csproj -c $BUILD_CONFIGURATION -o /app/publish --no-build
# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:$PORT
ENTRYPOINT ["dotnet", "DrugPreventionSystemBE.dll"]