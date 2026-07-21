FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/backend/CareerPilot.Api/CareerPilot.Api.csproj", "src/backend/CareerPilot.Api/"]
COPY ["src/backend/CareerPilot.Application/CareerPilot.Application.csproj", "src/backend/CareerPilot.Application/"]
COPY ["src/backend/CareerPilot.Domain/CareerPilot.Domain.csproj", "src/backend/CareerPilot.Domain/"]
COPY ["src/backend/CareerPilot.Infrastructure/CareerPilot.Infrastructure.csproj", "src/backend/CareerPilot.Infrastructure/"]
COPY ["src/backend/CareerPilot.Shared/CareerPilot.Shared.csproj", "src/backend/CareerPilot.Shared/"]

RUN dotnet restore "src/backend/CareerPilot.Api/CareerPilot.Api.csproj"

COPY . .
WORKDIR "/src/src/backend/CareerPilot.Api"
RUN dotnet build "CareerPilot.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CareerPilot.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CareerPilot.Api.dll"]
