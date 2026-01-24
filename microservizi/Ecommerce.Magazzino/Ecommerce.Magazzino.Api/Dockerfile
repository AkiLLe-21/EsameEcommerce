# Usa l'immagine base per l'esecuzione
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Usa l'immagine SDK per la compilazione
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
# Argomenti per le credenziali NuGet (passati dalla GitHub Action)
ARG NUGET_USERNAME
ARG NUGET_PASSWORD
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 1. Copia il file di configurazione NuGet per l'autenticazione
COPY ["NuGet.config", "."]

# 2. Copia i file .csproj di TUTTI i progetti della solution
COPY ["Ecommerce.Magazzino.Api/Ecommerce.Magazzino.Api.csproj", "Ecommerce.Magazzino.Api/"]
COPY ["Ecommerce.Magazzino.Business/Ecommerce.Magazzino.Business.csproj", "Ecommerce.Magazzino.Business/"]
COPY ["Ecommerce.Magazzino.ClientHttp/Ecommerce.Magazzino.ClientHttp.csproj", "Ecommerce.Magazzino.ClientHttp/"]
COPY ["Ecommerce.Magazzino.Repository/Ecommerce.Magazzino.Repository.csproj", "Ecommerce.Magazzino.Repository/"]
COPY ["Ecommerce.Magazzino.Shared/Ecommerce.Magazzino.Shared.csproj", "Ecommerce.Magazzino.Shared/"]

# 3. Esegui il restore delle dipendenze
RUN dotnet restore "./Ecommerce.Magazzino.Api/Ecommerce.Magazzino.Api.csproj"

# 4. Copia tutto il resto del codice sorgente
COPY . .
WORKDIR "/src/Ecommerce.Magazzino.Api"
RUN dotnet build "./Ecommerce.Magazzino.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 5. Pubblica l'applicazione
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ecommerce.Magazzino.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 6. Costruisci l'immagine finale
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Magazzino.Api.dll"]