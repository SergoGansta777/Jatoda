FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["JatodaBackendApi/JatodaBackendApi.csproj", "JatodaBackendApi/"]
RUN dotnet restore "JatodaBackendApi/JatodaBackendApi.csproj"
COPY . .
WORKDIR "/src/JatodaBackendApi"
RUN dotnet build "JatodaBackendApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "JatodaBackendApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "JatodaBackendApi.dll"]
