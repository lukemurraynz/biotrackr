# Tasks: Food API with Full Test Coverage and Infrastructure

**Input**: Design documents from `/specs/012-food-api/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml

**Tests**: Tests are REQUIRED per Constitution Principle II - comprehensive testing with test pyramid (unit ≥70%, contract, E2E).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

Project uses web API structure:
- Main API: `src/Biotrackr.Food.Api/Biotrackr.Food.Api/`
- Unit Tests: `src/Biotrackr.Food.Api/Biotrackr.Food.Api.UnitTests/`
- Integration Tests: `src/Biotrackr.Food.Api/Biotrackr.Food.Api.IntegrationTests/`
- Infrastructure: `infra/apps/food-api/`
- Workflow: `.github/workflows/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create solution directory at `src/Biotrackr.Food.Api/`
- [X] T002 Create solution file `src/Biotrackr.Food.Api/Biotrackr.Food.Api.sln`
- [X] T003 Create main API project at `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` with .NET 9.0 Web SDK
- [X] T004 Create unit test project at `src/Biotrackr.Food.Api/Biotrackr.Food.Api.UnitTests/` with .NET 9.0 SDK
- [X] T005 Create integration test project at `src/Biotrackr.Food.Api/Biotrackr.Food.Api.IntegrationTests/` with .NET 9.0 SDK
- [X] T006 [P] Add NuGet packages to main API project: Azure.Identity 1.14.1, Microsoft.Azure.Cosmos 3.52.0, Microsoft.Azure.AppConfiguration.AspNetCore 8.2.0, Swashbuckle.AspNetCore 9.0.1
- [X] T007 [P] Add NuGet packages to unit test project: xUnit 2.9.3, FluentAssertions 8.4.0, Moq 4.20.72, AutoFixture 4.18.1, coverlet.collector 6.0.4, ILogger.Moq 2.0.0
- [X] T008 [P] Add NuGet packages to integration test project: xUnit 2.9.3, FluentAssertions 8.4.0, Microsoft.AspNetCore.Mvc.Testing 9.0.0, coverlet.collector 6.0.4, AutoFixture 4.18.1, Azure.Identity 1.17.0
- [X] T009 Add project references: unit and integration tests reference main API project
- [X] T010 [P] Create `appsettings.json` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` with base configuration
- [X] T011 [P] Create `appsettings.Development.json` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` with Cosmos DB Emulator settings
- [X] T012 [P] Create `appsettings.Test.json` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api.IntegrationTests/` with test configuration
- [X] T013 [P] Create `Dockerfile` in `src/Biotrackr.Food.Api/` for container build
- [X] T014 [P] Create `.dockerignore` in `src/Biotrackr.Food.Api/` to exclude unnecessary files from Docker context

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T015 Create `Configuration/CosmosDbSettings.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` with Cosmos DB configuration model
- [X] T016 [P] Create `Models/FoodLog.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` with FoodLog entity (id, documentType, userId, date, summary, goals, ttl)
- [X] T017 [P] Create `Models/FoodLogSummary.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` with nutrition totals (calories, caloriesIn, water)
- [X] T018 [P] Create `Models/FoodLogGoals.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` with calorie goal
- [X] T019 [P] Create `Models/FoodLogResponse.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` for single food log API response
- [X] T020 [P] Create `Models/PaginatedFoodLogsResponse.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` with pagination metadata
- [X] T021 [P] Create `Models/ErrorResponse.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` for standardized error responses
- [X] T022 Create `Repositories/ICosmosRepository.cs` interface in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` with methods: GetAllFoodLogsAsync, GetFoodLogByDateAsync, GetFoodLogsByDateRangeAsync
- [X] T023 Implement `Repositories/CosmosRepository.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` with Cosmos DB SDK integration, partition key filtering, camelCase serialization
- [X] T024 Create `Extensions/ServiceCollectionExtensions.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` for DI registration (CosmosClient as Singleton, Repository as Scoped)
- [X] T025 Create `Extensions/WebApplicationExtensions.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` for endpoint mapping skeleton
- [X] T026 Create `Program.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` with minimal API setup, Azure App Configuration, Swagger, health checks, [ExcludeFromCodeCoverage] attribute

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Query Food Logs via RESTful API (Priority: P1) 🎯 MVP

**Goal**: Expose RESTful endpoints for querying food and nutrition data (GET /, GET /{date}, GET /range, GET /healthz/liveness)

