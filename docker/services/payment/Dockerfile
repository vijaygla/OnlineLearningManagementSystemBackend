FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY microservices/PaymentService/ microservices/PaymentService/
COPY shared/SharedKernel/ shared/SharedKernel/
WORKDIR /src/microservices/PaymentService/PaymentService.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.API.dll"]
