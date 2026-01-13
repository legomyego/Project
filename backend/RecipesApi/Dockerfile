# Dockerfile for .NET 9 Backend API
# Multi-stage build for optimized production image

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
# This layer is cached unless dependencies change
COPY RecipesApi.csproj .
RUN dotnet restore

# Copy source code and build
COPY . .
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
# Use smaller runtime image for production
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy published app from publish stage
COPY --from=publish /app/publish .

# Expose port 5010 (API port)
EXPOSE 5010

# Set environment to Production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5010

# Run the application
ENTRYPOINT ["dotnet", "RecipesApi.dll"]
