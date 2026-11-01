FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY GameStore.Api.csproj .
RUN dotnet restore "GameStore.Api.csproj"

COPY . .
RUN dotnet publish "GameStore.Api.csproj" \
    -c Release \
    -o /published \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /published .

RUN mkdir -p /data && chown "$APP_UID" /data

ENV ASPNETCORE_HTTP_PORTS=8080

USER $APP_UID

EXPOSE 8080

ENTRYPOINT ["dotnet", "GameStore.Api.dll"]