# 🚢 ContainerFlow

> A miniature maritime container shipment management and tracking platform built with modern .NET, Vue 3, and event-driven microservices architecture.

# Inspiration

## 🌏 Background

### Why I built ContainerFlow

While working as a **Full Stack Developer in the logistics department of an automotive manufacturing company**, I noticed that many warehouse and shipping activities were still highly dependent on manual coordination between operators.

One recurring challenge was **container visibility**. Operators often knew that a container had arrived at the plant or warehouse, but locating its exact position or knowing its latest operational status required checking spreadsheets, printed documents, or asking multiple departments through phone calls or chat.

Although the actual internal systems are confidential, the workflow inspired the idea behind **ContainerFlow**.

### Operational problems observed

| Current Challenge                                                      | Operational Impact                                                            |
| ---------------------------------------------------------------------- | ----------------------------------------------------------------------------- |
| Container locations are updated manually                               | Operators spend time searching for containers in the yard or warehouse        |
| Shipment status is communicated through chat or spreadsheets           | Different departments often work with outdated information                    |
| No centralized timeline for container movement                         | Difficult to identify where delays occur during inbound or outbound logistics |
| Booking, warehouse, and transportation teams maintain separate records | Information must be reconciled manually, increasing the risk of human error   |
| Managers rely on manual reports for shipment progress                  | Operational visibility is delayed and not available in real time              |

### A simplified real-world scenario

Imagine an inbound container carrying automotive components arrives at the factory.

Instead of opening a dashboard, an operator might need to:

1. Check the shipment spreadsheet.
2. Ask the receiving team whether the container has arrived.
3. Contact the yard operator to locate the container.
4. Verify whether the container has already been unloaded.
5. Update another spreadsheet so the production team knows the material is available.

Even if each individual step only takes a few minutes, repeating this process dozens of times per day creates unnecessary operational overhead.

### Project objective

ContainerFlow is my attempt to redesign that workflow using a modern microservices architecture.

Rather than focusing on ERP complexity, the project focuses on **operational visibility**.

The system aims to provide:

* A single source of truth for shipment and container status
* Real-time container tracking across operational stages
* Event-driven updates instead of manual notifications
* A dashboard for operators and supervisors to monitor logistics activity
* Clear ownership of data through independent backend services

### Scope disclaimer

This project is **not** a reproduction of Toyota Motor Manufacturing Indonesia's internal systems or business processes.

It is an independent portfolio project inspired by general operational challenges commonly found in manufacturing and logistics environments. All workflows, data models, and architecture are designed for educational and demonstration purposes only.

The goal is not to build a complete enterprise logistics system, but to demonstrate how a small distributed application can be designed around meaningful business boundaries using:

* .NET Web API microservices
* YARP API Gateway
* RabbitMQ + MassTransit
* PostgreSQL with database-per-service
* Vue 3 + TypeScript + Tailwind CSS
* JWT authentication and role-based access control
* Docker Compose
* Automated testing
* Health checks and observability

The project focuses on **architecture, domain modeling, asynchronous communication, and production-oriented development practices** rather than simply creating multiple CRUD APIs.

---

## 📌 Project Overview

ContainerFlow simulates a simplified container shipping operation.

A customer creates a shipment booking, containers are allocated to the shipment, and the shipment progresses through different operational states.

Example workflow:

```text
Customer
   │
   ▼
Create Booking
   │
   ▼
Booking Confirmed
   │
   ▼
Container Allocated
   │
   ▼
Loaded on Vessel
   │
   ▼
Vessel Departed
   │
   ▼
In Transit
   │
   ▼
Arrived at Destination
   │
   ▼
Container Discharged
   │
   ▼
Delivered
```

The system represents this workflow through several independently deployable services.

---

# 🎯 Project Goals

The primary goal of ContainerFlow is to demonstrate practical understanding of:

### Backend

* ASP.NET Core Web API
* RESTful API design
* Microservice architecture
* Domain-driven service boundaries
* Entity Framework Core
* PostgreSQL
* Database-per-service
* Dependency injection
* Configuration management
* JWT authentication
* Role-based authorization

### Distributed Systems

* API Gateway
* Asynchronous messaging
* RabbitMQ
* MassTransit
* Domain/integration events
* Eventual consistency
* Service isolation
* Health checks

### Frontend

* Vue 3
* TypeScript
* Composition API
* Tailwind CSS
* Dashboard design
* API integration
* Authentication state
* Role-based UI

### Infrastructure

