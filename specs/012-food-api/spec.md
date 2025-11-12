# Feature Specification: Food API with Full Test Coverage and Infrastructure

**Feature Branch**: `012-food-api`  
**Created**: 2025-11-11  
**Status**: Draft  
**Input**: User description: "Create Biotrackr.Food.Api with full test coverage and infrastructure"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Query Food Logs via RESTful API (Priority: P1)

As a developer or application, I want to query food and nutrition data through a RESTful API so that I can retrieve and display food logs in the UI or integrate with external systems.

**Why this priority**: Core functionality - without API endpoints, there's no way to access the food data that's already being collected by the Food Service. This is the primary value of the feature.

**Independent Test**: Can be fully tested by making HTTP requests to the API endpoints (GET /, GET /{date}, GET /range) and validating response structure, status codes, and data accuracy. Delivers immediate value by exposing food data for consumption.

**Acceptance Scenarios**:

1. **Given** the API is deployed and food logs exist in the database, **When** a client makes a GET request to `/` with pagination parameters, **Then** the API returns a paginated list of food logs with proper metadata (totalCount, pageNumber, pageSize, totalPages)

2. **Given** the API is deployed and a food log exists for a specific date, **When** a client makes a GET request to `/{date}` with format YYYY-MM-DD, **Then** the API returns the food log for that date with summary data (calories, caloriesIn, water) and goals

3. **Given** the API is deployed and food logs exist for a date range, **When** a client makes a GET request to `/range/{startDate}/{endDate}` with valid dates, **Then** the API returns all food logs within that range with pagination support

4. **Given** the API is deployed, **When** a client makes a GET request to `/healthz/liveness`, **Then** the API returns a 200 OK status within 100ms

5. **Given** a client makes a request with invalid date format, **When** the API validates the input, **Then** the API returns 400 Bad Request with a clear error message

6. **Given** a client requests a date or range with no matching food logs, **When** the API queries the database, **Then** the API returns 200 OK with an empty result set

---

### User Story 2 - Comprehensive Test Coverage for Quality Assurance (Priority: P1)

As a developer, I want comprehensive unit and integration tests (≥70% coverage) so that I can ensure code quality, prevent regressions, and deploy with confidence.

**Why this priority**: Testing is part of the core deliverable for this feature. Without tests, we cannot guarantee API reliability or maintain the codebase safely. Tests must be developed alongside the API implementation.

**Independent Test**: Can be fully tested by running the test suite locally and in CI/CD, verifying coverage reports reach ≥70%, and confirming all tests pass consistently. Delivers confidence in code quality and deployment readiness.

**Acceptance Scenarios**:

1. **Given** the unit test project is created with all dependencies, **When** tests are executed for endpoint handlers with mocked dependencies, **Then** all success, error, and edge case scenarios are validated and tests pass

2. **Given** the integration test project is created with contract tests, **When** tests validate service registrations and API startup, **Then** all contract tests pass without requiring external dependencies

3. **Given** the integration test project includes E2E tests with Cosmos DB Emulator, **When** tests execute full request/response cycles against the API, **Then** all E2E tests pass and validate end-to-end functionality

4. **Given** all test projects are complete, **When** code coverage is measured, **Then** coverage reaches ≥70% excluding Program.cs (which is excluded via ExcludeFromCodeCoverage attribute)

5. **Given** tests are run in CI/CD pipeline, **When** the GitHub Actions workflow executes, **Then** all tests pass consistently in both local and CI/CD environments

---

### User Story 3 - Infrastructure as Code for Automated Deployment (Priority: P2)

As a platform engineer, I want Bicep templates for deploying the Food API so that infrastructure is version-controlled, reproducible, and deployments are automated.

**Why this priority**: Essential for production deployment but can be developed after core API functionality is working. Infrastructure follows the same patterns as existing APIs, reducing risk and complexity.

**Independent Test**: Can be fully tested by running Bicep linting, validation, and what-if analysis, then deploying to a test environment and verifying the Container App is accessible via APIM. Delivers infrastructure automation and deployment repeatability.

