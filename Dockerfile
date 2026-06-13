FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY denkraum.sln ./
COPY src/denkraum.api/Denkraum.Api.csproj src/denkraum.api/
COPY src/denkraum.application/Denkraum.Application.csproj src/denkraum.application/
COPY src/denkraum.domain/Denkraum.Domain.csproj src/denkraum.domain/
COPY src/denkraum.infrastructure/Denkraum.Infrastructure.csproj src/denkraum.infrastructure/
RUN dotnet restore
COPY . .
RUN dotnet publish src/denkraum.api/Denkraum.Api.csproj -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Denkraum.Api.dll"]
