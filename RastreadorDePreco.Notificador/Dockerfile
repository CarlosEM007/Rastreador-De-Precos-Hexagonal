# O Notificador é um Worker, não uma API: a imagem base é a `runtime`, não a `aspnet`.
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

# Os .csproj são copiados antes do resto do código para que a camada do `restore`
# fique em cache e só seja refeita quando uma dependência mudar.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RastreadorDePreco.Notificador/Host.csproj", "RastreadorDePreco.Notificador/"]
COPY ["Notificador.Application/Application.csproj", "Notificador.Application/"]
COPY ["Notificador.Adapters.Messaging/Adapters.Messaging.csproj", "Notificador.Adapters.Messaging/"]
COPY ["Notificador.Adapters.Email/Adapters.Email.csproj", "Notificador.Adapters.Email/"]
COPY ["Rastreador.Queues/Rastreador.Queues.csproj", "Rastreador.Queues/"]
COPY ["Rastreador.Health/Rastreador.Health.csproj", "Rastreador.Health/"]
RUN dotnet restore "./RastreadorDePreco.Notificador/Host.csproj"
COPY . .
WORKDIR "/src/RastreadorDePreco.Notificador"
RUN dotnet build "./Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notificador.Host.dll"]
