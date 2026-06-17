# Repo kökünden build (Railway Root Directory boşsa)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY AdvertisementApp/ ./AdvertisementApp/
WORKDIR /src/AdvertisementApp
RUN dotnet restore AdvertisementApp.API/AdvertisementApp.API.csproj
RUN dotnet publish AdvertisementApp.API/AdvertisementApp.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AdvertisementApp.API.dll"]
