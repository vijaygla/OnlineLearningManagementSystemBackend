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

## Notification System Architecture
The project uses an asynchronous, event-driven architecture to handle system-wide notifications without blocking core business logic.

### Technologies & Dependencies:
- **MassTransit.RabbitMQ**: Distributed application framework used to manage message bus communication.
- **RabbitMQ**: The message broker that handles the queuing and delivery of events.
- **MailKit & MimeKit**: Robust libraries used for formatting and sending HTML emails via SMTP.
- **Swashbuckle.AspNetCore**: Provides Swagger UI and API documentation for service monitoring.
- **Mailhog**: A developer tool that acts as a local SMTP server and web-based email inbox for testing.

### Event Flow (Step-by-Step):
1.  **Trigger**: A domain event occurs in a microservice (e.g., `EnrollmentService` completes a new student enrollment).
2.  **Publish**: The source service publishes a `Shared.Contracts.Events.EnrollmentCreatedEvent` to the RabbitMQ exchange.
3.  **Transport**: RabbitMQ routes the message to the `enrollment-created-queue` based on configured bindings.
4.  **Consume**: The `NotificationService` (running as a background consumer) detects the message and triggers the `EnrollmentCreatedConsumer`.
5.  **Process**: The consumer extracts student details and course information, then constructs a personalized HTML email.
6.  **Delivery**: The `EmailService` connects to the SMTP server (Mailhog in dev) and delivers the message.
7.  **Verification**: The process is logged for monitoring, and the message is acknowledged in the queue upon successful delivery.

### Monitoring & Local Setup:
- **Health Check**: Verify the service status at `http://localhost:8090/health`.
- **RabbitMQ Management**: Monitor queues at [http://localhost:15672](http://localhost:15672) (guest/guest).
- **Mailhog Inbox**: View sent emails at [http://localhost:8025](http://localhost:8025).
- **Service Port**: Locally runs on `8090` to avoid conflicts with other services.

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


## API Gateway & Infrastructure

| Service / Tool | External Port | Internal Port | URL / Dashboard |
| --- | ---: | ---: | --- |
| RabbitMQ | 15672 | 15672 | [Dashboard](http://localhost:15672) (guest/guest) |
| MinIO Storage | 9001 | 9001 | [Console](http://localhost:9001) (minioadmin/minioadmin) |
| MailHog | 8025 | 8025 | [Email Inbox](http://localhost:8025) |

---

## Microservices Connectivity & Swagger Map

| Service Name | Gateway Route | Ext. Port (PC) |
| --- | --- | ---: |
| API Gateway | `/` | 5000 | 8000 |
| Identity | `/api/auth` | 5001 | 8001 |
| Category | `/api/categories` | 5002 | 8002 |
| Course | `/api/courses` | 5003 | 8003 |
| Content | `/api/lessons` | 5004 | 8004 |
| Enrollment | `/api/enrollment` | 5005 | 8005 |
| Progress | `/api/progress` | 5006 | 8006 |
| Assessment | `/api/assessment` | 5007 | 8007 |
| Certificate | `/api/certificate` | 5008 | 8008 |
| Review | `/api/review` | 5009 | 8009 |
| Notification | `/api/notification` | 5010 | 8010 |
| User | `/api/user` | 5011 | 8011 |
| Media | `/api/media` | 5012 | 8012 |
| Payment | `/api/payment` | 5013 | 8013 |
| Search | `/api/search` | 5014 | 8014 |
| Discussion | `/api/discussion` | 5015 | 8015 |

## Architecture Summary

Here is a summary of the project architecture and what I understand:

- Architecture: A modular, microservices-oriented backend built with .NET 10 and ASP.NET Core Web API.
- Gateway: Central entry point routed via YARP (ApiGateway).
- Microservices: 12 distinct domain services (Identity, Category, Course, Content, Enrollment, Progress, Assessment, Review, Notification, Certificate, Media, and User).
- Additional routed services shown in the gateway map include Payment, Search, and Discussion.
- Design Pattern: Each microservice strictly adheres to Clean Architecture patterns, split into:
  - API (Controllers, Program setup)
  - Application (Business logic, DTOs, Interfaces, Services)
  - Domain (Entities, core domain logic)
  - Infrastructure (Entity Framework Core Data context, Repositories)
- Shared Context: A SharedKernel project is used to house shared Base classes, Common utilities, Contracts (for messaging), Enums, Interfaces, and ValueObjects.
- Event-Driven Messaging: The architecture employs MassTransit and RabbitMQ for asynchronous system-wide events (e.g., triggering email notifications via NotificationService when enrollments happen).
- Deployment: Local and containerized development setups using Docker Compose with predefined routing and port configurations.

## Docker Command

| Task | Command |
| --- | --- |
| Start everything (First time) | `docker-compose up -d --build` |
| Start after code changes | `docker-compose up -d --build --force-recreate` |
| Stop everything | `docker-compose down` |
| Check service status | `docker-compose ps` |
| View Media Service logs | `docker-compose logs -f mediaservice` |
| Remove all data (Clean slate) | `docker-compose down -v` (Caution: Deletes DB & Files) |

---
