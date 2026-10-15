#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
# listen on all interfaces inside the container (appsettings.json has localhost)
ENV ServiceRegistry__Host=http://+:7777
EXPOSE 7777

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Service.Registry.Host/Service.Registry.Host.csproj", "Service.Registry.Host/"]
COPY ["Service.Registry.Impl/Service.Registry.Impl.csproj", "Service.Registry.Impl/"]
COPY ["Service.Registry/Service.Registry.csproj", "Service.Registry/"]
RUN dotnet restore "Service.Registry.Host/Service.Registry.Host.csproj"
COPY . .
WORKDIR "/src/Service.Registry.Host"
RUN dotnet build "Service.Registry.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Service.Registry.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Service.Registry.Host.dll"]
