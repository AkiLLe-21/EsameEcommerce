FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG NUGET_USERNAME
ARG NUGET_TOKEN
ARG BUILD_CONFIGURATION=Release
ENV NUGET_USERNAME=$NUGET_USERNAME
ENV NUGET_TOKEN=$NUGET_TOKEN

WORKDIR /src

# Copia il file di configurazione
COPY ["NuGet.config", "."]

# Copia i file di progetto
COPY ["Ecommerce.Magazzino.Api/Ecommerce.Magazzino.Api.csproj", "Ecommerce.Magazzino.Api/"]
COPY ["Ecommerce.Magazzino.Business/Ecommerce.Magazzino.Business.csproj", "Ecommerce.Magazzino.Business/"]
COPY ["Ecommerce.Magazzino.ClientHttp/Ecommerce.Magazzino.ClientHttp.csproj", "Ecommerce.Magazzino.ClientHttp/"]
COPY ["Ecommerce.Magazzino.Repository/Ecommerce.Magazzino.Repository.csproj", "Ecommerce.Magazzino.Repository/"]
COPY ["Ecommerce.Magazzino.Shared/Ecommerce.Magazzino.Shared.csproj", "Ecommerce.Magazzino.Shared/"]

# Restore
RUN dotnet restore "./Ecommerce.Magazzino.Api/Ecommerce.Magazzino.Api.csproj"


COPY . .
WORKDIR "/src/Ecommerce.Magazzino.Api"
RUN dotnet build "./Ecommerce.Magazzino.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ecommerce.Magazzino.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Magazzino.Api.dll"]