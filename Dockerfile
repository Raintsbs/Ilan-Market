# Repo kökünden build (Railway Root Directory boşsa)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY AdvertisementApp/ ./AdvertisementApp/
WORKDIR /src/AdvertisementApp
RUN dotnet restore AdvertisementApp.API/AdvertisementApp.API.csproj
RUN dotnet publish AdvertisementApp.API/AdvertisementApp.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
RUN mkdir -p /app/data
RUN mkdir -p /app/data /app/scripts/data
COPY --from=build /app/publish .
COPY AdvertisementApp/scripts/data/turkey-locations.json ./scripts/data/turkey-locations.json
COPY AdvertisementApp/scripts/data/seed-advertisements.json ./scripts/data/seed-advertisements.json
ENV ASPNETCORE_ENVIRONMENT=Staging
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV Database__Provider=Sqlite
ENV ConnectionStrings__DefaultConnection=Data Source=/app/data/ilanmarket.db
ENV Seed__RunOnStartup=true
ENV Seed__RunCategoryCatalog=true
ENV Seed__RunLegalPages=true
ENV Seed__ImportLocalAds=true
EXPOSE 8080
ENTRYPOINT ["dotnet", "AdvertisementApp.API.dll"]
