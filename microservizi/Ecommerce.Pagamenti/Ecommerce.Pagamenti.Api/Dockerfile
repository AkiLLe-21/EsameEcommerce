FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["microservizi/Ecommerce.Pagamenti/Ecommerce.Pagamenti.Api/Ecommerce.Pagamenti.Api.csproj", "microservizi/Ecommerce.Pagamenti/Ecommerce.Pagamenti.Api/"]
COPY ["microservizi/Ecommerce.Pagamenti/Ecommerce.Pagamenti.Business/Ecommerce.Pagamenti.Business.csproj", "microservizi/Ecommerce.Pagamenti/Ecommerce.Pagamenti.Business/"]
COPY ["microservizi/Ecommerce.Pagamenti/Ecommerce.Pagamenti.Business.Abstraction/Ecommerce.Pagamenti.Business.Abstraction.csproj", "microservizi/Ecommerce.Pagamenti/Ecommerce.Pagamenti.Business.Abstraction/"]
COPY ["microservizi/Ecommerce.Pagamenti/Ecommerce.Pagamenti.Repository/Ecommerce.Pagamenti.Repository.csproj", "microservizi/Ecommerce.Pagamenti/Ecommerce.Pagamenti.Repository/"]
COPY ["microservizi/Ecommerce.Pagamenti/Ecommerce.Pagamenti.Repository.Abstraction/Ecommerce.Pagamenti.Repository.Abstraction.csproj", "microservizi/Ecommerce.Pagamenti/Ecommerce.Pagamenti.Repository.Abstraction/"]
COPY ["librerie/Utility.Kafka/Utility.Kafka.csproj", "librerie/Utility.Kafka/"]

RUN dotnet restore "./microservizi/Ecommerce.Pagamenti/Ecommerce.Pagamenti.Api/Ecommerce.Pagamenti.Api.csproj"
COPY . .
WORKDIR "/src/microservizi/Ecommerce.Pagamenti/Ecommerce.Pagamenti.Api"
RUN dotnet build "./Ecommerce.Pagamenti.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ecommerce.Pagamenti.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Pagamenti.Api.dll"]