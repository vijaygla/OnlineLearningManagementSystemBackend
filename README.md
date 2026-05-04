# Online Learning Management System Backend

A modular backend for an online learning platform built with .NET 10, ASP.NET Core Web API, YARP, Entity Framework Core, RabbitMQ, and Docker Compose.

The solution follows a microservices-oriented structure: each domain service owns its API, application logic, domain model, and infrastructure, while shared contracts and common building blocks live under `shared/`.

## Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Repository Structure](#repository-structure)
- [Architecture](#architecture)
- [Service Map](#service-map)
- [Infrastructure Map](#infrastructure-map)
- [Configuration](#configuration)
- [Run Locally](#run-locally)
- [Run with Docker](#run-with-docker)
- [Entity Framework Core](#entity-framework-core)
- [Event-Driven Flows](#event-driven-flows)
- [Development Notes](#development-notes)

## Overview

This backend supports the main workflows of an online learning management system:

- Authentication, authorization, roles, and JWT token generation
- Course categories, courses, content, and lesson metadata
- Enrollment, learner progress, assessments, reviews, certificates, and user profiles
- Media uploads through MinIO-compatible object storage
- Course search through Meilisearch
- Notifications through RabbitMQ, MassTransit, SMTP, and MailHog
- Payments through Stripe PaymentIntents and webhook handling
- Discussion threads and comments

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- YARP Reverse Proxy
- Entity Framework Core
- Azure SQL / SQL Server
- RabbitMQ and MassTransit
- Stripe.net
- MinIO
- Meilisearch
- MailKit, MimeKit, and MailHog
- Docker Compose
- Swagger / OpenAPI
- JWT Bearer Authentication

## Repository Structure

```text
.
|-- gateway/
|   `-- ApiGateway/
|-- microservices/
|   |-- AssessmentService/
|   |-- CategoryService/
|   |-- CertificateService/
|   |-- ContentService/
|   |-- CourseService/
|   |-- DiscussionService/
|   |-- EnrollmentService/
|   |-- IdentityService/
|   |-- MediaService/
|   |-- NotificationService/
|   |-- PaymentService/
|   |-- ProgressService/
|   |-- ReviewService/
|   |-- SearchService/
|   `-- UserService/
|-- shared/
|   `-- SharedKernel/
|-- docker/
|   |-- docker-compose.yml
|   |-- gateway/
|   `-- services/
`-- OnlineLearningManagementSystemBackend.slnx
```

## Architecture

### Gateway

`gateway/ApiGateway` is the public entry point. It uses YARP to route incoming requests to the correct backend service.

Gateway root:

```text
http://localhost:5000
```

### Microservice Layout

Most services follow this Clean Architecture style:

```text
ServiceName/
|-- ServiceName.API/             # Controllers, Program setup, Swagger, auth setup
|-- ServiceName.Application/     # DTOs, interfaces, business services, consumers
|-- ServiceName.Domain/          # Entities, enums, domain rules
`-- ServiceName.Infrastructure/  # EF Core DbContext, repositories, external integrations
```

### Shared Kernel

`shared/SharedKernel` contains reusable pieces shared across services:

- Base entities and auditable entities
- Common wrappers such as `ApiResponse` and `PagedResponse`
- Shared exceptions, middleware, helpers, and extensions
- Shared contracts and integration events
- Common constants, enums, interfaces, and value objects

### Messaging

Services publish and consume integration events through MassTransit and RabbitMQ. Shared event contracts are stored in:

```text
shared/SharedKernel/Contracts/Events/
```

Current shared events include:

- `UserCreatedEvent`
- `UserDeletedEvent`
- `EnrollmentCreatedEvent`
- `ProgressUpdatedEvent`
- `CourseApprovedEvent`
- `PaymentCompletedEvent`

## Service Map

| Service | Responsibility | Gateway Route | External Port | Internal Port | Swagger |
| --- | --- | --- | ---: | ---: | --- |
| API Gateway | Public routing entry point | `/` | 5000 | 8000 | N/A |
| Identity | Registration, login, roles, JWT | `/api/auth` | 5001 | 8001 | [Swagger](http://localhost:5001/swagger) |
| Category | Course categories | `/api/categories` | 5002 | 8002 | [Swagger](http://localhost:5002/swagger) |
| Course | Course creation and approval | `/api/courses` | 5003 | 8003 | [Swagger](http://localhost:5003/swagger) |
| Content | Lessons and learning content | `/api/lessons` | 5004 | 8004 | [Swagger](http://localhost:5004/swagger) |
| Enrollment | Course enrollments | `/api/enrollments`, `/api/enrollment` | 5005 | 8005 | [Swagger](http://localhost:5005/swagger) |
| Progress | Learner progress tracking | `/api/progress` | 5006 | 8006 | [Swagger](http://localhost:5006/swagger) |
| Assessment | Quizzes, questions, submissions | `/api/assessments` | 5007 | 8007 | [Swagger](http://localhost:5007/swagger) |
| Certificate | Completion certificates | `/api/certificates` | 5008 | 8008 | [Swagger](http://localhost:5008/swagger) |
| Review | Course reviews and ratings | `/api/reviews` | 5009 | 8009 | [Swagger](http://localhost:5009/swagger) |
| Notification | Email notifications and consumers | `/api/notifications` | 5010 | 8010 | [Swagger](http://localhost:5010/swagger) |
| User | User profiles and preferences | `/api/users` | 5011 | 8011 | [Swagger](http://localhost:5011/swagger) |
| Media | Media metadata and file storage | `/api/media` | 5012 | 8012 | [Swagger](http://localhost:5012/swagger) |
| Payment | Stripe payments and webhooks | `/api/payments`, `/api/webhook` | 5013 | 8013 | [Swagger](http://localhost:5013/swagger) |
| Search | Course search indexing | `/api/search` | 5014 | 8014 | [Swagger](http://localhost:5014/swagger) |
| Discussion | Threads and comments | `/api/threads` | 5015 | 8015 | [Swagger](http://localhost:5015/swagger) |

When services run inside Docker, the browser uses the external ports. The gateway forwards traffic to the internal Docker ports.

## Infrastructure Map

| Tool | Purpose | External Port | Internal Port | URL |
| --- | --- | ---: | ---: | --- |
| RabbitMQ | Message broker | 5672 | 5672 | `amqp://localhost:5672` |
| RabbitMQ Management | Queue dashboard | 15672 | 15672 | [http://localhost:15672](http://localhost:15672) |
| MailHog SMTP | Local SMTP server | 1025 | 1025 | `localhost:1025` |
| MailHog Inbox | Local email inbox | 8025 | 8025 | [http://localhost:8025](http://localhost:8025) |
| Meilisearch | Search engine | 7700 | 7700 | [http://localhost:7700](http://localhost:7700) |
| MinIO API | Object storage API | 9000 | 9000 | [http://localhost:9000](http://localhost:9000) |
| MinIO Console | Object storage dashboard | 9001 | 9001 | [http://localhost:9001](http://localhost:9001) |

Default local dashboard credentials:

- RabbitMQ: `guest` / `guest`
- MinIO: `minioadmin` / `minioadmin`

## Configuration

Local secrets should not be committed. The repository ignores common secret files such as `.env`, `docker/.env`, `appsettings.json`, and `appsettings.Development.json`.

Create `docker/.env` for Docker-based development:

```env
AZURE_SQL_CONNECTION=Server=...
JWT_SECRET=replace-with-a-long-secret
RABBITMQ_USER=guest
RABBITMQ_PASSWORD=guest
MEILI_MASTER_KEY=masterKey123
MINIO_ROOT_USER=minioadmin
MINIO_ROOT_PASSWORD=minioadmin
MINIO_BUCKET_NAME=olms-media
SMTP_HOST=mailhog
SMTP_PORT=1025
SMTP_SENDER_EMAIL=noreply@lms.com
SMTP_SENDER_NAME=LMS Notifications
```

PaymentService also expects Stripe configuration in its local app settings or environment:

```env
Stripe__SecretKey=sk_test_...
Stripe__WebhookSecret=whsec_...
```

## Run Locally

Restore dependencies from the repository root:

```powershell
dotnet restore
```

Run the gateway:

```powershell
dotnet run --project gateway/ApiGateway/ApiGateway.csproj
```

Run a service:

```powershell
dotnet run --project microservices/IdentityService/IdentityService.API/IdentityService.API.csproj
dotnet run --project microservices/CategoryService/CategoryService.API/CategoryService.API.csproj
dotnet run --project microservices/CourseService/CourseService.API/CourseService.API.csproj
dotnet run --project microservices/ContentService/ContentService.API/ContentService.API.csproj
```

Use the same pattern for the remaining services:

```powershell
dotnet run --project microservices/<ServiceName>/<ServiceName>.API/<ServiceName>.API.csproj
```

## Run with Docker

From the repository root:

```powershell
docker-compose -f docker/docker-compose.yml --env-file docker/.env up -d --build
```

Or from the `docker/` folder:

```powershell
cd docker
docker-compose --env-file .env up -d --build
```

Useful Docker commands:

| Task | Command |
| --- | --- |
| Start everything | `docker-compose -f docker/docker-compose.yml --env-file docker/.env up -d --build` |
| Rebuild after code changes | `docker-compose -f docker/docker-compose.yml --env-file docker/.env up -d --build --force-recreate` |
| Stop everything | `docker-compose -f docker/docker-compose.yml down` |
| Check service status | `docker-compose -f docker/docker-compose.yml ps` |
| View MediaService logs | `docker-compose -f docker/docker-compose.yml logs -f mediaservice` |
| Remove containers and volumes | `docker-compose -f docker/docker-compose.yml down -v` |

`down -v` deletes Docker volumes, including local MinIO and Meilisearch data.

## Entity Framework Core

Common EF Core commands:

```powershell
dotnet ef migrations add InitialCreate
dotnet ef database update
```

For service-specific migrations, run the command against the API project and the infrastructure project that contains the `DbContext`.

Example:

```powershell
dotnet ef migrations add InitialCreate `
  --project microservices/IdentityService/IdentityService.Infrastructure `
  --startup-project microservices/IdentityService/IdentityService.API

dotnet ef database update `
  --project microservices/IdentityService/IdentityService.Infrastructure `
  --startup-project microservices/IdentityService/IdentityService.API
```

## Event-Driven Flows

### Notification Flow

1. A domain action happens in a source service, such as a new enrollment.
2. The source service publishes a shared integration event, such as `EnrollmentCreatedEvent`.
3. RabbitMQ routes the message to the configured queue.
4. `NotificationService` consumes the event through MassTransit.
5. The consumer builds an email message.
6. `EmailService` sends the email through SMTP.
7. In local Docker development, MailHog captures the message at [http://localhost:8025](http://localhost:8025).

### Payment and Enrollment Flow

1. The user chooses to buy a course.
2. The client sends the request through the API Gateway to `PaymentService`.
3. `PaymentService` creates a Stripe PaymentIntent.
4. The frontend collects card details through Stripe.
5. Stripe calls the webhook endpoint after payment processing.
6. `PaymentService` updates the payment record and publishes `PaymentCompletedEvent`.
7. Downstream services, such as `EnrollmentService`, can consume the event and enroll the user.

## Development Notes

- The project targets `net10.0`.
- HTTP sample files are available in API projects as `*.http`.
- The gateway route configuration is in `gateway/ApiGateway/appsettings.json`.
- Docker service definitions live in `docker/docker-compose.yml`.
- Shared contracts should be changed carefully because multiple services depend on them.
- Keep secrets in local environment files or user secrets, not in source control.

## Contributing

1. Create a feature branch.
2. Keep secrets out of Git.
3. Follow the existing service layout.
4. Test the affected service before opening a pull request.
5. Update this README when routes, ports, infrastructure, or setup steps change.