* Docker
* Docker Compose
* Containerized services
* Environment-based configuration
* Local development infrastructure

### Engineering Practices

* Unit testing
* Integration testing
* API documentation
* Architecture documentation
* Structured logging
* Observability

---

# 🏗️ Architecture

ContainerFlow follows a simplified microservices architecture.

```text
                         ┌─────────────────────────┐
                         │     Vue 3 Dashboard     │
                         │ TypeScript + Tailwind   │
                         └────────────┬────────────┘
                                      │
                                      │ HTTP
                                      ▼
                         ┌─────────────────────────┐
                         │      YARP Gateway       │
                         │                         │
                         │ Routing                 │
                         │ Authentication          │
                         │ Authorization           │
                         └────────────┬────────────┘
                                      │
                     ┌────────────────┼────────────────┐
                     │                │                │
                     ▼                ▼                ▼
              ┌────────────┐   ┌────────────┐   ┌──────────────┐
              │  Booking   │   │ Container  │   │ Notification │
              │  Service   │   │  Service   │   │   Service    │
              └─────┬──────┘   └─────┬──────┘   └──────┬───────┘
                    │                │                  │
                    ▼                ▼                  ▼
               PostgreSQL       PostgreSQL         PostgreSQL
                    │                │
                    │                │
                    └───────┬────────┘
                            │
                            ▼
                     ┌──────────────┐
                     │   RabbitMQ   │
                     │  MassTransit │
                     └──────────────┘
```

## Core architectural principles

### 1. Database per service

Each service owns its own database.

```text
Booking Service       → booking_db
Container Service     → container_db
Notification Service  → notification_db
```

A service must not directly query another service's database.

Instead, services communicate through APIs or messages.

This keeps service boundaries explicit and allows each service to evolve independently.

---

### 2. API Gateway

The frontend communicates with a single API endpoint.

```text
Vue Dashboard
      │
      ▼
http://localhost:8080
      │
      ├── /api/bookings
      ├── /api/containers
      └── /api/notifications
```

YARP is responsible for routing requests to the appropriate backend service.

This prevents the frontend from needing to know the internal network topology of the application.

---

### 3. Synchronous vs asynchronous communication

Not every operation needs messaging.

ContainerFlow intentionally demonstrates both approaches.

#### REST

Used when an immediate response is required.

```text
Frontend
   │
   ▼
Gateway
   │
   ▼
Booking Service
   │
   ▼
Response
```

#### RabbitMQ

Used for events and asynchronous workflows.

```text
Booking Service
      │
      │ BookingCreated
      ▼
   RabbitMQ
      │
      ├───────────────┐
      ▼               ▼
Notification      Other Consumers
Service
```

This demonstrates the difference between **request/response communication** and **event-driven communication**.

---

# 🧩 Microservices

ContainerFlow currently consists of three core services and one gateway.

---

## 📦 Booking Service

Responsible for the commercial and operational booking side of the system.

### Responsibilities

* Create bookings
* Manage shipment information
* Associate customers with bookings
* Track booking status
* Confirm or cancel bookings
* Publish booking-related events

### Example domain

```text
Booking
├── Booking Number
├── Customer
├── Origin
├── Destination
├── Cargo
├── Containers
├── Vessel/Voyage
└── Status
```

### Example statuses

```text
PENDING
CONFIRMED
CANCELLED
IN_PROGRESS
COMPLETED
```

### Example API

```http
POST   /api/bookings
GET    /api/bookings
GET    /api/bookings/{id}
PUT    /api/bookings/{id}
POST   /api/bookings/{id}/confirm
POST   /api/bookings/{id}/cancel
```

---

# 🚢 Container Service

Responsible for physical shipping containers and their allocation.

### Responsibilities

* Register containers
* Track container type
* Allocate containers to shipments
* Track container status
* Track container movements
* Calculate utilization

### Example container types

```text
20FT
40FT
40HC
REEFER
```

### Example statuses

```text
AVAILABLE
ALLOCATED
IN_YARD
LOADED
IN_TRANSIT
DISCHARGED
DELIVERED
```

### Example API

```http
GET    /api/containers
GET    /api/containers/{id}
POST   /api/containers
POST   /api/containers/{id}/allocate
POST   /api/containers/{id}/move
GET    /api/containers/utilization
```

---

# 🔔 Notification Service

Responsible for notifications generated by operational events.

This service is intentionally separated from the core booking and container domains.

### Responsibilities

