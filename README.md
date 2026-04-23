# Online Learning Management System Backend

A modular backend for an online learning platform built with ASP.NET Core and a microservices-oriented structure. The repository includes an API Gateway, multiple domain-focused services, shared building blocks, and Docker support for containerized development.

## Overview

This backend is designed to support the core workflows of a learning management system, including:

- Authentication and authorization with JWT
- Course category management
- Course creation and approval workflows
- Lesson and content management
- Enrollment, progress tracking, assessments, reviews, certificates, notifications, and media services

The solution uses a service-per-domain layout so features can evolve independently while still sharing common contracts, utilities, and kernel abstractions.

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- YARP Reverse Proxy
- Entity Framework Core
- Azure SQL
- Docker Compose
- Swagger / OpenAPI
- JWT Bearer Authentication

## Repository Structure

```text
.
|-- gateway/
|   `-- ApiGateway/
|-- microservices/
|   |-- IdentityService/
|   |-- CategoryService/
|   |-- CourseService/
|   |-- ContentService/
|   |-- EnrollmentService/
|   |-- ProgressService/
|   |-- AssessmentService/
|   |-- ReviewService/
|   |-- NotificationService/
|   |-- CertificateService/
|   |-- MediaService/
|   `-- UserService/
|-- shared/
|   |-- Common/
|   |-- Contracts/
|   `-- SharedKernel/
`-- docker/
```

## Services

### Core services currently wired for active development

- `IdentityService`  
  Handles registration, login, role-based access, and JWT token generation.

- `CategoryService`  
  Manages course categories.

- `CourseService`  
  Supports course creation and course approval workflows.

- `ContentService`  
  Manages lessons and learning content metadata.

- `ApiGateway`  
  Entry point for routing requests through YARP reverse proxy.

### Additional services present in the repo

- `EnrollmentService`
- `ProgressService`
- `AssessmentService`
- `ReviewService`
- `NotificationService`
- `CertificateService`
- `MediaService`
- `UserService`

## Prerequisites

Before running the project, make sure you have:

- .NET 10 SDK installed
- Docker Desktop installed
- Access to an Azure SQL database
- A valid connection string and JWT secret configured locally

## Configuration

Local secrets should not be committed. This repo is now configured to ignore files such as:

- `docker/.env`
- `appsettings.json`
- `appsettings.Development.json`

Typical local configuration includes:

- `AZURE_SQL_CONNECTION`
- `JWT_SECRET`

Create your own local environment values before running the services.

## Running the Project Locally

### 1. Restore dependencies

```powershell
dotnet restore
```

### 2. Run individual services

Use these commands from the repository root:

```powershell
dotnet run --project microservices/IdentityService/IdentityService.API/IdentityService.API.csproj
dotnet run --project microservices/CategoryService/CategoryService.API/CategoryService.API.csproj
dotnet run --project microservices/CourseService/CourseService.API/CourseService.API.csproj
dotnet run --project microservices/ContentService/ContentService.API/ContentService.API.csproj
dotnet run --project gateway/ApiGateway/ApiGateway.csproj
```

### Default local HTTP ports

- `ApiGateway`: `http://localhost:5031`
- `IdentityService`: `http://127.0.0.1:8081`
- `CategoryService`: `http://127.0.0.1:8082`
- `CourseService`: `http://127.0.0.1:8083`
- `ContentService`: `http://127.0.0.1:8084`

### Swagger

Most API projects expose Swagger in development. After starting a service, open:

```text
http://<host>:<port>/swagger
```

Examples:

- `http://127.0.0.1:8081/swagger`
- `http://127.0.0.1:8082/swagger`
- `http://127.0.0.1:8083/swagger`
- `http://127.0.0.1:8084/swagger`

## Running with Docker

The repository includes Docker support under the `docker/` folder.

### Start the configured containers

```powershell
cd docker
docker-compose --env-file .env up --build
```
`docker-compose -f docker/docker-compose.yml up --build` \____/

### Current Docker compose exposure

The current `docker-compose.yml` exposes:

- `ApiGateway` on `http://localhost:5000`
- `IdentityService` on `http://localhost:5001`
- `CourseService` on `http://localhost:5002`

If you expand Docker usage for the other services, keep their environment variables and routing definitions aligned with the gateway configuration.

## Entity Framework Core Commands

If you are working on migrations, common commands are:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

If a project does not yet have the required EF Core packages, install the relevant dependencies in that project first.

## Common Packages Used

Examples used across the solution include:

```powershell
dotnet add package Yarp.ReverseProxy
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next
dotnet add package Swashbuckle.AspNetCore
```

## Development Notes

- The project targets `net10.0`.
- Shared abstractions live under `shared/`.
- HTTP request sample files are included in several API projects for quick endpoint testing.
- The API Gateway is intended to be the main public entry point once routing is fully configured.

## Current Status

### Implemented or actively built

- Identity management with JWT authentication
- Category management
- Course management
- Content and lesson management
- Gateway foundation with reverse proxy support

### Present in the codebase and ready for continued development

- Enrollment
- Progress tracking
- Assessments
- Reviews
- Notifications
- Certificates
- Media handling
- User service

## Recommended Next Improvements

- Add a safe `docker/.env.example` with placeholder values
- Document gateway routes explicitly in the README
- Add service-to-port mapping for every microservice
- Add database migration instructions per service
- Add architecture diagrams and request flow examples

## Contributing

1. Create a feature branch.
2. Keep secrets out of Git.
3. Use environment-specific local config files.
4. Test the affected service before opening a pull request.

## License

This project currently has no license file defined in the repository. Add one if you plan to make the project public on GitHub.
