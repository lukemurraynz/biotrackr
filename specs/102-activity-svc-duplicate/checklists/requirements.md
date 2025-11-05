# Requirements Checklist: Remove Duplicate IFitbitService Registration

**Feature**: 102-activity-svc-duplicate  
**Purpose**: Validate specification completeness and correctness  
**Created**: 2025-11-06

This checklist serves as "unit tests for requirements" - each item verifies that a specific aspect of the specification is properly defined, clear, and testable.

## Specification Completeness

### User Stories & Scenarios
- [ ] **CHK-001**: Are all user stories prioritized (P1, P2, P3)?
  - **Traces to**: User Scenarios section
  - **Verification**: Each story has explicit priority marking
  
- [ ] **CHK-002**: Does each user story describe independent, testable value?
  - **Traces to**: User Story 1 (Clear Service Registration), User Story 2 (Consistent Pattern), User Story 3 (Reliable Tests)
  - **Verification**: Each story can be tested and validated independently

- [ ] **CHK-003**: Are acceptance scenarios written in Given-When-Then format?
  - **Traces to**: All user story acceptance scenarios
  - **Verification**: Each scenario follows GWT pattern

### Functional Requirements

- [ ] **CHK-004**: Is the duplicate registration clearly identified?
  - **Traces to**: FR-001, Issue #102 description
  - **Verification**: Line 70 in Program.cs is documented as the duplicate

- [ ] **CHK-005**: Is the correct registration pattern specified?
  - **Traces to**: FR-002
  - **Verification**: AddHttpClient with AddStandardResilienceHandler is the required pattern

- [ ] **CHK-006**: Is the expected service lifetime documented?
  - **Traces to**: FR-004, Key Entities section
  - **Verification**: IFitbitService should be transient (HttpClientFactory-managed)

- [ ] **CHK-007**: Are compilation requirements defined?
  - **Traces to**: FR-003
  - **Verification**: Code must compile successfully after change

- [ ] **CHK-008**: Are test validation requirements specified?
  - **Traces to**: FR-004, FR-005
  - **Verification**: Contract tests must verify transient lifetime, no regressions allowed

- [ ] **CHK-009**: Is consistency with other services required?
  - **Traces to**: FR-006
  - **Verification**: Must match Weight, Sleep, Food service patterns

- [ ] **CHK-010**: Are decision record guidelines referenced?
  - **Traces to**: FR-007, Issue #102 additional context
  - **Verification**: Service Lifetime Registration Decision Record is referenced

### Edge Cases

- [ ] **CHK-011**: Are service resolution edge cases considered?
  - **Traces to**: Edge Cases section
  - **Verification**: Multiple resolution behavior is documented (transient = different instances)

- [ ] **CHK-012**: Is HttpClientFactory connection pooling understood?
  - **Traces to**: Edge Cases section
  - **Verification**: Factory manages pooling, not service lifetime

- [ ] **CHK-013**: Is accidental deletion risk considered?
  - **Traces to**: Edge Cases section
  - **Verification**: Compilation failure would catch missing registration

## Technical Clarity

### File Locations

- [ ] **CHK-014**: Is the file to modify clearly identified?
  - **Traces to**: Issue #102 location section
  - **Verification**: `src/Biotrackr.Activity.Svc/Biotrackr.Activity.Svc/Program.cs` line 70

- [ ] **CHK-015**: Are test file locations documented?
  - **Traces to**: Issue #102 testing notes
  - **Verification**: `ServiceRegistrationTests.cs` path is specified

### Code Changes

- [ ] **CHK-016**: Is the exact line to remove documented?
  - **Traces to**: Issue #102 code examples
  - **Verification**: `services.AddScoped<IFitbitService, FitbitService>();` on line 70

- [ ] **CHK-017**: Is the line to preserve documented?
  - **Traces to**: FR-002, Issue #102 code examples
  - **Verification**: `AddHttpClient<IFitbitService, FitbitService>().AddStandardResilienceHandler()` must remain

### Service Lifetime Guidelines

- [ ] **CHK-018**: Are singleton service examples provided?
  - **Traces to**: Issue #102 service lifetime table
  - **Verification**: CosmosClient, SecretClient are singleton

- [ ] **CHK-019**: Are scoped service examples provided?
  - **Traces to**: Issue #102 service lifetime table
  - **Verification**: ICosmosRepository, IActivityService are scoped

- [ ] **CHK-020**: Is the transient pattern explained?
  - **Traces to**: Issue #102 service lifetime table, FR-004
  - **Verification**: HttpClient-based services are transient via AddHttpClient