* Consume integration events
* Create notifications
* Track notification status
* Expose notification history
* Eventually support external notification providers

### Example notifications

```text
Booking confirmed
Container allocated
Container loaded
Shipment departed
Shipment arrived
Container delivered
```

Example event flow:

```text
Booking Service
      │
      │ BookingConfirmed
      ▼
   RabbitMQ
      │
      ▼
Notification Service
      │
      ▼
Create Notification
```

The notification service does not need to block the booking operation.

This demonstrates **eventual consistency**.

---

# 📨 Event-Driven Architecture

RabbitMQ and MassTransit are used for asynchronous communication.

The initial integration events include:

```text
BookingCreated
BookingConfirmed
ContainerAllocated
ShipmentStatusChanged
ContainerStatusChanged
```

Example:

```text
┌───────────────────┐
│  Booking Service  │
└─────────┬─────────┘
          │
          │ BookingConfirmed
          ▼
     ┌──────────┐
     │ RabbitMQ │
     └─────┬────┘
           │
           ▼
┌──────────────────────┐
│ Notification Service │
└──────────────────────┘
```

## Why events?

The services should not become tightly coupled.

For example, the Booking Service should not need to know:

```text
How notifications are sent
Where notifications are stored
Which notification provider is used
```

It only needs to publish:

```text
BookingConfirmed
```

Consumers can then independently react to that event.

---

# 🔐 Authentication & Authorization

ContainerFlow uses JWT-based authentication.

The application supports three roles:

| Role       | Description                                        |
| ---------- | -------------------------------------------------- |
| `admin`    | Full system access                                 |
| `staff`    | Operational access to bookings and containers      |
| `customer` | Access to their own bookings and shipment tracking |

Example:

```text
                    JWT
                     │
                     ▼
              ┌─────────────┐
              │ API Gateway │
              └──────┬──────┘
                     │
           ┌─────────┼─────────┐
           ▼         ▼         ▼
         admin     staff    customer
```

Example authorization rules:

```text
Admin
 ├── Manage users
 ├── Manage bookings
 ├── Manage containers
 └── View notifications

Staff
 ├── Manage bookings
 ├── Manage containers
 └── View notifications

Customer
 ├── Create booking
 ├── View own booking
 └── Track own shipment
```

The initial implementation keeps identity management intentionally lightweight so that the project can focus on microservice architecture.

---

# 🖥️ Frontend

The frontend is built with:

* Vue 3
* TypeScript
* Vite
* Tailwind CSS

The application provides an operational dashboard for viewing shipping activity.

## Planned pages

```text
/login

/dashboard

/bookings
/bookings/:id

/containers

/notifications
```

---

## Dashboard

The main dashboard provides a high-level operational overview.

Example metrics:

```text
┌──────────────────────────────────────────┐
│ Active Shipments          128            │
│ Total Containers          842            │
│ Container Utilization      74%           │
│ Pending Bookings            23           │
└──────────────────────────────────────────┘
```

### Charts

The dashboard will eventually display:

* Shipment status distribution
* Container utilization
* Booking trends
* Container movement trends
* Recent operational events

---

# 🗂️ Repository Structure

```text
container-flow/
│
├── backend/
│   │
│   ├── ContainerFlow.sln
│   │
│   ├── src/
│   │   │
│   │   ├── Gateway/
│   │   │   └── ContainerFlow.Gateway/
│   │   │
│   │   ├── Services/
│   │   │   ├── Booking/
│   │   │   │   └── ContainerFlow.Booking.Api/
│   │   │   │
│   │   │   ├── Container/
│   │   │   │   └── ContainerFlow.Container.Api/
│   │   │   │
│   │   │   └── Notification/
│   │   │       └── ContainerFlow.Notification.Api/
│   │   │
│   │   └── BuildingBlocks/
│   │       ├── ContainerFlow.Contracts/
│   │       └── ContainerFlow.Shared/
│   │
│   └── tests/
│       ├── ContainerFlow.Booking.Tests/
│       ├── ContainerFlow.Container.Tests/
│       └── ContainerFlow.Notification.Tests/
│
├── frontend/
│   └── container-flow-dashboard/
│
├── infrastructure/
│   ├── docker/
│   │   ├── gateway.Dockerfile
│   │   ├── booking.Dockerfile
│   │   ├── container.Dockerfile
│   │   └── notification.Dockerfile
│   │
│   └── postgres/
│       └── init/
│
├── docs/
│   ├── architecture.md
│   ├── domain-model.md
│   ├── api-overview.md
│   └── events.md
│
├── docker-compose.yml
├── .dockerignore
├── .env.example
├── .gitignore
└── README.md
```

