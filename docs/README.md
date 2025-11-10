# ADPA .NET Project

This is an ADPA (Automated Data Processing Application) built with .NET 9.0 and C#.

## Prerequisites

- .NET 9.0 SDK: https://dotnet.microsoft.com/download

## Project Structure

```
ADPA.Net/
├── Controllers/          # Web API Controllers
│   ├── HealthController.cs
│   └── DataController.cs
├── Models/              # Data models and DTOs
│   ├── ProcessedData.cs
│   └── DTOs/
│       └── ApiDtos.cs
├── Services/            # Business logic services
│   ├── DataProcessingService.cs
│   └── HealthCheckService.cs
├── Properties/          # Project properties
│   └── launchSettings.json
├── appsettings.json     # Configuration settings
├── Program.cs           # Application entry point
├── ADPA.csproj         # Project file
└── .gitignore          # Git ignore file
```

## Getting Started

1. Install .NET 9.0 SDK from https://dotnet.microsoft.com/download
2. Navigate to the project directory
3. Run `dotnet restore` to restore dependencies
4. Run `dotnet build` to build the project
5. Run `dotnet run` to start the application

The application will start on:
- HTTP: http://localhost:5050
- HTTPS: https://localhost:7050

## API Endpoints

- **GET /api/health** - Health check endpoint
  - Returns application health status and version
- **GET /api/data** - Retrieve all processed data
  - Returns list of all data processing requests and their status
- **GET /api/data/{id}** - Retrieve specific processed data by ID
- **POST /api/data** - Submit data for processing
  - Body: `{"rawData": "your data here"}`
  - Returns processing request with ID and status
- **POST /api/data/process-pending** - Manually trigger processing of pending data

## Example Usage

### Health Check
```bash
curl http://localhost:5050/api/health
```

### Submit Data for Processing
```bash
curl -X POST http://localhost:5050/api/data \
  -H "Content-Type: application/json" \
  -d '{"rawData": "sample data to process"}'
```

### Get All Data
```bash
curl http://localhost:5050/api/data
```

## Development

This project follows clean architecture principles with:
- **Controllers**: Handle HTTP requests and responses
- **Services**: Contain business logic and data processing
- **Models**: Define data structures and DTOs
- **In-Memory Storage**: Uses concurrent dictionary for data persistence
- **Dependency Injection**: Loose coupling between components
- **CORS**: Configured for cross-origin requests

## Features

- **Asynchronous Processing**: Data processing happens in background tasks
- **Status Tracking**: Track processing status (Pending, Processing, Completed, Failed)
- **Error Handling**: Comprehensive error handling with logging
- **Health Monitoring**: Built-in health check endpoint
- **Clean Architecture**: Separation of concerns and SOLID principles

## Configuration

The application uses standard ASP.NET Core configuration:
- `appsettings.json` - Production settings  
- `appsettings.Development.json` - Development settings
- `Properties/launchSettings.json` - Development server settings

## VS Code Tasks

Available tasks:
- **build** - Build the project (`Ctrl+Shift+P` → "Tasks: Run Task" → "build")
- **run** - Run the application (`Ctrl+Shift+P` → "Tasks: Run Task" → "run")