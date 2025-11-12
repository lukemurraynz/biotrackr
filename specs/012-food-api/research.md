# Research: Food API Implementation

**Feature**: Food API with Full Test Coverage and Infrastructure  
**Date**: 2025-11-11  
**Status**: Complete

## Overview

This document consolidates research findings for implementing the Food API, following established patterns from Weight, Activity, and Sleep APIs. All technical decisions align with proven approaches in the biotrackr codebase.

---

## Technical Decisions

### 1. API Framework: ASP.NET Core Minimal APIs

**Decision**: Use ASP.NET Core 9.0 Minimal APIs with endpoint handlers

**Rationale**:
- Consistent with existing Weight, Activity, and Sleep APIs in the codebase
- Minimal APIs provide lightweight, performant HTTP endpoints
- Endpoint handler pattern separates HTTP concerns from business logic
- Built-in dependency injection, middleware pipeline, and OpenAPI support
- Proven performance characteristics for simple REST APIs

**Alternatives Considered**:
- **Controller-based APIs**: More ceremony than needed for simple REST endpoints, Minimal APIs are lighter
- **Azure Functions**: Would break consistency with existing API architecture, Container Apps better for sustained workloads
- **ASP.NET Core Web API (traditional)**: Minimal APIs achieve same functionality with less boilerplate

**References**:
- `src/Biotrackr.Weight.Api/Biotrackr.Weight.Api/Program.cs` - Example Minimal API setup
- `src/Biotrackr.Activity.Api/` - Established endpoint handler pattern

---

### 2. Data Access Pattern: Repository Pattern with Cosmos DB SDK

**Decision**: Implement `ICosmosRepository` interface with `CosmosRepository` implementation using Azure Cosmos DB SDK

**Rationale**:
- Consistent with all existing APIs (Weight, Activity, Sleep, Auth, Food Service)
- Provides abstraction over Cosmos DB SDK for testability
- Enables unit testing with mocked repositories
- Single Responsibility Principle: repository handles data access, handlers handle HTTP
- Proven pattern across entire biotrackr codebase

**Implementation Details**:
- `ICosmosRepository`: Generic interface for CRUD operations
- `CosmosRepository`: Concrete implementation using `CosmosClient`
- Singleton `CosmosClient` for connection pooling (per existing pattern)
- CamelCase JSON serialization for consistency

**Alternatives Considered**:
- **Direct CosmosClient usage in handlers**: Violates SRP, harder to test, rejected for maintainability
- **Entity Framework Core**: Not supported for Cosmos DB NoSQL API, N/A
- **Custom ORM**: Unnecessary complexity, SDK provides sufficient functionality

**References**:
- `src/Biotrackr.Weight.Api/Biotrackr.Weight.Api/Repositories/CosmosRepository.cs`
- `src/Biotrackr.Activity.Api/Biotrackr.Activity.Api/Repositories/CosmosRepository.cs`
- [Service Lifetime Registration Decision Record](../../docs/decision-records/2025-10-28-service-lifetime-registration.md)

---

### 3. Authentication: Azure Managed Identity (Production) + Account Key (Local/Test)

**Decision**: Use `DefaultAzureCredential` for production (Managed Identity), fallback to account key for local development

**Rationale**:
- Consistent with all existing services and APIs
- Managed Identity eliminates secrets management in production
- Account key enables local development without Azure authentication
- `DefaultAzureCredential` automatically handles both scenarios
- Follows Azure security best practices

**Implementation**:
```csharp
// Production: Uses Managed Identity
var cosmosClient = new CosmosClient(endpoint, new DefaultAzureCredential());

// Local/Test: Uses account key from configuration
var cosmosClient = new CosmosClient(endpoint, accountKey);
```

**Alternatives Considered**:
- **Connection strings only**: Less secure than Managed Identity, rejected for production
- **Service Principal**: More complex than Managed Identity, unnecessary overhead
- **Always require Managed Identity**: Breaks local development workflow, rejected

