FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TelegramConsumer/TelegramConsumer.csproj", "TelegramConsumer/"]
COPY ["ConsumerClient/ConsumerClient.csproj", "ConsumerClient/"]
COPY ["Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "TelegramConsumer/TelegramConsumer.csproj"
COPY . .
WORKDIR "/src/TelegramConsumer"
RUN dotnet build "TelegramConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TelegramConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramConsumer.dll"]
