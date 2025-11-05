# Quickstart: Remove Placeholder Test File

**Feature**: 010-remove-placeholder-test  
**Time to Complete**: 5-10 minutes

## Prerequisites

- Git installed and configured
- .NET 9.0 SDK installed
- Access to the biotrackr repository
- Checked out to branch `010-remove-placeholder-test`

## Quick Implementation Steps

### 1. Verify Current State
```powershell
# Navigate to integration test project
cd src\Biotrackr.Activity.Svc\Biotrackr.Activity.Svc.IntegrationTests

# Confirm file exists
Test-Path .\UnitTest1.cs  # Should return True

# Check current test count
dotnet test --list-tests
```

### 2. Delete the Placeholder File
```powershell
# Delete the file using git
git rm UnitTest1.cs

# OR delete manually and stage
Remove-Item UnitTest1.cs
git add -u
```

### 3. Verify Build Success
```powershell
# Clean build to clear cache
dotnet clean

# Build the test project
dotnet build

# Verify no errors or warnings
```

### 4. Run Integration Tests
```powershell
# Run all integration tests
dotnet test

# Verify:
# - All tests pass
# - Test count decreased by 1 (the empty placeholder test)
# - No test discovery errors
```

### 5. Verify No References
```powershell
# Search for any references to UnitTest1 (should find none)
cd ..\..\..  # Back to repo root
git grep "UnitTest1" -- "*.cs" "*.csproj"
```

### 6. Commit the Change
```powershell
git commit -m "chore: remove placeholder test file from Activity.Svc integration tests

- Delete auto-generated UnitTest1.cs placeholder
- File contained only empty test skeleton
- All integration tests still pass
- Fixes #100"
```

## Verification Checklist

After completing the steps above, verify:

- [ ] File `UnitTest1.cs` no longer exists in working directory
- [ ] File is staged for deletion in git (`git status` shows deleted file)
- [ ] Project builds successfully with no errors or warnings
- [ ] All integration tests pass (Contract and E2E)
- [ ] No references to `UnitTest1` remain in codebase
- [ ] Commit message follows conventional commit format

## Expected Outcome

**Before**:
```
src/Biotrackr.Activity.Svc/Biotrackr.Activity.Svc.IntegrationTests/
├── UnitTest1.cs                    ← Placeholder file
├── Contract/
│   ├── ProgramStartupTests.cs
│   └── ServiceRegistrationTests.cs
└── E2E/
    ├── ActivityServiceTests.cs
    └── CosmosRepositoryTests.cs
```

**After**:
```
src/Biotrackr.Activity.Svc/Biotrackr.Activity.Svc.IntegrationTests/
├── Contract/
│   ├── ProgramStartupTests.cs
│   └── ServiceRegistrationTests.cs
└── E2E/
    ├── ActivityServiceTests.cs
    └── CosmosRepositoryTests.cs
```

## Troubleshooting

### Build Cache Issues
If build shows errors after deletion:
```powershell
dotnet clean
dotnet build --no-incremental
```

### IDE Still Shows File
If Visual Studio or VS Code still shows the file:
1. Close and reopen the IDE
2. Or reload the solution/workspace
3. Sync with git: `git status` to confirm deletion

### Test Count Mismatch
Expected behavior: Test count should decrease by 1 (the empty `Test1()` method from UnitTest1.cs)

## Time Estimate

- **File Deletion**: 1 minute
- **Build Verification**: 2 minutes
- **Test Execution**: 3-5 minutes (depending on E2E test duration)
- **Commit**: 1 minute

**Total**: 5-10 minutes