**References**:
- All existing API `Program.cs` files use this pattern
- [Azure Identity Best Practices](https://learn.microsoft.com/en-us/dotnet/api/azure.identity)

---

### 4. Configuration: Azure App Configuration + appsettings.json

**Decision**: Use Azure App Configuration for centralized settings, with appsettings.json for local overrides

**Rationale**:
- Consistent with entire biotrackr infrastructure
- Centralized configuration management across all services
- Feature flags and dynamic configuration updates without redeployment
- Environment-specific settings via labels (dev, staging, prod)
- Local development supported via appsettings.Development.json

**Configuration Structure**:
- `Biotrackr:CosmosDb:Endpoint` - Cosmos DB endpoint URL
- `Biotrackr:CosmosDb:DatabaseName` - Database name ("Biotrackr")
- `Biotrackr:CosmosDb:ContainerName` - Container name ("HealthData")
- `Biotrackr:CosmosDb:PartitionKeyPath` - Partition key ("/documentType")
- `Biotrackr:FoodApiUrl` - Full API URL via APIM (set by infra)

**Alternatives Considered**:
- **appsettings.json only**: No centralized management, requires redeployment for config changes
- **Key Vault only**: More complex than needed for non-secret configuration
- **Environment variables only**: Less structured, harder to manage across services

**References**:
- `infra/modules/configuration/app-configuration.bicep`
- All existing API `Program.cs` files use App Configuration

---

### 5. Testing Strategy: Test Pyramid (Unit 70% + Contract + E2E)

**Decision**: Three-layer test strategy with xUnit, FluentAssertions, Moq, and Cosmos DB Emulator

**Test Layers**:
1. **Unit Tests** (≥70% coverage):
   - Test handlers with mocked dependencies (ICosmosRepository)
   - Test repository with mocked CosmosClient
   - Test extensions and configuration
   - Fast, isolated, no external dependencies

2. **Contract Tests** (Integration - no external deps):
   - `ProgramStartupTests`: Verify app starts without exceptions
   - `ServiceRegistrationTests`: Verify all services registered with correct lifetimes
   - `ApiSmokeTests`: Basic endpoint availability
   - Fast, validate configuration and DI setup

3. **E2E Tests** (Integration - with Cosmos DB Emulator):
   - Full HTTP request/response cycles
   - Real Cosmos DB queries via Emulator
   - Pagination, filtering, error scenarios
   - Validate complete feature behavior

**Rationale**:
- Aligns with established testing patterns across all APIs
- Test pyramid optimizes for fast feedback (unit) and confidence (E2E)
- ≥70% coverage ensures quality while being pragmatic
- Cosmos DB Emulator enables true integration testing locally and in CI/CD

**Tooling**:
- **xUnit 2.9.3**: Test framework (consistent across codebase)
- **FluentAssertions 8.4.0**: Readable assertions
- **Moq 4.20.72**: Mocking framework for unit tests
- **AutoFixture 4.18.1**: Test data generation
- **Microsoft.AspNetCore.Mvc.Testing 9.0.0**: WebApplicationFactory for integration tests
- **coverlet.collector 6.0.4**: Code coverage collection

**Alternatives Considered**:
- **100% coverage target**: Diminishing returns, increases maintenance burden
- **Only E2E tests**: Slower feedback loop, harder to diagnose failures
- **No Cosmos DB Emulator**: Would require mocking in E2E tests, less realistic

**References**:
- [Integration Test Project Structure](../../docs/decision-records/2025-10-28-integration-test-project-structure.md)
- [Contract Test Architecture](../../docs/decision-records/2025-10-28-contract-test-architecture.md)
- All existing API test projects follow this pattern

---

### 6. Infrastructure as Code: Bicep for Azure Container Apps + APIM

**Decision**: Use Bicep templates for declarative infrastructure, reusing established modules

**Infrastructure Components**:
- **Container App**: Host the Food API (Linux container, port 8080)
- **APIM Integration**: Expose API at `/food` path with operations and subscription
- **APIM Product**: Create "Food" product for subscription management
- **App Configuration**: Register `Biotrackr:FoodApiUrl` key-value
- **User-Assigned Managed Identity**: For Cosmos DB authentication

**Rationale**:
- Consistent with all existing API infrastructure
- Version-controlled infrastructure changes
- Reusable Bicep modules reduce duplication
- Automated deployments via GitHub Actions
- Preview changes with what-if analysis before deployment

**Reusable Modules**:
- `infra/modules/host/container-app-http.bicep`: Container App deployment
- `infra/modules/apim/apim-products.bicep`: APIM product creation
- `infra/modules/configuration/app-configuration-keys.bicep`: Config key-values

**Alternatives Considered**:
- **ARM templates**: More verbose than Bicep, less readable
- **Terraform**: Introduces new tooling, Bicep is Azure-native and already in use
- **Manual Azure Portal**: Not version-controlled, error-prone, not repeatable

**References**:
- `infra/apps/weight-api/main.bicep` - Reference implementation
- `infra/apps/activity-api/main.bicep` - Reference implementation
- [Bicep Modules Structure](../../docs/bicep-modules-structure.md)

---

### 7. CI/CD Pipeline: GitHub Actions with Multi-Stage Workflow

**Decision**: Multi-stage GitHub Actions workflow for testing, building, and deploying

**Workflow Stages**:
1. **env-setup**: Configure .NET 9.0 environment
2. **run-unit-tests**: Execute unit tests, enforce ≥70% coverage
3. **run-contract-tests**: Execute contract tests (fast, parallel with unit tests)
4. **build-container-image-dev**: Build and push Docker image to ACR
5. **lint**: Run Bicep linter on infrastructure templates
6. **validate**: Validate Bicep with parameters
7. **preview**: Generate what-if analysis for infrastructure changes
8. **deploy-dev**: Deploy to dev environment via Bicep
9. **run-e2e-tests**: Execute E2E tests against deployed API

**Rationale**:
- Consistent with existing API workflows
- Fast feedback: unit and contract tests run early in parallel
- Quality gates: coverage threshold, linting, validation before deployment
- Safe deployments: preview changes before applying
- Test results published to pull request via `dorny/test-reporter@v1`

**Workflow Triggers**:
- Pull requests to `main` modifying:
  - `src/Biotrackr.Food.Api/**`
  - `infra/apps/food-api/**`

**Required Permissions**:
```yaml
permissions:
  contents: read
  id-token: write        # Required for Azure authentication
  pull-requests: write   # Required for PR comments
  checks: write          # Required for test-reporter action
```

**Alternatives Considered**:
- **Azure Pipelines**: GitHub Actions already in use, prefer consistency
- **Single-stage deployment**: No quality gates, riskier deployments
- **Manual testing only**: Slow feedback, no automation, error-prone

**References**:
- `.github/workflows/deploy-weight-api.yml` - Reference workflow
- `.github/workflows/template-dotnet-run-unit-tests.yml` - Reusable template
- [GitHub Workflow Templates](../../docs/github-workflow-templates.md)

---

### 8. OpenAPI Documentation: Swashbuckle with Minimal API Support

**Decision**: Use Swashbuckle.AspNetCore 9.0.1 for automatic OpenAPI 3.0 generation

**Rationale**:
- Built-in support for ASP.NET Core Minimal APIs
- Generates interactive Swagger UI at `/swagger`
- Automatic endpoint discovery and schema generation
- Enables API testing and documentation in one place
- Consistent with existing APIs

**Implementation**:
```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

app.UseSwagger();
app.UseSwaggerUI();
```

**Alternatives Considered**:
- **NSwag**: Less commonly used in .NET 9 projects, Swashbuckle is standard
- **Manual OpenAPI spec**: Time-consuming, prone to drift from implementation
- **No documentation**: Unacceptable for API maintainability

**References**:
- All existing API `Program.cs` files use Swashbuckle
- [OpenAPI Specification](https://swagger.io/specification/)

---

### 9. Error Handling: Consistent HTTP Status Codes and Problem Details

**Decision**: Return appropriate HTTP status codes with clear error messages

**Status Code Standards**:
- **200 OK**: Successful query (even if result set is empty)
- **400 Bad Request**: Invalid input (date format, pagination params)
- **404 Not Found**: Endpoint not found (not for empty result sets)
- **500 Internal Server Error**: Unexpected exceptions
- **503 Service Unavailable**: Cosmos DB unreachable

**Error Response Format**:
```json
{
  "error": "Invalid date format. Expected YYYY-MM-DD.",
  "statusCode": 400
}
```

**Rationale**:
- Follows REST best practices
- Consistent with existing APIs
- Clear, actionable error messages for clients
- Proper HTTP semantics (200 for empty results, not 404)

**Alternatives Considered**:
- **Always return 200**: Hides errors, bad REST practice
- **Custom error codes**: Standard HTTP codes are well-understood
- **Exception details in production**: Security risk, rejected

**References**:
- [Backend API Route Structure](../../docs/decision-records/2025-10-28-backend-api-route-structure.md)
- Existing API handler implementations

---

### 10. Pagination: Cursor-less Offset-Based Pagination

**Decision**: Use offset-based pagination with `pageNumber` and `pageSize` parameters

**Pagination Model**:
```csharp
{
  "data": [...],
  "totalCount": 365,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 19
}
```

**Parameters**:
- `pageNumber`: Starting at 1 (default: 1)
- `pageSize`: Items per page (default: 20, max: 100)

**Rationale**:
- Consistent with existing APIs (Weight, Activity, Sleep)
- Simple to implement and understand
- Adequate for expected data volumes (~365 food logs/year)
- Provides navigation metadata (totalPages)

**Query Implementation**:
```sql
SELECT * FROM c 
WHERE c.documentType = 'FoodLog' 
ORDER BY c.date DESC 
OFFSET {(pageNumber - 1) * pageSize} LIMIT {pageSize}
```

**Alternatives Considered**:
- **Cursor-based pagination**: More complex, unnecessary for this data volume
- **Continuation tokens**: Cosmos DB native but less user-friendly
- **No pagination**: Would return all items, inefficient for large result sets

**References**:
- Weight API and Activity API use this pagination pattern
- [Cosmos DB OFFSET LIMIT](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/query/offset-limit)

---

## Security Considerations

### Authentication & Authorization
- **Production**: Managed Identity for Cosmos DB (no secrets in code/config)
- **APIM**: Subscription key required for all requests
- **Least Privilege**: Container App identity has minimal permissions (Cosmos DB data access only)

### Data Protection
- **TLS**: All communication encrypted (HTTPS for API, TLS for Cosmos DB)
- **No PII in logs**: Structured logging without sensitive data
- **TTL**: Food logs have TTL set for automatic expiration (data retention)

### Container Security
- **Base Image**: Use official Microsoft images (mcr.microsoft.com)
- **Non-root user**: Container runs as non-root
- **Vulnerability scanning**: Trivy scans in CI/CD (if enabled)

---

## Performance Optimization

### Cosmos DB Query Optimization
- **Partition Key**: Always include `/documentType` in queries for efficient routing
- **Indexes**: Leverage default indexing, add composite index for date + documentType if needed
- **Connection Pooling**: Singleton `CosmosClient` for connection reuse
- **Direct Mode**: Use Direct mode for lower latency (Gateway mode for emulator compatibility)

### API Performance
- **Async/Await**: All I/O operations asynchronous
- **Response Compression**: Enable for large payloads (built-in middleware)
- **Caching**: Consider response caching for frequently accessed dates (future optimization)

### Container App Scaling
- **Horizontal Scaling**: Auto-scale rules based on HTTP request rate
- **Resource Limits**: Right-size CPU/memory based on load testing
- **Health Probes**: Fast health checks for quick failure detection

---

## Testing in CI/CD

### Cosmos DB Emulator Setup
- **Docker Container**: Use `mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest`
- **Connection**: Gateway mode for emulator compatibility
- **Self-Signed Cert**: Trust emulator certificate in test environment

### Test Execution Order
1. Unit tests (fast, parallel)
2. Contract tests (fast, parallel with unit)
3. Build container image (after tests pass)
4. Deploy infrastructure (after build)
5. E2E tests (against deployed API, serial)

### Coverage Reporting
- **Tool**: coverlet.collector
- **Threshold**: ≥70% line coverage
- **Exclusions**: Program.cs (marked with `[ExcludeFromCodeCoverage]`)
- **Publishing**: `dorny/test-reporter@v1` with `checks: write` permission

---

## Monitoring & Observability

### Application Insights (Future)
- Request telemetry (duration, status codes)
- Dependency tracking (Cosmos DB calls)
- Exception tracking
- Custom metrics (cache hit rate, etc.)

### Health Checks
- **Liveness**: `/healthz/liveness` (always returns 200 OK)
- **Readiness** (future): Check Cosmos DB connectivity

### Logging
- Structured logging via `ILogger<T>`
- Log levels: Information (requests), Warning (retries), Error (exceptions)
- Correlation IDs for request tracing

---

## References

### Decision Records
- [Service Lifetime Registration](../../docs/decision-records/2025-10-28-service-lifetime-registration.md)
- [Integration Test Project Structure](../../docs/decision-records/2025-10-28-integration-test-project-structure.md)
- [Contract Test Architecture](../../docs/decision-records/2025-10-28-contract-test-architecture.md)
- [Backend API Route Structure](../../docs/decision-records/2025-10-28-backend-api-route-structure.md)

### Existing Implementations
- `src/Biotrackr.Weight.Api/` - Primary reference for API structure
- `src/Biotrackr.Activity.Api/` - Secondary reference
- `src/Biotrackr.Sleep.Api/` - Tertiary reference
- `infra/apps/weight-api/main.bicep` - Infrastructure reference

### External Documentation
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Azure Cosmos DB SDK for .NET](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/sdk-dotnet-v3)
- [Azure Managed Identity](https://learn.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/)
- [GitHub Actions](https://docs.github.com/en/actions)

---

**Research Complete**: All technical decisions documented with rationale and alternatives considered. Ready to proceed with Phase 1 (Design & Contracts).
