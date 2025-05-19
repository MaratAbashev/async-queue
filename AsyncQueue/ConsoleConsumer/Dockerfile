FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ConsoleConsumer/ConsoleConsumer.csproj", "ConsoleConsumer/"]
COPY ["ConsumerClient/ConsumerClient.csproj", "ConsumerClient/"]
COPY ["Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "ConsoleConsumer/ConsoleConsumer.csproj"
COPY . .
WORKDIR "/src/ConsoleConsumer"
RUN dotnet build "ConsoleConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "ConsoleConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsoleConsumer.dll"]
