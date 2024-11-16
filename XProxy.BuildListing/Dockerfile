FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app
 
COPY . ./

RUN dotnet publish -c Release -o out
 
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "/app/XProxy.BuildListing.dll"]