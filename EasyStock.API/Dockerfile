# Use official .NET 6 SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy everything
COPY . . 

# Restore dependencies, specifying the solution path
RUN dotnet restore EasyStock.API/EasyStock.API.sln

# Build and publish app
RUN dotnet publish EasyStock.API/EasyStock.API.sln -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

# Copy published app from build stage
COPY --from=build-env /app/out . 

# Start the app
ENTRYPOINT ["dotnet", "EasyStock.API.dll"]