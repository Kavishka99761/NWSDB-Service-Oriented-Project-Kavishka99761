# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["NWSDB_System.sln", "."]
COPY ["src/NWSDB.WebApi/NWSDB.WebApi.csproj", "src/NWSDB.WebApi/"]
COPY ["src/NWSDB.Core/NWSDB.Core.csproj", "src/NWSDB.Core/"]
COPY ["src/NWSDB.Data/NWSDB.Data.csproj", "src/NWSDB.Data/"]
COPY ["src/NWSDB.Services/NWSDB.Services.csproj", "src/NWSDB.Services/"]

# Restore dependencies
RUN dotnet restore "NWSDB_System.sln"

# Copy entire source
COPY . .

# Build WebApi
WORKDIR /src/src/NWSDB.WebApi
RUN dotnet build "NWSDB.WebApi.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "NWSDB.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "NWSDB.WebApi.dll"]
