# CQRS Booking System

This project is a .NET Core web application demonstrating a robust booking system built with a Command Query Responsibility Segregation (CQRS) and Event-Driven Architecture. It leverages multiple data stores and messaging patterns to achieve scalability, resilience, and real-time capabilities.

## Architecture Overview

The system is structured into several layers, following domain-driven design principles:

*   **API Layer (`src/API`):** The entry point for the application, exposing RESTful endpoints for commands and queries, and SignalR hubs for real-time communication with clients.
*   **Application Layer (`src/Application`):** Contains the application's use cases, defining commands, queries, and their respective handlers. It orchestrates the business logic and interacts with the domain and infrastructure layers through abstractions.
*   **Domain Layer (`src/Domain`):** The heart of the business logic, defining entities (e.g., `Booking`, `Seat`, `Showtime`), value objects, enums, and domain events (e.g., `SeatReservedEvent`). It encapsulates the core business rules and state transitions.
*   **Infrastructure Layer (`src/Infrastructure`):** Provides concrete implementations for the abstractions defined in the Application and Domain layers. This includes data access (repositories), external service integrations (e.g., messaging, caching), and consumers for processing events.

## Key Technologies and Patterns

*   **.NET Core:** The primary development framework.
*   **ASP.NET Core:** Used for building the web API and SignalR hubs.
*   **CQRS (Command Query Responsibility Segregation):** Explicit separation of concerns between operations that change state (commands like `ReserveSeatCommand`) and operations that read state (queries).
*   **Event-Driven Architecture:** Critical business events (e.g., `SeatReservedEvent`) are published and consumed, enabling decoupled services and reactive programming.
*   **MassTransit & RabbitMQ:** Utilized as a message broker for reliable asynchronous communication and event publishing/consumption. A **Transactional Outbox Pattern** (leveraging Entity Framework Core) ensures that events are published reliably as part of database transactions.
*   **Entity Framework Core & PostgreSQL:** Serves as the **write-model** database for transactional data storage (e.g., `Booking`, `Seat`, `Showtime` entities). **Optimistic Concurrency** is implemented for the `Seat` entity to handle concurrent updates and prevent race conditions.
*   **MongoDB:** Used as the **read-model** database. Denormalized data (e.g., `ShowtimeDocument`) is stored here, optimized for efficient querying and display, providing eventual consistency with the write-model.
*   **Valkey (Redis) & StackExchange.Redis:** Employed for a distributed **Seat Locking Service**, acting as a "Gatekeeper". This service uses atomic `SET NX EX` commands to implement a distributed lock, effectively preventing multiple users from concurrently reserving the same seat. The `NX` (Not eXists) ensures the lock is only set if it doesn't already exist, and `EX` (EXpire) sets a timeout, preventing deadlocks from crashed processes. This ensures transactional integrity and prevents race conditions in high-concurrency scenarios.
    *   **The Gatekeeper Pattern:** In this context, the Gatekeeper pattern refers to using a separate, highly performant service (Valkey) to control access to a critical section or resource (a seat for a showtime). It acts as a single point of entry to ensure that only one request can proceed to attempt a reservation for a specific seat at any given moment, thus preventing double-bookings.
    *   **References for further reading:**
        *   **Distributed Locks with Redis:** [Redis Documentation on Distributed Locks](https://redis.io/docs/manual/patterns/distributed-locks/)
        *   **Gatekeeper Pattern (General Microservices Context):** While not exclusively tied to Redis, the concept of a "Gatekeeper" service often aligns with API Gateways or dedicated concurrency control services. Searching for "API Gateway pattern" or "concurrency control in distributed systems" can provide broader context.
*   **SignalR:** Enables real-time, bi-directional communication between the server and connected clients, providing instant updates to the frontend (e.g., when a seat's status changes).
*   **Docker Compose:** Orchestrates all the necessary infrastructure services (PostgreSQL, Valkey, RabbitMQ, MongoDB) for local development and testing.

## Getting Started

### Prerequisites

*   [.NET SDK](https://dotnet.microsoft.com/download) (Version 8.0 or later)
*   [Docker Desktop](https://www.docker.com/products/docker-desktop) (or a Docker-compatible environment)

### Building and Running Locally

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-repo/cqrs-booking.git # Replace with actual repo URL
    cd cqrs-booking
    ```
2.  **Start the infrastructure services and the API:**
    Ensure Docker is running, then execute the following command in the project's root directory (where `docker-compose.yml` is located):
    ```bash
    docker-compose up --build
    ```
    This command will:
    *   Build the Docker images for the application and its dependencies.
    *   Start PostgreSQL, Valkey, RabbitMQ, MongoDB, and the .NET API service.
3.  **Access the API:**
    The API will be accessible, typically at `http://localhost:5000`. You might want to check `src/API/Properties/launchSettings.json` for the exact configured ports.
    The SignalR hub endpoint is `/hub/bookings`.

### Running Tests

The project includes integration tests to ensure the system's components work correctly together.

1.  **Navigate to the test project directory:**
    ```bash
    cd tests/IntegrationTests
    ```
2.  **Run the tests:**
    ```bash
    dotnet test
    ```
    Alternatively, you can run tests using your IDE's test runner (e.g., Visual Studio Test Explorer, Rider's Unit Tests).

## Development Conventions

*   **Clear Layered Architecture:** Strict separation of concerns across API, Application, Domain, and Infrastructure projects.
*   **Explicit CQRS:** Commands and handlers are clearly defined, separating write concerns.
*   **Event-Driven Principles:** Domain events drive reactions and updates across the system, fostering loose coupling.
*   **Dependency Injection:** Services are registered and resolved via constructor injection throughout the application.
*   **Robust Concurrency Handling:** Critical operations, like seat reservations, employ optimistic concurrency (EF Core) and distributed locking (Redis/Valkey) to maintain data integrity under high load.
*   **Reliable Messaging:** The transactional outbox pattern with MassTransit ensures that messages are published durably.