## Testing Requirements

### Contract Tests

- [ ] **CHK-021**: Is the contract test command documented?
  - **Traces to**: Issue #102 testing notes
  - **Verification**: `dotnet test --filter "FullyQualifiedName~Contract.ServiceRegistrationTests"`

- [ ] **CHK-022**: Are expected test results defined?
  - **Traces to**: SC-003
  - **Verification**: Tests must verify transient lifetime for IFitbitService

### Integration Tests

- [ ] **CHK-023**: Is the integration test command documented?
  - **Traces to**: Issue #102 suggested approach
  - **Verification**: `dotnet test` in integration test directory

- [ ] **CHK-024**: Are regression prevention criteria defined?
  - **Traces to**: FR-005, SC-004
  - **Verification**: All existing tests must pass

### Build Verification

- [ ] **CHK-025**: Is the build command specified?
  - **Traces to**: SC-002
  - **Verification**: `dotnet build` must succeed with zero errors

## Success Criteria Validation

- [ ] **CHK-026**: Are success criteria measurable?
  - **Traces to**: Success Criteria section
  - **Verification**: All criteria have objective pass/fail conditions

- [ ] **CHK-027**: Is "only one registration" verifiable by inspection?
  - **Traces to**: SC-001
  - **Verification**: Can grep for "IFitbitService" registrations

- [ ] **CHK-028**: Is compilation success objectively testable?
  - **Traces to**: SC-002
  - **Verification**: Exit code 0 from dotnet build

- [ ] **CHK-029**: Is test pass rate measurable?
  - **Traces to**: SC-003, SC-004
  - **Verification**: 100% pass rate is objective metric

- [ ] **CHK-030**: Is cross-service consistency verifiable?
  - **Traces to**: SC-005
  - **Verification**: Can compare Program.cs files across services

- [ ] **CHK-031**: Is alignment with decision records verifiable?
  - **Traces to**: SC-006
  - **Verification**: Can reference decision record and compare patterns

## Context & References

### Decision Records

- [ ] **CHK-032**: Is the Service Lifetime Registration decision record referenced?
  - **Traces to**: Issue #102 additional context
  - **Verification**: Link to `docs/decision-records/2025-10-28-service-lifetime-registration.md`

- [ ] **CHK-033**: Is the Common Resolutions document referenced?
  - **Traces to**: Issue #102 additional context
  - **Verification**: Link to `.specify/memory/common-resolutions.md`

- [ ] **CHK-034**: Are Copilot Instructions referenced?
  - **Traces to**: Issue #102 additional context
  - **Verification**: Link to `.github/copilot-instructions.md`

### Related Work

- [ ] **CHK-035**: Are previously fixed services documented?
  - **Traces to**: Issue #102 related issues
  - **Verification**: Weight (commit e5d89ab), Sleep, Food services mentioned

- [ ] **CHK-036**: Is related issue #108 mentioned?
  - **Traces to**: Issue #102 related issues
  - **Verification**: Test fixture cleanup has same pattern

### Microsoft Documentation

- [ ] **CHK-037**: Is HttpClientFactory documentation referenced?
  - **Traces to**: Issue #102 additional context
  - **Verification**: Link to Microsoft docs on HttpClientFactory

## Traceability Summary

### Requirements Coverage
- Total Requirements: 7 (FR-001 through FR-007)
- Requirements with Checklist Items: 7 (100%)
- Checklist Items: 37

### User Story Coverage
- Total User Stories: 3 (P1, P2, P3)
- User Stories with Checklist Items: 3 (100%)
- User Story Checklist Items: 3 (CHK-001, CHK-002, CHK-003)

### Success Criteria Coverage
- Total Success Criteria: 6 (SC-001 through SC-006)
- Success Criteria with Checklist Items: 6 (100%)
- Success Criteria Checklist Items: 6 (CHK-026 through CHK-031)

## Checklist Completion Target

**Target**: ≥80% traceability to spec sections (31/37 items)  
**Actual**: 100% (37/37 items trace to spec or issue)

---

## Notes

This is a straightforward cleanup task with minimal ambiguity. All requirements are clearly defined in the original issue #102. The specification provides:

1. **Clear problem statement**: Line 70 contains duplicate registration
2. **Specific solution**: Remove one line of code
3. **Verification method**: Contract tests already validate correct behavior
4. **Consistency check**: Compare with other services

No clarification should be needed for implementation.
