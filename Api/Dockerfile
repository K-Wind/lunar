#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

#EXPOSE 5672

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ENV ASPNETCORE_URLS http://*:44319
EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_ENVIRONMENT=Development

WORKDIR /src
COPY ["Api.csproj", "Api/"]
RUN dotnet restore "Api/Api.csproj"

WORKDIR "/src/Api"
COPY . .

RUN dotnet build "Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]