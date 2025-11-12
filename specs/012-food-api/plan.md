# Implementation Plan: Food API with Full Test Coverage and Infrastructure

**Branch**: `012-food-api` | **Date**: 2025-11-11 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/012-food-api/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Create a RESTful API for querying food and nutrition data stored in Cosmos DB, following the established architecture pattern used by Weight, Activity, and Sleep APIs. The Food API exposes endpoints for retrieving food logs by date, date range, and paginated lists, with comprehensive test coverage (≥70%), infrastructure as code (Bicep), and automated CI/CD pipeline (GitHub Actions). The API integrates with Azure App Configuration, supports Managed Identity authentication for production, and is accessible via Azure API Management at the `/food` path.

## Technical Context

**Language/Version**: C# / .NET 9.0  
**Primary Dependencies**: ASP.NET Core 9.0 (Minimal APIs), Azure.Identity 1.14.1, Microsoft.Azure.Cosmos 3.52.0, Microsoft.Azure.AppConfiguration.AspNetCore 8.2.0, Swashbuckle.AspNetCore 9.0.1 (OpenAPI)  
**Storage**: Azure Cosmos DB (existing database `Biotrackr`, container `HealthData`, partition key `/documentType`)  
**Testing**: xUnit 2.9.3, FluentAssertions 8.4.0, Moq 4.20.72, AutoFixture 4.18.1, Microsoft.AspNetCore.Mvc.Testing 9.0.0, coverlet.collector 6.0.4  
**Target Platform**: Linux containers (Azure Container Apps), Docker, deployed via Bicep  
**Project Type**: Web API (single backend service, no frontend)  
**Performance Goals**: <200ms p95 response time for date-based queries, support 100+ concurrent requests  
**Constraints**: ≥70% code coverage (excluding Program.cs), health check <100ms, container image <500MB  
**Scale/Scope**: Single-user health tracking, ~365 food logs/year, paginated responses (default 20, max 100 items)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Code Quality Excellence**: Design follows SOLID principles with clear separation of concerns (endpoint handlers, repository layer, models), minimal cognitive load through consistent naming patterns from existing APIs, single responsibility per class (handlers focus on HTTP, repository handles data access)
- [x] **Testing Strategy**: Test pyramid planned - Unit tests ≥70% coverage (handlers, repository, extensions with mocked dependencies), Integration Contract tests (service registration, API startup without external deps), Integration E2E tests (full request/response with Cosmos DB Emulator), TDD approach recommended for new endpoint handlers
- [x] **User Experience**: Consistent REST API patterns following OpenAPI specification, standardized error handling (400/404/500/503 status codes), clear error messages, pagination metadata in responses, accessible Swagger documentation at `/swagger`
- [x] **Performance Requirements**: Response time <200ms p95 for queries, health check <100ms, 100+ concurrent request support, Cosmos DB connection pooling via Singleton CosmosClient, efficient query patterns with proper partition key usage
- [x] **Technical Debt**: No anticipated debt - follows established patterns from Weight/Activity/Sleep APIs, Repository pattern already validated in existing services, minimal new complexity introduced

## Project Structure

### Documentation (this feature)