**Acceptance Scenarios**:

1. **Given** Bicep templates are created following the weight-api pattern, **When** templates are linted and validated, **Then** no errors or warnings are reported

2. **Given** Bicep templates include Container App configuration, **When** what-if analysis is run against dev environment, **Then** expected resources are shown (Container App, APIM integration, App Configuration keys)

3. **Given** Bicep templates are deployed to dev environment, **When** deployment completes, **Then** the Container App is running, accessible via APIM at `/food` path, and health checks pass

4. **Given** APIM integration is configured, **When** a request is made to `{apimGatewayUrl}/food`, **Then** the request is routed to the Container App and returns a valid response

5. **Given** App Configuration is updated, **When** the API reads configuration, **Then** the `Biotrackr:FoodApiUrl` key returns the correct APIM gateway URL

---

### User Story 4 - CI/CD Pipeline for Automated Testing and Deployment (Priority: P2)

As a developer, I want automated testing and deployment via GitHub Actions so that code changes are validated automatically and deployed safely to dev environment.

**Why this priority**: CI/CD automation is important for long-term maintainability but can be implemented after core API and tests are working locally. The workflow follows established patterns from existing APIs.

**Independent Test**: Can be fully tested by triggering the workflow on a pull request, validating all jobs execute successfully (unit tests, contract tests, build, lint, validate, deploy, E2E tests), and confirming the deployed API is functional. Delivers automation and deployment confidence.

**Acceptance Scenarios**:

1. **Given** the GitHub Actions workflow is created, **When** a pull request modifies Food API code or infrastructure, **Then** the workflow is triggered automatically

2. **Given** the workflow runs unit tests, **When** code coverage is below 70%, **Then** the workflow fails and prevents deployment

3. **Given** the workflow runs contract tests, **When** service registrations or API startup fail, **Then** the workflow fails and reports specific test failures

4. **Given** the workflow builds the container image, **When** the build completes, **Then** the image is pushed to Azure Container Registry with the correct tag

5. **Given** the workflow deploys infrastructure, **When** Bicep deployment completes, **Then** the Container App is updated with the new image and configuration

6. **Given** the workflow runs E2E tests against deployed API, **When** tests complete, **Then** all E2E tests pass and validate the deployed API functionality

7. **Given** test results are generated, **When** the test-reporter action runs, **Then** test results are published to the pull request with checks: write permission

---

### Edge Cases