---

# 🛠️ Technology Stack

## Backend

| Technology            | Purpose                     |
| --------------------- | --------------------------- |
| .NET / ASP.NET Core   | Web API                     |
| Entity Framework Core | ORM                         |
| PostgreSQL            | Persistent storage          |
| YARP                  | API Gateway                 |
| MassTransit           | Message-based communication |
| RabbitMQ              | Message broker              |
| JWT                   | Authentication              |
| Swagger / OpenAPI     | API documentation           |

## Frontend

| Technology    | Purpose                 |
| ------------- | ----------------------- |
| Vue 3         | UI framework            |
| TypeScript    | Type safety             |
| Vite          | Frontend tooling        |
| Tailwind CSS  | Styling                 |
| Pinia         | State management        |
| Vue Router    | Routing                 |
| Chart library | Dashboard visualization |

## Infrastructure

| Technology     | Purpose             |
| -------------- | ------------------- |
| Docker         | Containerization    |
| Docker Compose | Local orchestration |
| PostgreSQL     | Service databases   |
| RabbitMQ       | Messaging           |

## Testing

| Technology       | Purpose             |
| ---------------- | ------------------- |
| xUnit            | Unit testing        |
| FluentAssertions | Test assertions     |
| Testcontainers   | Integration testing |

---

# 🐳 Running the Project

The goal is to make the entire application start with a single command.

### Prerequisites

Install:

* Docker Desktop
* Git
* .NET SDK
* Node.js

### Clone

```bash
git clone https://github.com/<your-username>/container-flow.git

cd container-flow
```

### Environment configuration

Copy:

```bash
cp .env.example .env
```

Update the environment variables if necessary.

### Start the application

```bash
docker compose up --build
```

This will eventually start:

```text
Vue Dashboard
API Gateway
Booking API
Container API
Notification API
Booking PostgreSQL
Container PostgreSQL
Notification PostgreSQL
RabbitMQ
```

---

# 🔌 Local Services

During development, the following endpoints will be exposed:

| Service             | URL                      |
| ------------------- | ------------------------ |
| Frontend            | `http://localhost:5173`  |
| API Gateway         | `http://localhost:8080`  |
| RabbitMQ Management | `http://localhost:15672` |
| Booking API         | Internal                 |
| Container API       | Internal                 |
| Notification API    | Internal                 |

The frontend should communicate with the **Gateway**, not directly with individual services.

---

# 📚 API Documentation

Each API will expose OpenAPI/Swagger documentation during development.

Example:

```text
/api/bookings
/api/containers
/api/notifications
```

The Gateway provides the public-facing API surface while individual services remain internal.

---

# ❤️ Domain Example

A typical shipment might look like:

```json
{
  "bookingNumber": "BK-2026-00182",
  "customer": "PT Example Indonesia",
  "origin": "Jakarta",
  "destination": "Singapore",
  "vessel": "MV ContainerFlow Express",
  "voyage": "CF-026",
  "containers": [
    {
      "number": "MSKU1234567",
      "type": "40HC"
    }
  ],
  "status": "IN_TRANSIT"
}
```

The shipment can then generate operational events:

```text
BookingCreated
        ↓
BookingConfirmed
        ↓
ContainerAllocated
        ↓
ContainerLoaded
        ↓
ShipmentDeparted
        ↓
ShipmentArrived
        ↓
ContainerDischarged
        ↓
ShipmentDelivered
```

---

# 🔄 Example Distributed Workflow

Consider a customer confirming a booking.

### Step 1 — Client request

```text
Vue
 │
 ▼
POST /api/bookings/{id}/confirm
```

### Step 2 — Gateway

```text
Gateway
 │
 ▼
Booking Service
```

### Step 3 — Booking Service

The service updates its own database:

```text
Booking
Status = CONFIRMED
```

Then publishes:

```text
BookingConfirmed
```

### Step 4 — RabbitMQ

```text
Booking Service
      │
      ▼
    RabbitMQ
      │
      ▼
Notification Service
```

### Step 5 — Notification Service

Creates:

```text
"Booking BK-2026-00182 has been confirmed."
```

The Booking Service does not need to wait for this operation.

---

# 📊 Container Utilization

One of the dashboard's primary metrics is container utilization.

Example:

