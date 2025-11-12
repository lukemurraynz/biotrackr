# Feature Specification: APIM Managed Identity Authentication for UI Consumer

**Feature Branch**: `013-apim-managed-identity`  
**Created**: 2025-11-12  
**Status**: Draft  
**Parent Issue**: #151  
**Input**: User description: "Enable APIM Managed Identity Authentication for UI Consumer - Update Azure API Management (APIM) infrastructure to support Managed Identity authentication from the Blazor UI Container App. This eliminates the need for subscription key management and provides more secure authentication."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Secure API Access via Managed Identity (Priority: P1)

As a Blazor UI application running in Azure Container Apps, I need to access APIM-protected backend APIs using my managed identity instead of subscription keys, so that authentication is more secure and requires no secret management.

**Why this priority**: This is the core security improvement that eliminates manual credential management and reduces security risks. It's the MVP that delivers immediate value.

**Independent Test**: Can be fully tested by configuring APIM JWT validation policies and calling an APIM endpoint with a managed identity token via Azure CLI, delivering verified secure access without subscription keys.

**Acceptance Scenarios**:

1. **Given** the UI managed identity has appropriate RBAC permissions, **When** the UI requests an access token for the APIM audience, **Then** Azure AD issues a valid JWT token
2. **Given** a valid JWT token from the UI managed identity, **When** the token is presented to an APIM endpoint, **Then** APIM validates the token and allows the request through
3. **Given** an invalid or expired JWT token, **When** the token is presented to an APIM endpoint, **Then** APIM rejects the request with a 401 Unauthorized response

---

### User Story 2 - Maintain Backward Compatibility with Subscription Keys (Priority: P2)

As a system operator, I need existing subscription key authentication to continue working alongside the new managed identity authentication, so that we can migrate gradually without breaking existing integrations.

**Why this priority**: Ensures zero downtime during migration and allows for phased rollout. Critical for production stability but not needed for initial proof of concept.

**Independent Test**: Can be fully tested by calling APIM endpoints with both subscription keys (existing method) and JWT tokens (new method), verifying both authentication methods work simultaneously.

**Acceptance Scenarios**:

1. **Given** an APIM endpoint configured with both JWT validation and subscription key policies, **When** a request includes a valid subscription key, **Then** APIM allows the request through
2. **Given** an APIM endpoint configured with both authentication methods, **When** a request includes a valid JWT token, **Then** APIM allows the request through
3. **Given** an APIM endpoint configured with both authentication methods, **When** a request has neither valid subscription key nor JWT token, **Then** APIM rejects the request

---

### User Story 3 - RBAC Configuration for UI to APIM Access (Priority: P1)

As a DevOps engineer, I need the UI managed identity to have appropriate RBAC roles assigned for APIM access, so that the identity can request tokens with the correct audience and scope.

**Why this priority**: Essential prerequisite for managed identity authentication to work. Without proper RBAC, tokens cannot be obtained or validated.

**Independent Test**: Can be fully tested by checking Azure RBAC role assignments on the UI managed identity and verifying token acquisition with correct claims.

**Acceptance Scenarios**:

1. **Given** the UI managed identity exists, **When** RBAC roles are assigned, **Then** the identity has `API Management Service Contributor` role or appropriate custom role
2. **Given** proper RBAC configuration, **When** the UI requests a token for APIM audience, **Then** the token contains expected claims including audience and issuer
3. **Given** proper RBAC configuration, **When** APIM validates the token, **Then** APIM can verify the token signature and claims against Azure AD

---

### Edge Cases

