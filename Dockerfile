# Stage 1: Build the ASP.NET Core backend
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore NuGet dependencies (cached layer)
COPY CspmEngine.csproj ./
RUN dotnet restore

# Copy all source files and web assets
COPY . ./

# Build and publish release binaries
RUN dotnet publish CspmEngine.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Production Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy compiled binaries and wwwroot assets from the build stage
COPY --from=build /app/publish .

# Create the persistent data directory inside the container
RUN mkdir -p /app/data

# Default .NET 8 container HTTP port
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

ENTRYPOINT ["dotnet", "CspmEngine.dll"]