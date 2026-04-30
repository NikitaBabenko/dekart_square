FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY DecisionHelper.sln ./
COPY src/DecisionHelper.Core/DecisionHelper.Core.csproj src/DecisionHelper.Core/
COPY src/DecisionHelper.Infrastructure/DecisionHelper.Infrastructure.csproj src/DecisionHelper.Infrastructure/
COPY src/DecisionHelper.Web/DecisionHelper.Web.csproj src/DecisionHelper.Web/
COPY tests/DecisionHelper.Tests/DecisionHelper.Tests.csproj tests/DecisionHelper.Tests/
RUN dotnet restore DecisionHelper.sln

COPY . .
RUN dotnet publish src/DecisionHelper.Web/DecisionHelper.Web.csproj \
    -c Release -o /app/publish \
    --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DecisionHelper.Web.dll"]
