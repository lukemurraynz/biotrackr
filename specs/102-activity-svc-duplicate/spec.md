# Feature Specification: Remove Duplicate IFitbitService Registration

**Feature Branch**: `102-activity-svc-duplicate`  
**Created**: 2025-11-06  
**Status**: ✅ Completed  
**Input**: Remove duplicate IFitbitService registration from Activity.Svc Program.cs (Issue #102)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Clear Service Registration (Priority: P1)

As a developer maintaining the Activity Service, I want clear and consistent service registrations so that I understand service lifetimes without confusion and avoid runtime issues.

**Why this priority**: This is the core fix - removing the duplicate registration that violates documented patterns and causes confusion about service lifetime behavior.

**Independent Test**: Can be fully tested by verifying only one registration exists for IFitbitService and confirming contract tests pass with transient lifetime verification.

**Acceptance Scenarios**:

1. **Given** the Activity Service Program.cs has duplicate IFitbitService registrations, **When** I review the service configuration, **Then** only the AddHttpClient registration should exist
2. **Given** the duplicate AddScoped registration is removed, **When** I compile the service, **Then** the code compiles successfully with no errors
3. **Given** the service is configured with only AddHttpClient registration, **When** contract tests run, **Then** they verify IFitbitService is registered as transient

---

### User Story 2 - Consistent Pattern Across Services (Priority: P2)

As a team member working across multiple microservices, I want all services to follow identical registration patterns so that I can quickly understand and maintain any service.

**Why this priority**: Consistency across the codebase improves maintainability and reduces cognitive load when switching between services.

**Independent Test**: Can be verified by comparing Program.cs files across Weight, Sleep, Food, and Activity services to confirm identical patterns for HttpClient-based services.

**Acceptance Scenarios**:

1. **Given** Weight, Sleep, and Food services use only AddHttpClient for IFitbitService, **When** Activity Service is fixed, **Then** it matches the same pattern
2. **Given** the documented service lifetime guidelines, **When** I review Activity Service registrations, **Then** all registrations follow the documented patterns

---

### User Story 3 - Reliable Contract Tests (Priority: P3)

As a test engineer, I want contract tests to verify actual runtime behavior so that I can trust the service configuration and catch misconfigurations early.

**Why this priority**: While important, the tests already verify correct behavior - this story is about maintaining that trust after the fix.

**Independent Test**: Can be verified by running contract tests and confirming they detect transient lifetime for IFitbitService.

**Acceptance Scenarios**:

1. **Given** the duplicate registration is removed, **When** ServiceRegistrationTests run, **Then** they confirm IFitbitService is transient
2. **Given** all integration tests are executed, **When** tests complete, **Then** no test failures occur related to service resolution

---

### Edge Cases

- What happens if IFitbitService is resolved multiple times in the same scope? (Should get different instances due to transient lifetime)
- How does the HttpClientFactory manage connection pooling? (Factory manages pooling internally, service instances are lightweight)
- What if both registrations were removed by mistake? (Compilation would fail when IFitbitService is injected)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST have only one registration for IFitbitService interface
- **FR-002**: System MUST register IFitbitService via AddHttpClient with AddStandardResilienceHandler
- **FR-003**: System MUST compile successfully after removing duplicate registration
- **FR-004**: Contract tests MUST verify IFitbitService is registered as transient lifetime
- **FR-005**: System MUST maintain all existing functionality after the change (no regression)
- **FR-006**: Activity Service MUST follow the same registration pattern as Weight, Sleep, and Food services
- **FR-007**: Code MUST align with documented service lifetime guidelines in decision records

### Key Entities *(include if feature involves data)*

- **IFitbitService**: HttpClient-based service interface for Fitbit API interactions, should be transient lifetime
- **FitbitService**: Implementation class that depends on HttpClient, managed by HttpClientFactory
- **ActivityWorker**: Hosted service that depends on IFitbitService for periodic data fetching

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Only one service registration exists for IFitbitService (AddHttpClient only)
- **SC-002**: Activity Service compiles with zero errors after modification
- **SC-003**: 100% of contract tests pass, including ServiceRegistrationTests
- **SC-004**: 100% of integration tests pass with no regressions
- **SC-005**: Service registration pattern matches Weight, Sleep, and Food services (verified by code review)
- **SC-006**: Change aligns with service lifetime guidelines documented in decision records
