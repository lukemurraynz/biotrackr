---
description: "Task list for removing placeholder test file"
---

# Tasks: Remove Placeholder Test File

**Input**: Design documents from `/specs/010-remove-placeholder-test/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, quickstart.md ✅

**Tests**: Tests are NOT REQUIRED for this cleanup task - existing integration tests will verify no regressions.

**Organization**: This simple cleanup task has a single user story with straightforward file deletion and verification steps.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1)
- Include exact file paths in descriptions

## Path Conventions

This task affects: `src/Biotrackr.Activity.Svc/Biotrackr.Activity.Svc.IntegrationTests/`

---

## Phase 1: Setup (Verification)

**Purpose**: Verify current state before making changes

- [X] T001 Verify placeholder file exists at `src/Biotrackr.Activity.Svc/Biotrackr.Activity.Svc.IntegrationTests/UnitTest1.cs`
- [X] T002 [P] Document current test count by running `dotnet test --list-tests` in integration test project
- [X] T003 [P] Search codebase for any references to UnitTest1 using `git grep "UnitTest1"`

**Checkpoint**: Confirmed file exists, baseline test count recorded, no external references found

---

## Phase 2: User Story 1 - Remove Dead Code from Test Project (Priority: P1) 🎯 MVP

**Goal**: Delete the placeholder test file and verify all existing tests still pass

**Independent Test**: File no longer exists, build succeeds, all integration tests pass, test count decreased by 1

### Implementation for User Story 1

- [ ] T004 [US1] Delete placeholder file `src/Biotrackr.Activity.Svc/Biotrackr.Activity.Svc.IntegrationTests/UnitTest1.cs` using `git rm`
- [ ] T005 [US1] Clean build cache by running `dotnet clean` in integration test project directory
- [ ] T006 [US1] Build test project using `dotnet build` and verify zero errors/warnings
- [ ] T007 [US1] Run Contract tests using `dotnet test --filter "FullyQualifiedName~Contract"` and verify all pass
- [ ] T008 [US1] Run E2E tests using `dotnet test --filter "FullyQualifiedName~E2E"` and verify all pass
- [ ] T009 [US1] Verify test count decreased by exactly 1 (the empty Test1 method from UnitTest1.cs)
- [ ] T010 [US1] Verify no references to UnitTest1 remain by running `git grep "UnitTest1"` (should return nothing)
- [ ] T011 [US1] Stage deletion and commit with message referencing issue #100

**Acceptance Verification**:
- ✅ File `UnitTest1.cs` does not exist (FR-001, SC-001)
- ✅ Build completes successfully with no warnings (FR-003, SC-003)
- ✅ All integration tests pass (FR-002, SC-002)
- ✅ No code references to UnitTest1 remain (FR-004)
- ✅ Test project structure contains only meaningful tests (FR-005, SC-004)

**Checkpoint**: User Story 1 complete - placeholder file removed, all tests passing, codebase clean

---

## Phase 3: Polish & Documentation

**Purpose**: Final validation and documentation updates

- [ ] T012 [P] Update `.github/copilot-instructions.md` if manual additions needed (already auto-updated by agent script)
- [ ] T013 [P] Run full test suite from repository root to verify no unexpected impacts
- [ ] T014 Close issue #100 with reference to commit

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **User Story 1 (Phase 2)**: Depends on Setup verification completion
- **Polish (Phase 3)**: Depends on User Story 1 completion

### Task Dependencies Within User Story 1

1. **T004**: Delete file - Must complete first
2. **T005**: Clean build - Must complete after T004
3. **T006**: Build verification - Must complete after T005
4. **T007-T008**: Test execution - Can run in sequence after T006
5. **T009-T010**: Verification checks - Can run after tests pass
6. **T011**: Commit - Must be last task after all verifications pass

### Parallel Opportunities

**Setup Phase (Phase 1)**:
- T002 and T003 can run in parallel (different operations)

**Polish Phase (Phase 3)**:
- T012 and T013 can run in parallel (different scopes)

**Note**: User Story 1 tasks (T004-T011) must run sequentially due to dependencies (each step validates the previous)

---

## Parallel Example: Setup Phase

```bash
# Launch verification tasks together:
Task: "Document current test count in test project"
Task: "Search codebase for UnitTest1 references"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only - Recommended)

1. Complete Phase 1: Setup (verification) - ~2 minutes
2. Complete Phase 2: User Story 1 (file deletion and validation) - ~5 minutes
3. **STOP and VALIDATE**: Verify all tests pass independently
4. Complete Phase 3: Polish (final checks) - ~2 minutes
5. **Total time: ~10 minutes**

### Risk Mitigation

- **Low Risk Task**: File contains no logic, no dependencies
- **Safety Checks**: Multiple verification steps (build, tests, grep)
- **Rollback**: Simple `git revert` if unexpected issues arise
- **Edge Cases Covered**: Build cache clearing, reference checking, test count validation

---

## Task Summary

**Total Tasks**: 14  
**User Stories**: 1 (P1 only)  
**Setup Tasks**: 3  
**Implementation Tasks**: 8  
**Polish Tasks**: 3  

**Parallel Opportunities**: 4 tasks can run in parallel (2 in Setup, 2 in Polish)  
**Sequential Tasks**: 8 tasks must run in sequence (User Story 1 implementation)

**MVP Scope**: Complete User Story 1 (Phase 2) - removes placeholder file and verifies test suite health

**Estimated Duration**: 10 minutes total
- Setup: 2 minutes
- User Story 1: 5 minutes
- Polish: 3 minutes

---

## Notes

- This is an extremely low-risk cleanup task with straightforward execution
- No new code is being written - only file deletion
- Multiple verification steps ensure safety
- Existing test suite provides regression detection
- Quick feedback loop (minutes, not hours)
- Can be completed in a single focused session
- Follow quickstart.md for detailed command-by-command execution
- Constitution principles: This task REDUCES technical debt (Principle V)