**Independent Test**: Make HTTP requests to all endpoints, validate response structure, status codes, pagination, date validation, error handling

### Tests for User Story 1 (TDD: Write tests FIRST, ensure they FAIL) ⚠️

#### Unit Tests
- [X] T027 [P] [US1] Create `EndpointHandlers/GetAllFoodLogsHandlerTests.cs` in unit tests - test success with data, empty result, pagination validation (pageSize > 100, < 1), repository exceptions
- [X] T028 [P] [US1] Create `EndpointHandlers/GetFoodLogByDateHandlerTests.cs` in unit tests - test success with data, null result (date not found), invalid date format, repository exceptions
- [X] T029 [P] [US1] Create `EndpointHandlers/GetFoodLogsByDateRangeHandlerTests.cs` in unit tests - test success with data, empty range, invalid date format, startDate > endDate, pagination validation, repository exceptions
- [ ] T030 [P] [US1] Create `EndpointHandlers/HealthCheckHandlerTests.cs` in unit tests - test returns 200 OK with "Healthy" status
- [X] T031 [P] [US1] Create `Repositories/CosmosRepositoryTests.cs` in unit tests - test GetAllFoodLogsAsync, GetFoodLogByDateAsync, GetFoodLogsByDateRangeAsync with mocked CosmosClient

#### Integration Contract Tests
- [X] T032 [P] [US1] Create `Contract/ProgramStartupTests.cs` in integration tests - verify app starts without exceptions, services resolve correctly
- [X] T033 [P] [US1] Create `Contract/ServiceRegistrationTests.cs` in integration tests - verify CosmosClient (Singleton), ICosmosRepository (Scoped), all handlers registered
- [X] T034 [P] [US1] Create `Contract/ApiSmokeTests.cs` in integration tests - verify all endpoints return expected status codes without Cosmos DB dependency

#### Integration E2E Tests
- [X] T035 [P] [US1] Create `E2E/Fixtures/FoodApiWebApplicationFactory.cs` in integration tests - setup WebApplicationFactory with Cosmos DB Emulator connection, Gateway mode, test data seeding
- [X] T036 [US1] Create `E2E/FoodEndpointsTests.cs` in integration tests - full HTTP request/response tests: GET /, GET /{date}, GET /range, health check, pagination, error scenarios (requires T035)

### Implementation for User Story 1

- [X] T037 [P] [US1] Create `EndpointHandlers/GetAllFoodLogsHandler.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` - implement pagination logic (default pageSize 20, max 100), call repository, map to PaginatedFoodLogsResponse
- [X] T038 [P] [US1] Create `EndpointHandlers/GetFoodLogByDateHandler.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` - validate YYYY-MM-DD format, call repository, return FoodLogResponse or null
- [X] T039 [P] [US1] Create `EndpointHandlers/GetFoodLogsByDateRangeHandler.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` - validate date range (startDate <= endDate), implement pagination, call repository
- [X] T040 [P] [US1] Create `EndpointHandlers/HealthCheckHandler.cs` in `src/Biotrackr.Food.Api/Biotrackr.Food.Api/` - return 200 OK with simple status response
- [X] T041 [US1] Update `Extensions/WebApplicationExtensions.cs` - register all endpoints: MapGet("/"), MapGet("/{date}"), MapGet("/range/{startDate}/{endDate}"), MapGet("/healthz/liveness")
- [X] T042 [US1] Add error handling middleware in Program.cs for 400/500/503 responses with ErrorResponse model
- [X] T043 [US1] Add request logging in Program.cs using ILogger

**Checkpoint**: User Story 1 complete - API endpoints functional and fully tested (≥70% coverage for handlers and repository)

---

## Phase 4: User Story 2 - Comprehensive Test Coverage for Quality Assurance (Priority: P1)

**Goal**: Achieve ≥70% code coverage, ensure all tests pass in local and CI/CD environments

**Independent Test**: Run test suite locally and in CI/CD, verify coverage reports, confirm zero flaky tests

### Implementation for User Story 2

