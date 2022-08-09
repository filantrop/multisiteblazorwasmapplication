#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5002:80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ApplicationAdmin/Server/Admin.Server.csproj", "ApplicationAdmin/Server/"]
COPY ["ApplicationShared/Application.Shared.csproj", "ApplicationShared/"]
RUN dotnet restore "ApplicationAdmin/Server/Admin.Server.csproj"
COPY . .
WORKDIR "/src/ApplicationAdmin/Server"
RUN dotnet build "Admin.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Admin.Server.csproj" -c Release -o /app/publish

# We then get the base image for Nginx and set the 
# work directory 
#FROM nginx:alpine AS final
#WORKDIR /usr/share/nginx/html


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Admin.Server.dll"]

#COPY --from=publish /app/publish /usr/local/webapp/nginx/html
#COPY Admin/Server/nginx.conf /etc/nginx/nginx.conf