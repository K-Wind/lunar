#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 5672

COPY ["Scripts/entrypoint.sh", "/app"]

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Business.csproj", "Business/"]
RUN dotnet restore "Business/Business.csproj"

WORKDIR "/src/Business"
COPY . .

RUN dotnet build "Business.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Business.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./entrypoint.sh"]
#ENTRYPOINT ["dotnet", "Business.dll"]