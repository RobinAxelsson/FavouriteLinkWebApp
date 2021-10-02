FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 5000

ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["src/LinkAsp/FavouriteLinkWebApp.csproj", "src/LinkAsp/"]
RUN dotnet restore "src\LinkAsp\FavouriteLinkWebApp.csproj"
COPY . .
WORKDIR "/src/src/LinkAsp"
RUN dotnet build "FavouriteLinkWebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FavouriteLinkWebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FavouriteLinkWebApp.dll"]