- What happens when pagination parameters exceed limits (pageSize > 100)? - API should cap pageSize at 100 and return a 400 Bad Request for pageSize < 1
- How should the API handle invalid date formats (not YYYY-MM-DD)? - API should return 400 Bad Request with a clear error message indicating expected format
- What behavior is expected when startDate > endDate in range queries? - API should return 400 Bad Request with an error message indicating invalid date range
- How should the API respond when Cosmos DB is unavailable? - API should return 503 Service Unavailable and health check should fail
- What happens when no food logs exist for a requested date or range? - API should return 200 OK with an empty result set (not 404), as the absence of data is a valid state
- How does the API handle concurrent requests? - API should handle at least 100 concurrent requests without degradation, leveraging Cosmos DB connection pooling
- Should the API support filtering by userId for multi-user scenarios? - Assume single-user scenario for initial implementation; userId is stored but not exposed as a query parameter

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: API MUST expose RESTful endpoints for querying food logs: GET /, GET /{date}, GET /range/{startDate}/{endDate}, GET /healthz/liveness
- **FR-002**: API MUST support pagination with configurable page size (default: 20, min: 1, max: 100) and page number (starting at 1)
- **FR-003**: API MUST validate date formats as YYYY-MM-DD and return 400 Bad Request for invalid formats with descriptive error messages
- **FR-004**: API MUST return proper HTTP status codes: 200 OK (success), 400 Bad Request (invalid input), 404 Not Found (endpoint not found), 500 Internal Server Error (unexpected errors), 503 Service Unavailable (database unreachable)
- **FR-005**: API MUST integrate with Azure App Configuration for centralized settings management (database endpoint, container name, partition key path)
- **FR-006**: API MUST provide OpenAPI/Swagger documentation accessible at `/swagger` endpoint for API exploration
- **FR-007**: API MUST serialize Cosmos DB documents using camelCase property naming convention
- **FR-008**: API MUST use Managed Identity for Cosmos DB authentication in production environments (Azure-hosted)
- **FR-009**: API MUST support account key authentication for local development and testing scenarios
- **FR-010**: Unit tests MUST achieve ≥70% code coverage excluding Program.cs (which has ExcludeFromCodeCoverage attribute)
- **FR-011**: Integration tests MUST include Contract Tests (ProgramStartupTests, ServiceRegistrationTests, ApiSmokeTests) that validate configuration without external dependencies
- **FR-012**: Integration tests MUST include E2E Tests (FoodEndpointsTests) that validate full request/response cycles against Cosmos DB Emulator
- **FR-013**: Infrastructure MUST be defined in Bicep templates following established patterns from weight-api and activity-api
- **FR-014**: Container App MUST be configured with health probes for liveness checks, environment variables for configuration, and user-assigned managed identity
- **FR-015**: API MUST be accessible via Azure API Management at `/food` path with operations: GetAllFoodLogs, GetFoodLogByDate, GetFoodLogsByDateRange, LivenessCheck
- **FR-016**: APIM product named "Food" MUST be created with subscription requirements enabled
- **FR-017**: GitHub Actions workflow MUST execute on pull requests that modify Food API code or infrastructure files
- **FR-018**: GitHub Actions workflow MUST include jobs for: env setup, unit tests, contract tests, build container image, lint Bicep, validate Bicep, preview changes, deploy to dev, run E2E tests
- **FR-019**: GitHub Actions workflow MUST fail if code coverage is below 70% threshold
- **FR-020**: GitHub Actions workflow MUST publish test results to pull request using dorny/test-reporter action with checks: write permission

### Key Entities

- **FoodLog**: Represents a daily food and nutrition log with unique identifier, document type ("FoodLog"), user identifier, date (YYYY-MM-DD), summary data (calories, caloriesIn, water), goals (calories), and time-to-live for data retention
  
- **FoodLogSummary**: Nested data within FoodLog representing daily nutrition totals including total calories burned, calories consumed (caloriesIn), and water intake in milliliters

- **FoodLogGoals**: Nested data within FoodLog representing daily nutrition targets including calorie goal

- **FoodLogResponse**: API response model for a single food log containing all FoodLog properties formatted for client consumption

- **PaginatedFoodLogsResponse**: API response model for collections of food logs including data array, pagination metadata (totalCount, pageNumber, pageSize, totalPages), and navigation helpers

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Food API is successfully deployed and accessible via APIM gateway at `{apimGatewayUrl}/food` with all endpoints returning proper responses
- **SC-002**: All API endpoints respond with correct HTTP status codes (200, 400, 404, 500, 503) based on request validity and system state
- **SC-003**: API query responses for date-based searches return results in under 200 milliseconds for 95th percentile (measured via Application Insights or load testing)
- **SC-004**: API successfully handles 100 concurrent requests without performance degradation or errors (measured via load testing)
- **SC-005**: Health check endpoint responds within 100 milliseconds consistently (measured via health probe logs)
- **SC-006**: Unit test suite achieves ≥70% code coverage across all API components excluding Program.cs
- **SC-007**: All integration tests (contract and E2E) pass consistently in both local development environment and CI/CD pipeline with zero flaky tests
- **SC-008**: GitHub Actions workflow completes all quality gates (tests, linting, validation, deployment) in under 15 minutes per run
- **SC-009**: Container image size is under 500MB for efficient deployment and scaling
- **SC-010**: Infrastructure deployment via Bicep completes successfully to dev environment with no manual intervention required
- **SC-011**: API documentation is accessible via Swagger UI and accurately describes all endpoints, request/response models, and status codes
- **SC-012**: Zero critical or high-severity security vulnerabilities detected in container image scan (Trivy or equivalent)