```text
Total Capacity     2,000 TEU
Allocated          1,420 TEU
Available            580 TEU

Utilization          71%
```

The dashboard can visualize this using:

```text
Allocated     ███████████████░░░░░
Available     ██████░░░░░░░░░░░░░░

              71% utilized
```

This gives the project a realistic logistics-oriented business metric rather than being purely technical.

---

# 🧪 Testing Strategy

Testing will be separated by responsibility.

## Unit Tests

Used for business rules such as:

```text
Booking status transitions
Container allocation rules
Container status transitions
Utilization calculation
Role authorization rules
```

Example:

```text
Given a container is already allocated
When another shipment attempts to allocate it
Then the operation should fail
```

---

## Integration Tests

Integration tests will verify:

```text
API
 ↓
Database
```

and eventually:

```text
Service
 ↓
RabbitMQ
 ↓
Consumer
```

Testcontainers can be used to run real PostgreSQL and RabbitMQ containers during integration tests.

---

# 🩺 Health Checks

Each backend service will expose health information.

Example:

```http
GET /health
```

Expected response:

```text
Healthy
```

The health checks will eventually cover dependencies such as:

```text
Booking API
 ├── Application
 └── PostgreSQL

Container API
 ├── Application
 └── PostgreSQL

Notification API
 ├── Application
 ├── PostgreSQL
 └── RabbitMQ
```

This is particularly important when running the application through Docker Compose.

---

# 📈 Observability

The project is designed to eventually support:

* Structured logging
* Correlation IDs
* Distributed tracing
* Metrics
* Health checks

A request should be traceable across services:

```text
Frontend
   │
   ▼
Gateway
   │
   ▼
Booking Service
   │
   ▼
RabbitMQ
   │
   ▼
Notification Service
```

This makes it possible to investigate distributed failures more effectively.

---

# 🔐 Security Considerations

The project will follow several basic security principles:

* JWT authentication
* Role-based authorization
* Secrets provided through environment variables
* No credentials committed to Git
* Input validation
* Service boundaries
* Internal services not unnecessarily exposed publicly

This project is intended for learning and portfolio purposes and is **not production-ready security infrastructure**.

---

# 🧠 Architecture Decisions

The project intentionally makes several architectural decisions that can be discussed during an interview.

## Why microservices?

The purpose is not to claim that microservices are always better.

The project uses microservices to demonstrate:

* Independent service ownership
* Database isolation
* Service-to-service communication
* Asynchronous messaging
* Eventual consistency
* Independent deployment boundaries

---

## Why three services?

Three services provide enough complexity to demonstrate distributed architecture without creating unnecessary operational overhead.

```text
Booking
Container
Notification
```

Each service has a meaningful business responsibility.

---

## Why RabbitMQ?

RabbitMQ provides a straightforward way to demonstrate asynchronous event-driven communication.

It allows services to publish events without knowing which services will consume them.

---

## Why MassTransit?

MassTransit reduces the amount of low-level messaging infrastructure required when working with RabbitMQ and provides useful abstractions around:

* Consumers
* Messages
* Endpoints
* Retry policies
* Messaging pipelines

---

## Why YARP?

YARP provides a modern, configurable reverse proxy and API gateway implementation for .NET.

It keeps the frontend independent from the internal service topology.

---

## Why database-per-service?

Each service owns its own persistence model.

This avoids creating a distributed system where multiple services directly depend on the same database schema.

---

# 🚧 Current Project Status

> **Phase: Architecture & Project Setup**

The initial phase focuses on establishing the repository and project structure.

### Completed

* [x] Repository structure
* [x] Microservice boundaries defined
* [x] Backend solution structure
* [x] Frontend structure
* [x] Docker structure
* [x] Documentation structure

### In Progress

* [ ] ASP.NET Core project initialization
* [ ] YARP Gateway configuration
* [ ] Vue dashboard initialization
* [ ] Docker Compose configuration
* [ ] PostgreSQL configuration
* [ ] RabbitMQ configuration

### Planned

* [ ] Booking CRUD
* [ ] Container management
* [ ] Shipment tracking
* [ ] RabbitMQ integration
* [ ] MassTransit consumers
* [ ] JWT authentication
* [ ] Role-based authorization
* [ ] Dashboard
* [ ] Shipment charts
* [ ] Container utilization metrics
* [ ] Health checks
* [ ] Structured logging
* [ ] Integration tests
* [ ] End-to-end demo workflow

---

# 🗺️ Development Roadmap

## Phase 1 — Foundation

