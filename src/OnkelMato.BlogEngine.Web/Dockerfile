# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["OnkelMato.BlogEngine.Web/OnkelMato.BlogEngine.Web.csproj", "OnkelMato.BlogEngine.Web/"]
RUN dotnet restore "./OnkelMato.BlogEngine.Web/OnkelMato.BlogEngine.Web.csproj"
COPY . .
WORKDIR "/src/OnkelMato.BlogEngine.Web"
ENV GITVERSION_DISABLED=true
RUN dotnet build "./OnkelMato.BlogEngine.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./OnkelMato.BlogEngine.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
USER 0
RUN mkdir /data
RUN mkdir /data/keys
RUN chown -R $APP_UID /data

USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnkelMato.BlogEngine.Web.dll"]
