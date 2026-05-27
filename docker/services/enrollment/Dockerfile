FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY microservices/EnrollmentService/ ./
WORKDIR /src/EnrollmentService.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EnrollmentService.API.dll"]