- [ ] T044 [P] [US2] Create `Extensions/ServiceCollectionExtensionsTests.cs` in unit tests - verify all service registrations (CosmosClient, Repository, Configuration)
- [ ] T045 [P] [US2] Create `Extensions/WebApplicationExtensionsTests.cs` in unit tests - verify all endpoints are mapped correctly
- [ ] T046 [US2] Run all unit tests locally: `dotnet test --filter FullyQualifiedName!~E2E` and verify ≥70% coverage excluding Program.cs
- [ ] T047 [US2] Run contract tests locally: `dotnet test --filter FullyQualifiedName~Contract` and verify all pass without external dependencies
- [ ] T048 [US2] Start Cosmos DB Emulator, run E2E tests locally: `dotnet test --filter FullyQualifiedName~E2E`, verify all pass with real database
- [ ] T049 [US2] Generate coverage report: `dotnet test --collect:"XPlat Code Coverage"` and verify ≥70% threshold
- [ ] T050 [US2] Review coverage report, identify gaps, add missing unit tests for uncovered branches/edge cases
- [ ] T051 [US2] Add test documentation in `src/Biotrackr.Food.Api/Biotrackr.Food.Api.IntegrationTests/README.md` - explain Contract vs E2E tests, how to run, Cosmos DB Emulator setup

**Checkpoint**: Test suite complete with ≥70% coverage, all tests passing consistently

---

## Phase 5: User Story 3 - Infrastructure as Code for Automated Deployment (Priority: P2)

**Goal**: Create Bicep templates for Container App, APIM integration, App Configuration

**Independent Test**: Lint Bicep, validate with parameters, run what-if analysis, deploy to dev environment, verify API accessible via APIM

### Implementation for User Story 3

- [ ] T052 [P] [US3] Create `infra/apps/food-api/main.bicep` - define Container App resource following `infra/apps/weight-api/main.bicep` pattern
- [ ] T053 [P] [US3] Create `infra/apps/food-api/main.dev.bicepparam` - dev environment parameters (image tag, environment variables, managed identity)
- [ ] T054 [US3] Add Container App configuration in main.bicep: target port 8080, health probe for `/healthz/liveness`, environment variables (azureappconfigendpoint, managedidentityclientid, cosmosdbendpoint)
- [ ] T055 [US3] Add user-assigned managed identity configuration in main.bicep for Cosmos DB authentication
- [ ] T056 [US3] Add APIM integration in main.bicep: API path `/food`, operations (GetAllFoodLogs, GetFoodLogByDate, GetFoodLogsByDateRange, LivenessCheck), subscription required
- [ ] T057 [US3] Add APIM product creation in main.bicep: product name "Food", subscription requirements enabled
- [ ] T058 [US3] Add App Configuration key-value in main.bicep: `Biotrackr:FoodApiUrl` = `{apimGatewayUrl}/food`
- [ ] T059 [US3] Run Bicep linter: `az bicep lint --file infra/apps/food-api/main.bicep` and fix any warnings
- [ ] T060 [US3] Run Bicep validation: `az deployment group validate --template-file infra/apps/food-api/main.bicep --parameters infra/apps/food-api/main.dev.bicepparam` 
- [ ] T061 [US3] Run what-if analysis: `az deployment group what-if --template-file infra/apps/food-api/main.bicep --parameters infra/apps/food-api/main.dev.bicepparam` and review expected changes
- [ ] T062 [US3] Deploy to dev environment: `az deployment group create --template-file infra/apps/food-api/main.bicep --parameters infra/apps/food-api/main.dev.bicepparam`
- [ ] T063 [US3] Verify Container App is running: `az containerapp show --name food-api --resource-group biotrackr-dev`
- [ ] T064 [US3] Test API via APIM: `curl https://{apimGateway}/food/healthz/liveness` with subscription key

**Checkpoint**: Infrastructure deployed to dev environment, API accessible via APIM gateway

---

## Phase 6: User Story 4 - CI/CD Pipeline for Automated Testing and Deployment (Priority: P2)

**Goal**: Automate testing, building, and deployment via GitHub Actions workflow

**Independent Test**: Trigger workflow on pull request, verify all jobs execute successfully, confirm deployed API is functional

### Implementation for User Story 4

