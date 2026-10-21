# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
EXPOSE 8080
WORKDIR "/app"

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
# src/src/Threads.Api
WORKDIR "/src"
COPY ["Directory.Packages.props","src/Threads.Api/"]
COPY ["src/Threads.Api/Threads.Api.csproj", "src/Threads.Api/"]
RUN dotnet restore "./src/Threads.Api/Threads.Api.csproj"
COPY [".", "."]
RUN dotnet publish "./src/Threads.Api/Threads.Api.csproj" -c $BUILD_CONFIGURATION -o "/app/publish" /p:UseAppHost=false

FROM base AS final
WORKDIR "/app"
COPY --from=build ["/app/publish/", "."]
ENTRYPOINT ["dotnet", "Threads.Api.dll"]

