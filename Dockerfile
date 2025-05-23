FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Novademy.API/Novademy.API.csproj", "Novademy.API/"]
RUN dotnet restore "Novademy.API/Novademy.API.csproj"
COPY . .
WORKDIR "/src/Novademy.API"
RUN dotnet build "Novademy.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Novademy.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Novademy.API.dll"]