```text
specs/012-food-api/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   └── openapi.yaml     # OpenAPI 3.0 specification for Food API endpoints
├── checklists/          # Created during /speckit.specify
│   └── requirements.md  # Specification quality validation checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/Biotrackr.Food.Api/
├── Biotrackr.Food.Api/
│   ├── Biotrackr.Food.Api.csproj
│   ├── Program.cs                      # Application entry point with [ExcludeFromCodeCoverage]
│   ├── appsettings.json                # Base configuration
│   ├── appsettings.Development.json    # Local dev configuration
│   ├── Dockerfile                      # Container image definition
│   ├── .dockerignore
│   ├── Configuration/
│   │   └── CosmosDbSettings.cs         # Cosmos DB configuration model
│   ├── Models/
│   │   ├── FoodLog.cs                  # FoodLog entity model
│   │   ├── FoodLogSummary.cs           # Summary nested model
│   │   ├── FoodLogGoals.cs             # Goals nested model
│   │   ├── FoodLogResponse.cs          # Single food log API response
│   │   └── PaginatedFoodLogsResponse.cs # Paginated collection response
│   ├── Repositories/
│   │   ├── ICosmosRepository.cs        # Repository interface
│   │   └── CosmosRepository.cs         # Cosmos DB data access implementation
│   ├── EndpointHandlers/
│   │   ├── GetAllFoodLogsHandler.cs    # GET / handler with pagination
│   │   ├── GetFoodLogByDateHandler.cs  # GET /{date} handler
│   │   ├── GetFoodLogsByDateRangeHandler.cs # GET /range/{startDate}/{endDate} handler
│   │   └── HealthCheckHandler.cs       # GET /healthz/liveness handler
│   └── Extensions/
│       ├── ServiceCollectionExtensions.cs # DI registration extensions
│       └── WebApplicationExtensions.cs    # Endpoint mapping extensions
├── Biotrackr.Food.Api.UnitTests/
│   ├── Biotrackr.Food.Api.UnitTests.csproj
│   ├── EndpointHandlers/
│   │   ├── GetAllFoodLogsHandlerTests.cs
│   │   ├── GetFoodLogByDateHandlerTests.cs
│   │   ├── GetFoodLogsByDateRangeHandlerTests.cs
│   │   └── HealthCheckHandlerTests.cs
│   ├── Repositories/
│   │   └── CosmosRepositoryTests.cs
│   └── Extensions/
│       ├── ServiceCollectionExtensionsTests.cs
│       └── WebApplicationExtensionsTests.cs
├── Biotrackr.Food.Api.IntegrationTests/
│   ├── Biotrackr.Food.Api.IntegrationTests.csproj
│   ├── appsettings.Test.json           # Test-specific configuration
│   ├── Contract/
│   │   ├── ProgramStartupTests.cs      # Verify app starts without errors
│   │   ├── ServiceRegistrationTests.cs # Verify all services registered correctly
│   │   └── ApiSmokeTests.cs            # Basic endpoint availability checks
│   ├── E2E/
│   │   ├── FoodEndpointsTests.cs       # Full request/response cycle tests
│   │   └── Fixtures/
│   │       └── FoodApiWebApplicationFactory.cs # Test fixture with Cosmos DB Emulator
│   └── README.md                        # Test execution instructions
└── Biotrackr.Food.Api.sln               # Solution file

infra/apps/food-api/
├── main.bicep                           # Container App + APIM integration
└── main.dev.bicepparam                  # Dev environment parameters

.github/workflows/
└── deploy-food-api.yml                  # CI/CD pipeline for Food API
```

**Structure Decision**: Following established web API pattern from Weight/Activity/Sleep APIs. Single backend service (no frontend) with three projects: main API, unit tests, and integration tests. Solution structure mirrors existing APIs for consistency and maintainability. Infrastructure defined in dedicated `infra/apps/food-api/` directory following the apps-per-service pattern.

## Complexity Tracking

> **No violations identified** - All Constitution principles are satisfied. The Food API follows established patterns from existing APIs (Weight, Activity, Sleep), uses proven Repository pattern, and maintains clear separation of concerns. No additional complexity justification required.

---

## Constitution Check Re-Evaluation (Post-Design)

**Date**: 2025-11-11  
**Phase**: After Phase 1 (Design & Contracts)

### ✅ Code Quality Excellence
**Status**: PASS

**Evidence**:
- Data model (`data-model.md`) defines clear entities with single responsibilities
- OpenAPI contract (`contracts/openapi.yaml`) shows clean separation: handlers → repository → Cosmos DB
- No God objects or violation of SRP
- Naming follows established patterns (FoodLog, FoodLogResponse, PaginatedFoodLogsResponse)
- Cognitive load minimized through consistency with existing APIs

### ✅ Testing Strategy
**Status**: PASS

**Evidence**:
- Test pyramid documented in `research.md` section 5
- Unit tests target ≥70% coverage with mocked dependencies
- Contract tests validate DI and configuration (fast, no external deps)
- E2E tests use Cosmos DB Emulator for realistic integration testing
- TDD approach recommended in `quickstart.md`

### ✅ User Experience
**Status**: PASS

**Evidence**:
- OpenAPI 3.0 specification provides consistent API contract
- Error responses documented with clear messages (400/500/503)
- Pagination metadata included in all paginated responses
- Swagger UI accessible at `/swagger` for interactive documentation
- Date formats standardized (YYYY-MM-DD)

### ✅ Performance Requirements
**Status**: PASS

**Evidence**:
- Performance targets defined: <200ms p95, <100ms health check
- Cosmos DB optimization strategies in `research.md` section "Performance Optimization"
- Singleton CosmosClient for connection pooling
- Partition key usage enforced in all queries
- Async/await throughout for non-blocking I/O

### ✅ Technical Debt
**Status**: PASS

**Evidence**:
- No new debt introduced - follows existing patterns
- Repository pattern already proven in 4+ services
- Infrastructure reuses established Bicep modules
- No shortcuts or workarounds required
- All design decisions documented with rationale in `research.md`

### Final Verdict
**✅ ALL GATES PASSED** - Design is ready for implementation (Phase 2: Tasks)
