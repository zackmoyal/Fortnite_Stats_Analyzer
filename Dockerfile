# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY *.sln .
COPY FortniteStatsAnalyzer/*.csproj ./FortniteStatsAnalyzer/
RUN dotnet restore

COPY FortniteStatsAnalyzer/. ./FortniteStatsAnalyzer/
WORKDIR /app/FortniteStatsAnalyzer
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/FortniteStatsAnalyzer/out ./
ENTRYPOINT ["dotnet", "FortniteStatsAnalyzer.dll"]
