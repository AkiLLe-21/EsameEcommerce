# Usa l'immagine base per l'esecuzione
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Usa l'immagine SDK per la compilazione
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Argomenti passati dalla GitHub Action
ARG NUGET_USERNAME
ARG NUGET_PASSWORD
ARG BUILD_CONFIGURATION=Release

# IMPORTANTE: Mappa gli ARG in ENV. 
# Questo assicura che NuGet.config possa leggerli in QUALSIASI comando successivo.
ENV NUGET_USERNAME=$NUGET_USERNAME
ENV NUGET_PASSWORD=$NUGET_PASSWORD

WORKDIR /src

# Copia il file di configurazione
COPY ["NuGet.config", "."]

# Copia i file di progetto
COPY ["Ecommerce.Magazzino.Api/Ecommerce.Magazzino.Api.csproj", "Ecommerce.Magazzino.Api/"]
COPY ["Ecommerce.Magazzino.Business/Ecommerce.Magazzino.Business.csproj", "Ecommerce.Magazzino.Business/"]
COPY ["Ecommerce.Magazzino.ClientHttp/Ecommerce.Magazzino.ClientHttp.csproj", "Ecommerce.Magazzino.ClientHttp/"]
COPY ["Ecommerce.Magazzino.Repository/Ecommerce.Magazzino.Repository.csproj", "Ecommerce.Magazzino.Repository/"]
COPY ["Ecommerce.Magazzino.Shared/Ecommerce.Magazzino.Shared.csproj", "Ecommerce.Magazzino.Shared/"]

# Esegue il restore (Qui usa le credenziali)
RUN dotnet restore "./Ecommerce.Magazzino.Api/Ecommerce.Magazzino.Api.csproj"

# Copia tutto il resto del codice
COPY . .
WORKDIR "/src/Ecommerce.Magazzino.Api"

# CRUCIALE: Aggiunto --no-restore
# Dice a dotnet: "Non cercare di scaricare pacchetti, usa quelli che hai già".
RUN dotnet build "./Ecommerce.Magazzino.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# CRUCIALE: Aggiunto --no-restore anche qui
RUN dotnet publish "./Ecommerce.Magazzino.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

# Costruisci l'immagine finale
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Magazzino.Api.dll"]