# Implementation Plan: APIM Managed Identity Authentication

**Branch**: `013-apim-managed-identity` | **Date**: 2025-11-12 | **Spec**: [spec.md](spec.md)  
**Input**: Feature specification from `/specs/013-apim-managed-identity/spec.md`

## Summary

Enable Azure API Management to authenticate requests from the Blazor UI using Azure AD managed identity tokens instead of subscription keys. This involves updating APIM Bicep infrastructure to support JWT validation policies, configuring RBAC permissions for the UI managed identity, and maintaining backward compatibility with existing subscription key authentication.

## Technical Context

**Language/Version**: Bicep (Azure Infrastructure as Code)  
**Primary Dependencies**: Azure API Management (Consumption tier), Azure Active Directory, Azure RBAC  
**Storage**: N/A (infrastructure configuration only)  
**Testing**: Azure CLI for policy validation, manual token testing  
**Target Platform**: Azure Cloud (APIM Consumption tier)  
**Project Type**: Infrastructure as Code (Bicep modules)  
**Performance Goals**: JWT validation overhead < 50ms per request  
**Constraints**: Must maintain backward compatibility with subscription keys, must use consumption tier APIM  
**Scale/Scope**: 4 backend API services (Weight, Activity, Sleep, Food), single Azure AD tenant

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [X] **Code Quality Excellence**: Bicep modules follow existing project structure, parameterized for reusability
- [X] **Testing Strategy**: Manual validation with Azure CLI and token acquisition testing
- [X] **User Experience**: N/A (infrastructure only)
- [X] **Performance Requirements**: JWT validation latency target < 50ms documented
- [X] **Technical Debt**: No significant debt introduced, follows existing IaC patterns

## Project Structure

### Documentation (this feature)

```text
specs/013-apim-managed-identity/
├── plan.md              # This file
├── research.md          # Technical research on APIM JWT policies
├── data-model.md        # N/A (infrastructure only)
├── quickstart.md        # Deployment and testing guide
├── contracts/           # N/A (infrastructure only)
└── tasks.md             # Implementation task breakdown
```

### Source Code (repository root)

```text
infra/
├── modules/
│   └── apim/
│       ├── apim-consumption.bicep           # EXISTING - UPDATE JWT policy support
│       └── apim-policy-fragments.bicep      # NEW - Reusable policy XML fragments
└── core/
    ├── main.bicep                           # EXISTING - MAY UPDATE to pass JWT params
    └── main.dev.bicepparam                  # EXISTING - MAY UPDATE with tenant ID, audience

docs/
└── decision-records/
    └── 2025-11-12-apim-managed-identity-auth.md  # NEW - Decision record for this pattern

.github/
└── workflows/
    └── deploy-core-infra.yml                # EXISTING - Verify deployment workflow
```

**Structure Decision**: This feature modifies existing Bicep infrastructure under `infra/modules/apim/`. The main changes are:
1. Update `apim-consumption.bicep` to accept JWT validation parameters
2. Create reusable policy fragments for JWT validation
3. Update APIM API definitions to include validate-jwt policy
4. Document RBAC role assignments in Bicep comments

No new microservices or applications are created in this feature.

## Complexity Tracking

> No Constitution Check violations detected. This feature follows existing infrastructure patterns.

## Phase Breakdown

### Phase 0: Research (Complete before design)
- Research APIM validate-jwt policy syntax and requirements
- Investigate Azure AD token audience configuration for APIM
- Document RBAC roles needed for UI managed identity to APIM access
- Research backward compatibility approach (OR logic with subscription-key policy)

### Phase 1: Design
- Design Bicep parameter structure for JWT validation configuration
- Design policy XML fragments for reusability across APIs
- Define RBAC role assignment approach (Bicep vs manual)
- Document testing approach with Azure CLI

### Phase 2: Implementation (via /speckit.tasks)
- Update apim-consumption.bicep with JWT validation parameters
- Create policy fragment for validate-jwt configuration
- Apply JWT validation policy to Weight API
- Apply JWT validation policy to Activity API
- Apply JWT validation policy to Sleep API
- Apply JWT validation policy to Food API
- Configure RBAC role assignments (if via Bicep)
- Update main.bicepparam with tenant ID and audience
- Create decision record document

### Phase 3: Validation
- Deploy infrastructure to dev environment
- Test JWT token acquisition via Azure CLI
- Test APIM API call with JWT token
- Verify subscription key still works (backward compatibility)
- Verify 401 response for invalid tokens
- Document testing procedure in quickstart.md

## Dependencies

- **Prerequisite**: APIM instance must exist (already deployed)
- **Prerequisite**: Azure AD tenant accessible for JWT issuer validation
- **Blocked by**: UI managed identity creation (issue #2 of parent #151) - for end-to-end testing only
- **Enables**: UI Container App authentication (issue #3 of parent #151)

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Breaking existing API consumers | Low | High | Maintain subscription key authentication, test thoroughly |
| JWT validation performance overhead | Medium | Medium | Benchmark with Azure Monitor, optimize if needed |
| RBAC permission issues | Medium | Medium | Document required roles clearly, provide troubleshooting guide |
| Azure AD unavailable during token validation | Low | High | APIM caches public keys, degrades gracefully |

## Success Metrics

- All 4 backend APIs accept JWT tokens from managed identity
- All 4 backend APIs still accept subscription keys
- JWT validation latency < 50ms (measured in Application Insights)
- Zero manual credential management required
- Infrastructure deployed via Bicep (no manual portal steps)
