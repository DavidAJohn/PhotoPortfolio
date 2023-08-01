
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
RUN apt-get update -y
RUN apt-get install -y python3
RUN dotnet workload install wasm-tools
WORKDIR /src
COPY ["Server/PhotoPortfolio.Server.csproj", "Server/"]
COPY ["Client/PhotoPortfolio.Client.csproj", "Client/"]
COPY ["Shared/PhotoPortfolio.Shared.csproj", "Shared/"]
RUN dotnet restore "Server/PhotoPortfolio.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "PhotoPortfolio.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PhotoPortfolio.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Execute this command from a terminal inside the Server project to create the certificate: 
# dotnet dev-certs https -ep cert.pfx -p Test1234!
COPY ["./Server/cert.pfx", "/https/cert.pfx"]
ENTRYPOINT ["dotnet", "PhotoPortfolio.Server.dll"]