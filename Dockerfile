FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY BitCoder.Core/BitCoder.Core.csproj BitCoder.Core/
COPY BitCoder.Web/BitCoder.Web.csproj BitCoder.Web/
RUN dotnet restore BitCoder.Web/BitCoder.Web.csproj
COPY BitCoder.Core/ BitCoder.Core/
COPY BitCoder.Web/ BitCoder.Web/
RUN dotnet publish BitCoder.Web/BitCoder.Web.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app ./
# Хостинг передаёт порт в переменной PORT (на Render по умолчанию 10000).
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} exec dotnet BitCoder.Web.dll"]
