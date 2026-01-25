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
COPY ["NuGet.config", "."]
COPY ["Ecommerce.Ordini.Api/Ecommerce.Ordini.Api.csproj", "Ecommerce.Ordini.Api/"]
COPY ["Ecommerce.Ordini.Business/Ecommerce.Ordini.Business.csproj", "Ecommerce.Ordini.Business/"]
COPY ["Ecommerce.Ordini.ClientHttp/Ecommerce.Ordini.ClientHttp.csproj", "Ecommerce.Ordini.ClientHttp/"]
COPY ["Ecommerce.Ordini.Repository/Ecommerce.Ordini.Repository.csproj", "Ecommerce.Ordini.Repository/"]
COPY ["Ecommerce.Ordini.Shared/Ecommerce.Ordini.Shared.csproj", "Ecommerce.Ordini.Shared/"]

RUN dotnet restore "./Ecommerce.Ordini.Api/Ecommerce.Ordini.Api.csproj"

COPY . .
WORKDIR "/src/Ecommerce.Ordini.Api"
RUN dotnet build "./Ecommerce.Ordini.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build --no-restore

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ecommerce.Ordini.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ecommerce.Ordini.Api.dll"]