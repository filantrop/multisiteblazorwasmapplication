#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ApplicationPublic/Client/Public.Client.csproj", "ApplicationPublic/Client/"]
COPY ["ApplicationShared/Application.Shared.csproj", "ApplicationShared/"]
RUN dotnet restore "ApplicationPublic/Client/Public.Client.csproj"
COPY . .

# Set AppEnvironment based on choosen Environment in docker-compose.yml
ARG APPENVIRONMENT
COPY ["ApplicationPublic/Client/AppEnvironments/AppEnvironment$APPENVIRONMENT.txt", "ApplicationPublic/Client/Domain/AppEnvironment.cs"]

WORKDIR "/src/ApplicationPublic/Client"
RUN dotnet build "Public.Client.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Public.Client.csproj" -c Release -o /app/publish

# We then get the base image for Nginx and set the 
# work directory 
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "Admin.Client.dll"]

COPY --from=publish /app/publish /usr/local/webapp/nginx/html
COPY ApplicationPublic/Client/nginx.conf /etc/nginx/nginx.conf


# Add envrionment file to use from blazorclient
ARG APPENVIRONMENT
#RUN echo "AppEnvironment='$APPENVIRONMENT'" > /usr/local/webapp/nginx/html/wwwroot/generatedscripts/environment.js

# Remove all environment appsettings except choosen environment
WORKDIR /usr/local/webapp/nginx/html/wwwroot/

#RUN apk add perl
#RUN ls|perl -ne "print if /^appsettings\.(?!$APPENVIRONMENT).+?\.json/"| xargs rm

