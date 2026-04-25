FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["microservices/NotificationService/NotificationService.API/NotificationService.API.csproj", "microservices/NotificationService/NotificationService.API/"]
COPY ["microservices/NotificationService/NotificationService.Application/NotificationService.Application.csproj", "microservices/NotificationService/NotificationService.Application/"]
COPY ["microservices/NotificationService/NotificationService.Domain/NotificationService.Domain.csproj", "microservices/NotificationService/NotificationService.Domain/"]
COPY ["microservices/NotificationService/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "microservices/NotificationService/NotificationService.Infrastructure/"]
COPY ["shared/SharedKernel/SharedKernel.csproj", "shared/SharedKernel/"]

# Restore dependencies
RUN dotnet restore "microservices/NotificationService/NotificationService.API/NotificationService.API.csproj"

# Copy the rest of the code
COPY ["microservices/NotificationService/", "microservices/NotificationService/"]
COPY ["shared/SharedKernel/", "shared/SharedKernel/"]

WORKDIR "/src/microservices/NotificationService/NotificationService.API"
RUN dotnet publish "NotificationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "NotificationService.API.dll"]