- What happens when a JWT token expires during a long-running API call?
- How does the system handle token refresh for the UI managed identity?
- What occurs if RBAC roles are removed or modified after deployment?
- How does APIM behave if Azure AD is temporarily unavailable for token validation?
- What happens when the same endpoint receives both subscription key and JWT token simultaneously?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: APIM MUST validate JWT tokens using the `validate-jwt` policy on all backend APIs (Weight, Activity, Sleep, Food)
- **FR-002**: APIM MUST accept the APIM App ID URI as the expected audience in JWT tokens
- **FR-003**: APIM MUST validate that JWT tokens are issued by the correct Azure AD tenant issuer URL
- **FR-004**: APIM MUST continue to accept valid subscription keys for authentication (backward compatibility)
- **FR-005**: APIM MUST support both JWT token authentication AND subscription key authentication simultaneously
- **FR-006**: The UI managed identity MUST have RBAC permissions to request tokens for the APIM audience
- **FR-007**: APIM infrastructure (Bicep) MUST support configuration parameters for JWT validation (tenant ID, App ID URI)
- **FR-008**: APIM MUST extract and forward relevant claims from validated JWT tokens to backend services
- **FR-009**: System MUST reject requests that have neither valid JWT token nor valid subscription key
- **FR-010**: APIM configuration MUST be parameterized to support different environments (dev, staging, production)

### Non-Functional Requirements

- **NFR-001**: JWT token validation MUST NOT significantly increase API response latency (< 50ms overhead)
- **NFR-002**: APIM policy configuration MUST be maintainable and not require manual XML editing for environment changes
- **NFR-003**: RBAC role assignments MUST be documented in Bicep code comments for clarity
- **NFR-004**: Token validation failure messages MUST provide clear guidance for troubleshooting (without exposing security details)

### Key Entities *(infrastructure components)*

- **APIM Instance**: Azure API Management service in consumption tier, acts as API gateway
- **JWT Validation Policy**: APIM policy configuration that validates tokens against Azure AD
- **UI Managed Identity**: System-assigned or user-assigned managed identity for the Blazor UI Container App
- **APIM App Registration**: Azure AD application registration representing APIM (or default APIM audience)
- **RBAC Role Assignment**: Azure role-based access control binding between UI identity and APIM resource

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: APIM successfully validates JWT tokens from UI managed identity and allows access to protected APIs
- **SC-002**: Existing clients using subscription keys continue to access APIM endpoints without modification
- **SC-003**: JWT token validation adds less than 50ms to API response time (measured via Azure Monitor)
- **SC-004**: Zero manual credential management required for UI-to-APIM authentication (no keys stored in code or configuration)
- **SC-005**: All four backend API services (Weight, Activity, Sleep, Food) are protected with JWT validation policies
- **SC-006**: RBAC role assignments are deployed via Bicep infrastructure code (no manual Azure portal configuration)
- **SC-007**: Token validation failures return appropriate HTTP 401/403 responses with clear error messages
- **SC-008**: Infrastructure can be deployed to multiple environments (dev, staging, prod) using Bicep parameters without code changes

## Assumptions

- Azure AD tenant is already configured and accessible
- APIM instance already exists in the target resource group
- UI Container App will be created in a subsequent phase (#153 per parent issue #151)
- APIM uses the default Azure API Management audience or a specific App ID URI can be configured
- Network connectivity between UI Container App and APIM will be established
- Azure RBAC permissions are sufficient for managed identity token acquisition
- All backend APIs (Weight, Activity, Sleep, Food) are already registered in APIM

## Dependencies

- Azure AD tenant must be accessible for JWT issuer validation
- APIM instance must exist and be in healthy state
- UI managed identity will be created in subsequent sub-issue (#2 per parent issue #151)
- Bicep deployment pipeline must have permissions to update APIM policies and RBAC roles

## Out of Scope

- Creating the Blazor UI Container App (handled in parent issue #151)
- Creating the UI managed identity (handled in sub-issue #2)
- Implementing actual API client code in the Blazor UI (handled in sub-issue #3)
- Custom claims extraction beyond standard JWT claims
- Multi-tenant Azure AD support (single tenant only)
- Certificate-based authentication alternatives
- Custom JWT token issuers (only Azure AD supported)

## Technical Constraints

- Must use APIM consumption tier (current deployment)
- Must maintain existing subscription key authentication for backward compatibility
- JWT validation must use Azure AD as the token issuer
- Bicep IaC must be the deployment method (no manual portal configuration)
- Must follow existing Bicep module structure under `infra/modules/apim/`

## Estimated Effort

**Size: S** (4-6 hours)
- Bicep module updates: 1-2 hours
- Policy configuration: 1-2 hours
- RBAC setup: 1 hour
- Testing & validation: 1 hour
- Documentation: 0.5-1 hour
