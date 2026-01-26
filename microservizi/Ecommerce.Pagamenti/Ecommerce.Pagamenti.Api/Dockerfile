FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG NUGET_USERNAME
ARG NUGET_TOKEN
ARG BUILD_CONFIGURATION=Release
ENV NUGET_USERNAME=$NUGET_USERNAME
ENV NUGET_TOKEN=$NUGET_TOKEN

WORKDIR /src

# 1. Copia il config che abbiamo appena creato (fondamentale!)
COPY ["NuGet.config", "."]

# 2. Copia i file di progetto (i path sono relativi al Context, cioè la cartella Pagamenti)
COPY ["Ecommerce.Pagamenti.Api/Ecommerce.Pagamenti.Api.csproj", "Ecommerce.Pagamenti.Api/"]
COPY ["Ecommerce.Pagamenti.Business/Ecommerce.Pagamenti.Business.csproj", "Ecommerce.Pagamenti.Business/"]
COPY ["Ecommerce.Pagamenti.ClientHttp/Ecommerce.Pagamenti.ClientHttp.csproj", "Ecommerce.Pagamenti.ClientHttp/"]
COPY ["Ecommerce.Pagamenti.Repository/Ecommerce.Pagamenti.Repository.csproj", "Ecommerce.Pagamenti.Repository/"]
COPY ["Ecommerce.Pagamenti.Shared/Ecommerce.Pagamenti.Shared.csproj", "Ecommerce.Pagamenti.Shared/"]

# 3. Restore usando le credenziali (scaricherà Utility.Kafka da GitHub Packages)
RUN dotnet restore "./Ecommerce.Pagamenti.Api/Ecommerce.Pagamenti.Api.csproj"


COPY . .
WORKDIR "/src/Ecommerce.Pagamenti.Api"
RUN dotnet build "./Ecommerce.Pagamenti.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ecommerce.Pagamenti.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Pagamenti.Api.dll"]