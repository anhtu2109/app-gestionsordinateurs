# Construire depuis la racine de la solution.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY GestionOrdinateurs.API/GestionOrdinateurs.API.csproj GestionOrdinateurs.API/
COPY GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj GestionOrdinateurs.Shared/
RUN dotnet restore GestionOrdinateurs.API/GestionOrdinateurs.API.csproj
COPY GestionOrdinateurs.API/ GestionOrdinateurs.API/
COPY GestionOrdinateurs.Shared/ GestionOrdinateurs.Shared/
RUN dotnet publish GestionOrdinateurs.API/GestionOrdinateurs.API.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish/ ./
# Docker initialise le volume neuf avec les donnees du TP5.
COPY --from=build /app/publish/parcinfo.db ./data/parcinfo.db
ENV URLS=http://+:5256
EXPOSE 5256
ENTRYPOINT ["dotnet", "GestionOrdinateurs.API.dll"]
