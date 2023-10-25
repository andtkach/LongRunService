FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

COPY . .

RUN dotnet restore ./src/LongRunService/LongRunService.csproj

# Build and publish a release

RUN dotnet publish ./src/LongRunService/LongRunService.csproj -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "LongRunService.dll" ]