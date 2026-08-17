# O Scraper é um Worker, não uma API: a imagem base é a `runtime`, não a `aspnet`.
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

# Os .csproj são copiados antes do resto do código para que a camada do `restore`
# fique em cache e só seja refeita quando uma dependência mudar.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RastreadorDePreco.Scraper/Host.csproj", "RastreadorDePreco.Scraper/"]
COPY ["Scraper.Application/Application.csproj", "Scraper.Application/"]
COPY ["Scraper.Adapters.Messaging/Adapters.Messaging.csproj", "Scraper.Adapters.Messaging/"]
COPY ["Adapters.Coletor/Adapters.Coletor.csproj", "Adapters.Coletor/"]
COPY ["Contracts/Rastreador.Contracts.csproj", "Contracts/"]
RUN dotnet restore "./RastreadorDePreco.Scraper/Host.csproj"
COPY . .
WORKDIR "/src/RastreadorDePreco.Scraper"
RUN dotnet build "./Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Host.dll"]
