FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY AuthentificationApi/AuthentificationApi.csproj AuthentificationApi/
COPY GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj GestionOrdinateurs.Shared/
RUN dotnet restore AuthentificationApi/AuthentificationApi.csproj
COPY AuthentificationApi/ AuthentificationApi/
COPY GestionOrdinateurs.Shared/ GestionOrdinateurs.Shared/
RUN dotnet publish AuthentificationApi/AuthentificationApi.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish/ ./
ENV URLS=http://+:5011
EXPOSE 5011
ENTRYPOINT ["dotnet", "AuthentificationApi.dll"]
