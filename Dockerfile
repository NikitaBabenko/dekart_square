# --- Tailwind CSS build stage --------------------------------------------
FROM node:20-alpine AS css
WORKDIR /web
COPY src/DecisionHelper.Web/package.json src/DecisionHelper.Web/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY src/DecisionHelper.Web/tailwind.config.js ./
COPY src/DecisionHelper.Web/Styles ./Styles
COPY src/DecisionHelper.Web/Components ./Components
COPY src/DecisionHelper.Web/wwwroot ./wwwroot
RUN npm run build:css

# --- .NET publish stage --------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY src/DecisionHelper.Core/DecisionHelper.Core.csproj src/DecisionHelper.Core/
COPY src/DecisionHelper.Infrastructure/DecisionHelper.Infrastructure.csproj src/DecisionHelper.Infrastructure/
COPY src/DecisionHelper.Web/DecisionHelper.Web.csproj src/DecisionHelper.Web/
RUN dotnet restore src/DecisionHelper.Web/DecisionHelper.Web.csproj

COPY . .
# Inject the Tailwind output produced by the css stage.
COPY --from=css /web/wwwroot/app.css src/DecisionHelper.Web/wwwroot/app.css

RUN dotnet publish src/DecisionHelper.Web/DecisionHelper.Web.csproj \
    -c Release -o /app/publish \
    --no-restore /p:UseAppHost=false

# --- Runtime stage -------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DecisionHelper.Web.dll"]
