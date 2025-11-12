# Quickstart: Food API Development

**Feature**: Food API with Full Test Coverage and Infrastructure  
**Date**: 2025-11-11  
**Audience**: Developers setting up local development environment

## Overview

This quickstart guide helps you set up, build, test, and run the Food API locally. Follow these steps to get started quickly.

---

## Prerequisites

### Required Software
- **.NET 9.0 SDK**: [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Docker Desktop**: [Download here](https://www.docker.com/products/docker-desktop)
- **Azure Cosmos DB Emulator**: Use Docker image (Linux/macOS) or [Windows installer](https://aka.ms/cosmosdb-emulator)
- **Git**: For cloning and version control
- **Visual Studio 2022** (optional) or **VS Code** with C# extension

### Verify Installations
```powershell
# Check .NET version
dotnet --version  # Should be 9.0.x

# Check Docker
docker --version

# Check Git
git --version
```

---

## Quick Start (5 Minutes)

### 1. Clone Repository
```powershell
git clone https://github.com/willvelida/biotrackr.git
cd biotrackr
git checkout 012-food-api
```

### 2. Start Cosmos DB Emulator
```powershell
# From repository root
.\cosmos-emulator.ps1
```

Wait for emulator to be ready (~30 seconds). You should see:
```
Cosmos DB Emulator is running at https://localhost:8081
```

### 3. Build Solution
```powershell
cd src/Biotrackr.Food.Api
dotnet restore
dotnet build
```

### 4. Run API
```powershell
cd Biotrackr.Food.Api
dotnet run
```

API will start at `http://localhost:8080`

### 5. Test Endpoints
Open browser or use `curl`:
```powershell
# Health check
curl http://localhost:8080/healthz/liveness

# Get all food logs (empty initially)
curl http://localhost:8080/

# Swagger UI
Start-Process http://localhost:8080/swagger
```

---

## Detailed Setup

### Project Structure
```
src/Biotrackr.Food.Api/
├── Biotrackr.Food.Api/              # Main API project
├── Biotrackr.Food.Api.UnitTests/    # Unit tests
├── Biotrackr.Food.Api.IntegrationTests/  # Integration tests
└── Biotrackr.Food.Api.sln           # Solution file
```

### Configuration

#### Local Development (appsettings.Development.json)
```json
{
  "CosmosDb": {
    "Endpoint": "https://localhost:8081",
    "AccountKey": "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
    "DatabaseName": "Biotrackr",
    "ContainerName": "HealthData",
    "PartitionKeyPath": "/documentType"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**Note**: The account key shown is the default Cosmos DB Emulator key (safe for local use).

#### Production (Azure App Configuration)
Configuration keys:
- `Biotrackr:CosmosDb:Endpoint`
- `Biotrackr:CosmosDb:DatabaseName`
- `Biotrackr:CosmosDb:ContainerName`
- `Biotrackr:CosmosDb:PartitionKeyPath`

Managed Identity used instead of account key in production.

---

## Running Tests

### Unit Tests
```powershell
cd src/Biotrackr.Food.Api/Biotrackr.Food.Api.UnitTests
dotnet test --logger "console;verbosity=normal"
```

**Expected Output**:
```
Passed!  - Failed:  0, Passed:  X, Skipped:  0, Total:  X
```

### Integration Tests (Contract)
```powershell
cd src/Biotrackr.Food.Api/Biotrackr.Food.Api.IntegrationTests
dotnet test --filter "FullyQualifiedName~Contract" --logger "console;verbosity=normal"
```

### Integration Tests (E2E)
**Prerequisites**: Cosmos DB Emulator must be running

```powershell
# Start emulator first
.\cosmos-emulator.ps1

# Run E2E tests
cd src/Biotrackr.Food.Api/Biotrackr.Food.Api.IntegrationTests
dotnet test --filter "FullyQualifiedName~E2E" --logger "console;verbosity=normal"
```

### All Tests with Coverage
```powershell
cd src/Biotrackr.Food.Api
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

**View Coverage Report**:
Coverage results will be in `TestResults/` directory as XML files.

---

## Development Workflow

### 1. Create a Feature Branch
```powershell
git checkout -b feature/my-food-api-feature
```

### 2. Make Changes
Edit files in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/`

### 3. Run Tests
```powershell
dotnet test
```

### 4. Run API Locally
```powershell
cd Biotrackr.Food.Api
dotnet run
```

### 5. Test Manually
```powershell
# Via Swagger UI
Start-Process http://localhost:8080/swagger

# Via curl
curl http://localhost:8080/
curl http://localhost:8080/2025-11-11
curl http://localhost:8080/range/2025-11-01/2025-11-30
```

### 6. Commit Changes
```powershell
git add .
git commit -m "feat: add new food API feature"
git push origin feature/my-food-api-feature
```

---

## Common Tasks

### Add Sample Data to Cosmos DB
Use Azure Cosmos DB Emulator Data Explorer:

1. Open browser: `https://localhost:8081/_explorer/index.html`
2. Accept self-signed certificate warning
3. Navigate to `Biotrackr` > `HealthData`
4. Click "New Item"
5. Paste sample JSON:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "documentType": "FoodLog",
  "userId": "testuser",
  "date": "2025-11-11",
  "summary": {
    "calories": 2500,
    "caloriesIn": 2200,
    "water": 2000
  },
  "goals": {
    "calories": 2400
  }
}
```
6. Click "Save"

### View Logs
```powershell
# API logs (while running)
# Console output shows request logs

# Enable detailed logging
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

### Debug in Visual Studio
1. Open `Biotrackr.Food.Api.sln` in Visual Studio 2022
2. Set `Biotrackr.Food.Api` as startup project
3. Press F5 to debug
4. Set breakpoints in endpoint handlers or repository

### Debug in VS Code
1. Open `src/Biotrackr.Food.Api/` folder in VS Code
2. Create `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/Biotrackr.Food.Api/bin/Debug/net9.0/Biotrackr.Food.Api.dll",
      "args": [],
      "cwd": "${workspaceFolder}/Biotrackr.Food.Api",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```
3. Press F5 to debug

---

## Troubleshooting

### Cosmos DB Emulator Not Starting
**Symptoms**: API fails to connect, "unable to resolve service endpoint"

**Solutions**:
```powershell
# Stop and restart emulator
docker stop $(docker ps -q --filter ancestor=mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator)
.\cosmos-emulator.ps1

# Check emulator status
docker ps

# View emulator logs
docker logs <container-id>
```

### Port Already in Use (8080)
**Symptoms**: `Address already in use` error when starting API

**Solutions**:
```powershell
# Find process using port 8080
Get-NetTCPConnection -LocalPort 8080

# Kill process
Stop-Process -Id <PID>

# Or use a different port in launchSettings.json
```

### Tests Failing in Integration Tests
**Symptoms**: E2E tests fail with connection errors

**Solutions**:
1. Ensure Cosmos DB Emulator is running
2. Check emulator is accessible: `curl https://localhost:8081/_explorer/index.html` (ignore cert warning)
3. Verify test configuration in `appsettings.Test.json`
4. Clear test container data between runs

### Build Errors
**Symptoms**: Compilation errors, missing dependencies

**Solutions**:
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build

# Update NuGet packages (if needed)
dotnet list package --outdated
dotnet add package <PackageName> --version <Version>
```

---

## API Endpoints Reference

### Base URL
```
Local: http://localhost:8080
Production: https://{apimGateway}/food
```

### Endpoints

#### GET /
Get all food logs (paginated)

**Query Parameters**:
- `pageNumber` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 20, max: 100)

**Example**:
```powershell
curl "http://localhost:8080/?pageNumber=1&pageSize=20"
```

#### GET /{date}
Get food log for specific date

**Path Parameter**:
- `date`: Date in YYYY-MM-DD format

**Example**:
```powershell
curl "http://localhost:8080/2025-11-11"
```

#### GET /range/{startDate}/{endDate}
Get food logs in date range (paginated)

**Path Parameters**:
- `startDate`: Start date (YYYY-MM-DD, inclusive)
- `endDate`: End date (YYYY-MM-DD, inclusive)

**Query Parameters**: Same as `GET /`

**Example**:
```powershell
curl "http://localhost:8080/range/2025-11-01/2025-11-30?pageNumber=1&pageSize=25"
```

#### GET /healthz/liveness
Health check endpoint

**Example**:
```powershell
curl "http://localhost:8080/healthz/liveness"
```

---

## Next Steps

### For New Features
1. Review [spec.md](spec.md) for requirements
2. Review [data-model.md](data-model.md) for entity definitions
3. Review [contracts/openapi.yaml](contracts/openapi.yaml) for API contract
4. Follow TDD: Write tests first, then implementation
5. Ensure ≥70% code coverage

### For Infrastructure
1. Review [plan.md](plan.md) for Bicep templates
2. Deploy to dev environment: See `.github/workflows/deploy-food-api.yml`
3. Test via APIM gateway

### For Documentation
- Update OpenAPI spec if adding endpoints
- Update data model if changing entities
- Update this quickstart if setup changes

---

## Useful Links

### Project Documentation
- [Feature Specification](spec.md)
- [Implementation Plan](plan.md)
- [Research Findings](research.md)
- [Data Model](data-model.md)
- [OpenAPI Contract](contracts/openapi.yaml)

### External Resources
- [.NET 9.0 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Azure Cosmos DB Emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/emulator)
- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)

### Repository Specific
- [Cosmos Emulator Setup](../../docs/cosmos-emulator-setup.md)
- [GitHub Workflow Templates](../../docs/github-workflow-templates.md)
- [Decision Records](../../docs/decision-records/)

---

**Happy Coding!** 🚀

For questions or issues, create a GitHub Issue in the [biotrackr repository](https://github.com/willvelida/biotrackr/issues).
