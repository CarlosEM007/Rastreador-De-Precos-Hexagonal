# Acesse https://aka.ms/customizecontainer para saber como personalizar seu contêiner de depuração e como o Visual Studio usa este Dockerfile para criar suas imagens para uma depuração mais rápida.

# Esta fase é usada durante a execução no VS no modo rápido (Padrão para a configuração de Depuração)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 5250


# Esta fase é usada para compilar o projeto de serviço
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Bootstrap/Bootstrap.csproj", "Bootstrap/"]
COPY ["Adapters.Notification/Adapters.Notification.csproj", "Adapters.Notification/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Adapters.Persistence/Adapters.Persistence.csproj", "Adapters.Persistence/"]
COPY ["Adapters.Rest/Adapters.Rest.csproj", "Adapters.Rest/"]
COPY ["Adapters.Scraper/Adapters.Scraping.csproj", "Adapters.Scraper/"]
COPY ["Adapters.Messaging/Adapters.Messaging.csproj", "Adapters.Messaging/"]
COPY ["Contracts/Rastreador.Contracts.csproj", "Contracts/"]
RUN dotnet restore "./Bootstrap/Bootstrap.csproj"
COPY . .
WORKDIR "/src/Bootstrap"
RUN dotnet build "./Bootstrap.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase é usada para publicar o projeto de serviço a ser copiado para a fase final
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Bootstrap.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase é usada na produção ou quando executada no VS no modo normal (padrão quando não está usando a configuração de Depuração)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bootstrap.dll"]