- [ ] T065 [US4] Create `.github/workflows/deploy-food-api.yml` following `.github/workflows/deploy-weight-api.yml` pattern
- [ ] T066 [US4] Add workflow triggers: pull_request to main for paths `src/Biotrackr.Food.Api/**` and `infra/apps/food-api/**`
- [ ] T067 [US4] Add workflow permissions: `contents: read`, `id-token: write`, `pull-requests: write`, `checks: write` (required for test-reporter)
- [ ] T068 [US4] Add env-setup job: configure .NET 9.0, checkout code
- [ ] T069 [US4] Add run-unit-tests job: call reusable template `.github/workflows/template-dotnet-run-unit-tests.yml`, working directory `src/Biotrackr.Food.Api/Biotrackr.Food.Api.UnitTests`, coverage threshold 70%
- [ ] T070 [US4] Add run-contract-tests job: call reusable template with filter `FullyQualifiedName~Contract`, working directory `src/Biotrackr.Food.Api/Biotrackr.Food.Api.IntegrationTests`
- [ ] T071 [US4] Add build-container-image-dev job: build Docker image, push to ACR with tag, use Dockerfile at `src/Biotrackr.Food.Api/Dockerfile`
- [ ] T072 [US4] Add lint job: run `az bicep lint` on `infra/apps/food-api/main.bicep`
- [ ] T073 [US4] Add validate job: run `az deployment group validate` with `main.bicep` and `main.dev.bicepparam`
- [ ] T074 [US4] Add preview job: run `az deployment group what-if` to show infrastructure changes
- [ ] T075 [US4] Add deploy-dev job: deploy Bicep template to dev environment, update Container App with new image
- [ ] T076 [US4] Add run-e2e-tests job: start Cosmos DB Emulator, run E2E tests against deployed API, filter `FullyQualifiedName~E2E`
- [ ] T077 [US4] Add test result publishing: use `dorny/test-reporter@v1` to publish unit, contract, and E2E test results to pull request
- [ ] T078 [US4] Test workflow locally: create feature branch, make small change, push, verify workflow triggers and all jobs pass
- [ ] T079 [US4] Verify workflow fails when coverage < 70%: temporarily reduce test coverage, push, confirm workflow fails with clear error message
- [ ] T080 [US4] Verify workflow fails when tests fail: break a test, push, confirm workflow fails and reports specific test failure

**Checkpoint**: CI/CD pipeline functional, all quality gates enforced, automatic deployment to dev

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories, documentation, final validation

- [ ] T081 [P] Update `README.md` in repository root with Food API description, links to documentation
- [ ] T082 [P] Verify OpenAPI documentation at `/swagger` is accurate and complete for all endpoints
- [ ] T083 [P] Review all error messages for clarity and consistency (400/500/503 responses)
- [ ] T084 Code review: verify SOLID principles, no code duplication, clear naming conventions
- [ ] T085 Security review: verify Managed Identity is used in production, no secrets in code/config, TLS everywhere
- [ ] T086 Performance review: verify Singleton CosmosClient, partition key in all queries, async/await throughout
- [ ] T087 [P] Add XML documentation comments to public APIs in endpoint handlers and repository
- [ ] T088 Run `quickstart.md` validation: follow all steps in quickstart guide, ensure no errors
- [ ] T089 [P] Create developer onboarding checklist based on quickstart experience
- [ ] T090 Final test run: execute all tests (unit, contract, E2E) locally, verify 100% pass rate
- [ ] T091 Final deployment: merge to main, verify workflow deploys to dev, test via APIM, validate all endpoints

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (T015-T026) - Core API implementation
- **User Story 2 (Phase 4)**: Depends on User Story 1 (T027-T043) - Tests validate US1 implementation
- **User Story 3 (Phase 5)**: Can start in parallel with US1/US2 after Foundational - Infrastructure is independent
- **User Story 4 (Phase 6)**: Depends on US1, US2, US3 completion - CI/CD requires all components
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational - No dependencies on other stories
- **User Story 2 (P1)**: Depends on User Story 1 - Tests validate US1 implementation
- **User Story 3 (P2)**: Independent of US1/US2 after Foundational - Can work in parallel
- **User Story 4 (P2)**: Depends on US1, US2, US3 - Integrates all previous work

### Within Each User Story

**User Story 1**:
1. Write tests first (T027-T036) - ensure they FAIL
2. Implement handlers (T037-T040) - can be parallel
3. Wire up endpoints (T041)
4. Add error handling (T042-T043)
5. Run tests - should now PASS

**User Story 2**:
1. Add extension tests (T044-T045) - parallel
2. Run all test suites (T046-T048)
3. Generate coverage report (T049)
4. Fill coverage gaps (T050)
5. Document test approach (T051)

**User Story 3**:
1. Create Bicep templates (T052-T053) - parallel
2. Add configurations (T054-T058) - sequential
3. Validate (T059-T061)
4. Deploy (T062)
5. Verify (T063-T064)

**User Story 4**:
1. Create workflow (T065)
2. Add triggers/permissions (T066-T067)
3. Add jobs (T068-T076) - can define in parallel
4. Add test reporting (T077)
5. Test workflow (T078-T080)

