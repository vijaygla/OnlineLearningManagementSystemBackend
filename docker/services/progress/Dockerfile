FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY microservices/ProgressService/ microservices/ProgressService/
COPY shared/SharedKernel/ shared/SharedKernel/
WORKDIR /src/microservices/ProgressService/ProgressService.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ProgressService.API.dll"]
