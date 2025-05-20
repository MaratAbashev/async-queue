# Этап сборки с полным SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем только файлы проектов для восстановления зависимостей
COPY ["BotConsumer/BotConsumer.csproj", "BotConsumer/"]
COPY ["ConsumerClient/ConsumerClient.csproj", "ConsumerClient/"]
COPY ["Domain/Domain.csproj", "Domain/"]

# Восстанавливаем зависимости
RUN dotnet restore "BotConsumer/BotConsumer.csproj"

# Копируем всё остальное
COPY . .

# Сборка и публикация
WORKDIR "/src/BotConsumer"
RUN dotnet publish "BotConsumer.csproj" -c Release -o /app/publish \
    -r linux-x64 \
    --self-contained false \
    /p:UseAppHost=false

# Финальный образ с полным ASP.NET + дополнительными зависимостями
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Устанавливаем системные зависимости для Telegram.Bot
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    libc6-dev \
    libgdiplus \
    libx11-dev \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
EXPOSE 8081
USER $APP_UID
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BotConsumer.dll"]