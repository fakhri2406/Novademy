# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY Novademy.API/*.csproj Novademy.API/
COPY Novademy.Application/*.csproj Novademy.Application/
COPY Novademy.Contracts/*.csproj Novademy.Contracts/
RUN dotnet restore Novademy.API/Novademy.API.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish Novademy.API/Novademy.API.csproj -c Release -o out

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "Novademy.API.dll"]