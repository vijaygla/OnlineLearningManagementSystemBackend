FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the entire microservices/MediaService folder
COPY microservices/MediaService/ ./MediaService/
# Also need the SharedKernel if it's referenced (checking earlier shows no direct reference, but good to be safe if it's in the solution)
# Actually, looking at the csproj, it doesn't reference SharedKernel yet, but I'll copy the structure needed.

WORKDIR /src/MediaService/MediaService.API
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MediaService.API.dll"]