### Parallel Opportunities

**Phase 1 (Setup)**: T006, T007, T008, T010, T011, T012, T013, T014 can run in parallel

**Phase 2 (Foundational)**: T016, T017, T018, T019, T020, T021 (models) can run in parallel

**Phase 3 (User Story 1)**:
- Tests: T027, T028, T029, T030, T031, T032, T033, T034, T035 can run in parallel
- Handlers: T037, T038, T039, T040 can run in parallel

**Phase 4 (User Story 2)**: T044, T045 can run in parallel

**Phase 5 (User Story 3)**: T052, T053 can run in parallel

**Phase 7 (Polish)**: T081, T082, T083, T087, T089 can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all unit test files together:
Task: "Create GetAllFoodLogsHandlerTests.cs" [T027]
Task: "Create GetFoodLogByDateHandlerTests.cs" [T028]
Task: "Create GetFoodLogsByDateRangeHandlerTests.cs" [T029]
Task: "Create HealthCheckHandlerTests.cs" [T030]
Task: "Create CosmosRepositoryTests.cs" [T031]

# Launch all contract tests together:
Task: "Create ProgramStartupTests.cs" [T032]
Task: "Create ServiceRegistrationTests.cs" [T033]
Task: "Create ApiSmokeTests.cs" [T034]

# Launch all handlers together:
Task: "Create GetAllFoodLogsHandler.cs" [T037]
Task: "Create GetFoodLogByDateHandler.cs" [T038]
Task: "Create GetFoodLogsByDateRangeHandler.cs" [T039]
Task: "Create HealthCheckHandler.cs" [T040]
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup (T001-T014)
2. Complete Phase 2: Foundational (T015-T026) - CRITICAL checkpoint
3. Complete Phase 3: User Story 1 (T027-T043) - API endpoints
4. Complete Phase 4: User Story 2 (T044-T051) - Test coverage
5. **STOP and VALIDATE**: Run all tests, verify ≥70% coverage, test API manually
6. **MVP READY**: Food API functional locally with comprehensive tests

### Incremental Delivery

1. **Setup + Foundational** (T001-T026) → Foundation ready
2. **Add User Story 1 + 2** (T027-T051) → Test independently → **MVP achieved!**
3. **Add User Story 3** (T052-T064) → Deploy to Azure → API accessible via APIM
4. **Add User Story 4** (T065-T080) → CI/CD automation → Full DevOps pipeline
5. **Polish** (T081-T091) → Production-ready

### Parallel Team Strategy

With multiple developers after Foundational phase (T015-T026):

- **Developer A**: User Story 1 - API implementation (T027-T043)
- **Developer B**: User Story 3 - Infrastructure (T052-T064, can start in parallel)
- **Developer C**: User Story 2 - Test coverage (T044-T051, after US1 complete)
- **Developer D**: User Story 4 - CI/CD (T065-T080, after US1/US2/US3 complete)

---

## Task Summary

**Total Tasks**: 91 tasks  
**Parallelizable Tasks**: 30 tasks (33%)

**Task Count by Phase**:
- Phase 1 (Setup): 14 tasks
- Phase 2 (Foundational): 12 tasks
- Phase 3 (User Story 1): 17 tasks
- Phase 4 (User Story 2): 8 tasks
- Phase 5 (User Story 3): 13 tasks
- Phase 6 (User Story 4): 16 tasks
- Phase 7 (Polish): 11 tasks

**MVP Scope** (User Stories 1 + 2): 43 tasks (47% of total)

**Independent Test Criteria**:
- **US1**: Make HTTP requests to all endpoints, validate responses
- **US2**: Run test suite, verify ≥70% coverage, all tests pass
- **US3**: Deploy Bicep, verify API accessible via APIM
- **US4**: Trigger workflow, verify all jobs pass, API deploys successfully

---

## Notes

- All tasks follow strict checklist format: `- [ ] [ID] [P?] [Story?] Description with file path`
- [P] tasks = different files, no dependencies, can run in parallel
- [Story] labels (US1, US2, US3, US4) map tasks to user stories for traceability
- TDD approach: Write tests first (ensure they fail), then implement (tests pass)
- Verify tests fail before implementing to ensure tests are valid
- Commit after each task or logical group of related tasks
- Stop at checkpoints to validate each story independently
- MVP = User Stories 1 + 2 (API + Tests), then add infrastructure and CI/CD incrementally
