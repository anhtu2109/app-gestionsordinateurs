FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY GestionOrdinateurs.Client/GestionOrdinateurs.Client.csproj GestionOrdinateurs.Client/
COPY GestionOrdinateurs.Shared/GestionOrdinateurs.Shared.csproj GestionOrdinateurs.Shared/
RUN dotnet restore GestionOrdinateurs.Client/GestionOrdinateurs.Client.csproj
COPY GestionOrdinateurs.Client/ GestionOrdinateurs.Client/
COPY GestionOrdinateurs.Shared/ GestionOrdinateurs.Shared/
RUN dotnet publish GestionOrdinateurs.Client/GestionOrdinateurs.Client.csproj -c Release --no-restore -o /app/publish

FROM nginx:stable-alpine AS runtime
COPY GestionOrdinateurs.Client/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/publish/wwwroot/ /usr/share/nginx/html/
EXPOSE 5000
