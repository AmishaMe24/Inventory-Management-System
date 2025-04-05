## How to Build and Run the Application

### Steps to Run the Application

1. Unzip the Project:
   - Extract the provided zip file to a folder on your computer.

2. Open the Solution:
   - Open the extracted folder and locate the `InventoryManagementSystem.sln` file.
   - Double-click the `.sln` file to open it in Visual Studio.

3. Restore Dependencies:
   - In Visual Studio, click on Build > Build Solution.

4. Run the Application:
   - Press `F5` or click on Debug > Start Debugging to run the application.
   - The application will start, and the browser opens up with the swagger UI.

5. Access the API:
   - Once the application is running, open your browser and navigate to the Swagger UI at `https://localhost:7069/swagger`.
   - Use the Swagger UI to interact with the API endpoints (e.g., place orders, update products, etc.).

---

## Key Design Decisions

### Unit of Work Pattern
The Unit of Work (UoW) pattern is implemented to manage database transactions and ensure consistency across multiple operations. Here's why it was chosen:
- Transaction Management: The UoW pattern ensures that all changes made during a business transaction are committed or rolled back as a single unit. This is critical for maintaining data integrity, especially in scenarios like order placement, where multiple tables (e.g., `Orders`, `Products`) are updated.
- Repository Abstraction: The UoW pattern encapsulates repositories (e.g., `ProductRepository`, `OrderRepository`), providing a single entry point for database operations. This reduces redundancy and improves maintainability.
- Concurrency Handling: The UoW pattern works in tandem with optimistic concurrency control to handle concurrent updates safely.

### Concurrency Handling and Thread Safety
Here's how concurrency is handled:

1. Optimistic Concurrency Control:
   - Each entity (e.g., `Product`, `Order`) has a `Version` property, which acts as a concurrency token.
   - When an update is attempted, the `Version` in the database is compared with the `Version` in the request. If they don't match, a `ConflictException` is thrown, indicating that another user has modified the resource.
   - The application retries the operation up to a maximum number of times(3 retries).

2. Thread Safety:
   - Asynchronous programming (`async`/`await`) is used throughout the application to avoid blocking threads and improve scalability.
   - The `OrderFulfillmentService` (a background service) processes pending orders asynchronously, ensuring that long-running tasks don't block the main thread.

3. Retry Mechanism:
   - In the `UnitOfWork` class, a retry mechanism is implemented to handle transient concurrency conflicts. If a conflict occurs, the operation is retried up to 3 times before failing.

### Asynchronous Processing
- Database Operations: All database operations (e.g., `SaveChangesAsync`, `GetPendingOrdersAsync`) are performed asynchronously to avoid blocking the main thread.
- Background Services: The `OrderFulfillmentService` runs as a background service, processing pending orders asynchronously every 10 seconds.

---

## Error Handling & Logging

### Global Exception Handling
- The `ExceptionHandlingMiddleware` catches unhandled exceptions and returns appropriate HTTP status codes (e.g., 404 for `NotFoundException`, 409 for `ConflictException`).
- Custom exceptions (e.g., `NotFoundException`, `BadRequestException`) are used to provide meaningful error messages to the client.

### Logging
- The application uses `ILogger` to log important events and errors. Logs are output to the console, making it easy to diagnose issues during development and testing.
- Logs include details such as the number of pending orders, concurrency conflicts, and database update errors.

---

## Unit Tests

### Running Unit Tests
1. Open the Test Project in the Terminal:
    cd `InventoryManagementSystem.Tests`

2. Run the Tests by running the following command:
   dotnet test

Note: Please delete the bin and obj folders from InventoryManagementSystem.Tests if error comes while running the test.
