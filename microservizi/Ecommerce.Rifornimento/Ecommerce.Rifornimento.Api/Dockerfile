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

# 2. Copia i file di progetto (i path sono relativi al Context, cioè la cartella Rifornimento)
COPY ["Ecommerce.Rifornimento.Api/Ecommerce.Rifornimento.Api.csproj", "Ecommerce.Rifornimento.Api/"]
COPY ["Ecommerce.Rifornimento.Business/Ecommerce.Rifornimento.Business.csproj", "Ecommerce.Rifornimento.Business/"]
COPY ["Ecommerce.Rifornimento.ClientHttp/Ecommerce.Rifornimento.ClientHttp.csproj", "Ecommerce.Rifornimento.ClientHttp/"]
COPY ["Ecommerce.Rifornimento.Repository/Ecommerce.Rifornimento.Repository.csproj", "Ecommerce.Rifornimento.Repository/"]
COPY ["Ecommerce.Rifornimento.Shared/Ecommerce.Rifornimento.Shared.csproj", "Ecommerce.Rifornimento.Shared/"]

# 3. Restore usando le credenziali (scaricherà Utility.Kafka da GitHub Packages)
RUN dotnet restore "./Ecommerce.Rifornimento.Api/Ecommerce.Rifornimento.Api.csproj"


COPY . .
WORKDIR "/src/Ecommerce.Rifornimento.Api"
RUN dotnet build "./Ecommerce.Rifornimento.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ecommerce.Rifornimento.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Rifornimento.Api.dll"]