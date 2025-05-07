FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DatabaseConsumer/DatabaseConsumer.csproj", "DatabaseConsumer/"]
COPY ["ConsumerClient/ConsumerClient.csproj", "ConsumerClient/"]
COPY ["Domain/Domain.csproj", "Domain/"]
RUN dotnet restore "DatabaseConsumer/DatabaseConsumer.csproj"
COPY . .
WORKDIR "/src/DatabaseConsumer"
RUN dotnet build "DatabaseConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DatabaseConsumer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DatabaseConsumer.dll"]