```text
Repository
    ↓
.NET Solution
    ↓
Microservice projects
    ↓
Vue application
    ↓
Docker Compose
```

---

## Phase 2 — Core Domain

```text
Booking
    ↓
Shipment
    ↓
Container
```

Implement the core domain models and persistence.

---

## Phase 3 — API Gateway

Configure YARP:

```text
/api/bookings
/api/containers
/api/notifications
```

---

## Phase 4 — Messaging

Introduce:

```text
RabbitMQ
     +
MassTransit
```

Then implement events:

```text
BookingCreated
BookingConfirmed
ContainerAllocated
ShipmentStatusChanged
```

---

## Phase 5 — Authentication

Implement:

```text
JWT
 │
 ├── admin
 ├── staff
 └── customer
```

---

## Phase 6 — Frontend

Build:

```text
Login
Dashboard
Bookings
Containers
Tracking
Notifications
```

---

## Phase 7 — Observability

Add:

```text
Health checks
Structured logging
Correlation IDs
Distributed tracing
Metrics
```

---

## Phase 8 — Testing

Add:

```text
Unit Tests
Integration Tests
Messaging Tests
End-to-End Tests
```

---

# 🎓 What This Project Demonstrates

ContainerFlow is intentionally designed to demonstrate skills commonly required for a modern .NET backend/full-stack developer.

### .NET

```text
ASP.NET Core
Entity Framework Core
Dependency Injection
Configuration
Middleware
Authentication
Authorization
REST APIs
```

### Microservices

```text
Service boundaries
Database per service
API Gateway
REST communication
Message-based communication
Event-driven architecture
Eventual consistency
```

### Frontend

```text
Vue 3
TypeScript
Tailwind CSS
State management
API integration
Dashboard development
```

### Infrastructure

```text
Docker
Docker Compose
PostgreSQL
RabbitMQ
Environment configuration
Health checks
```

### Engineering

```text
Domain modeling
Business rules
Testing
Observability
Documentation
Architecture decisions
```

---

# 💼 Portfolio / Resume Relevance

This project is designed to be more than a generic CRUD application.

Instead of:

> Built an e-commerce API using .NET.

The project demonstrates a domain-oriented distributed system:

> **Designed and developed a container shipment management platform using ASP.NET Core microservices, YARP API Gateway, PostgreSQL database-per-service architecture, and RabbitMQ/MassTransit for event-driven communication. Built a Vue 3 operational dashboard with JWT-based role authorization and Docker Compose orchestration.**

Potential technical talking points include:

* Why microservices were chosen
* How service boundaries were determined
* Why each service owns its database
* When REST is preferable to messaging
* How eventual consistency is handled
* How duplicate messages are handled
* How container status transitions are validated
* How authentication is propagated through the gateway
* How distributed requests are traced
* How the system behaves when a service becomes unavailable

---

# ⚠️ Scope

ContainerFlow is a **portfolio and learning project**.

It intentionally simplifies many real maritime logistics processes.

It does not attempt to model:

* Real shipping company operations
* Real customs procedures
* Real port authority integrations
* Real vessel scheduling systems
* Real-time vessel AIS data
* Actual payment processing
* Production-grade identity management
* Real customer data

The goal is to demonstrate **software engineering concepts through a realistic domain**, not to reproduce a complete commercial logistics platform.

---

# 📷 Planned Demo

The final repository will include screenshots and/or a short demo showing:

### Dashboard

```text
Shipment Statistics
Container Utilization
Recent Bookings
Operational Alerts
```

### Booking

```text
Create Booking
View Booking
Confirm Booking
Track Shipment
```

### Container

```text
Container List
Container Details
Allocation
Status
Movement History
```

### Event Flow

```text
Booking Confirmed
        ↓
RabbitMQ
        ↓
Notification Created
        ↓
Dashboard Updated
```

---

# 📄 Documentation

Additional architecture documentation:

* [`Architecture`](docs/architecture.md)
* [`Domain Model`](docs/domain-model.md)
* [`API Overview`](docs/api-overview.md)
* [`Event Flow`](docs/events.md)

---

# 📜 License

This project is created for educational and portfolio purposes.

---

## 👨‍💻 Author

**Faza Akbar**

Full Stack Developer

Focused on:

```text
.NET
Angular / Vue
Microservices
Cloud & Infrastructure
Distributed Systems
```

---

> **ContainerFlow — a small logistics problem designed to demonstrate real distributed-system engineering.**

