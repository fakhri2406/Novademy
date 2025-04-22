# === Build Stage ===
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy solution and project files
COPY Novademy.sln ./
COPY Novademy.API/Novademy.API.csproj Novademy.API/
COPY Novademy.Application/Novademy.Application.csproj Novademy.Application/
COPY Novademy.Contracts/Novademy.Contracts.csproj Novademy.Contracts/
COPY Novademy.Contracts/Novademy.Contracts.csproj Novademy.UnitTests/

# Restore only once
RUN dotnet restore

# Copy all project contents
COPY . ./

# Publish only the API project
WORKDIR /app/Novademy.API
RUN dotnet publish -c Release -o /app/publish

# === Runtime Stage ===
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "Novademy.API.dll"]