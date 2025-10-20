# Multi-stage build for API + Web (Razor)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./ECommerce.sln ./
COPY ./src/ECommerce.Api/ECommerce.Api.csproj ./src/ECommerce.Api/
COPY ./src/ECommerce.Web/ECommerce.Web.csproj ./src/ECommerce.Web/
COPY ./src/ECommerce.Core/ECommerce.Core.csproj ./src/ECommerce.Core/
COPY ./src/ECommerce.Infrastructure/ECommerce.Infrastructure.csproj ./src/ECommerce.Infrastructure/
RUN dotnet restore ECommerce.sln
COPY . .
RUN dotnet publish src/ECommerce.Api/ECommerce.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ECommerce.Api.dll"]
