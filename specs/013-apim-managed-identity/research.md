# Research: APIM Managed Identity Authentication

**Feature**: 013-apim-managed-identity  
**Date**: 2025-11-12  
**Status**: Complete

## Research Questions

1. How does APIM validate-jwt policy work with Azure AD managed identities?
2. What audience value should be used for APIM JWT tokens?
3. What RBAC roles are needed for UI managed identity to access APIM?
4. How can we maintain backward compatibility with subscription keys?

## Findings

### 1. APIM validate-jwt Policy

The `validate-jwt` policy validates JWT tokens in the Authorization header before allowing requests to reach backend services.

**Key Configuration**:
```xml
<validate-jwt header-name="Authorization" failed-validation-httpcode="401">
    <openid-config url="https://login.microsoftonline.com/{tenant-id}/v2.0/.well-known/openid-configuration" />
    <audiences>
        <audience>{apim-app-id-uri}</audience>
    </audiences>
    <issuers>
        <issuer>https://sts.windows.net/{tenant-id}/</issuer>
    </issuers>
</validate-jwt>
```

**Key Points**:
- `openid-config` URL provides Azure AD's public signing keys for token validation
- Policy validates signature, expiration, audience, and issuer claims
- Failed validation returns 401 Unauthorized
- Can extract claims to variables for backend forwarding

**References**:
- [APIM validate-jwt policy documentation](https://learn.microsoft.com/en-us/azure/api-management/api-management-authentication-policies#ValidateJWT)
- [APIM policy expressions](https://learn.microsoft.com/en-us/azure/api-management/api-management-policy-expressions)

### 2. APIM Audience Configuration

For managed identity authentication to APIM, the audience claim must match one of:

**Option A: Default APIM Audience** (Recommended)
- Audience: `https://management.azure.com/` (Azure Management API)
- No App Registration required
- Simpler configuration
- Works for service-to-service calls

**Option B: Custom App Registration**
- Register APIM as Azure AD application
- Audience: Custom App ID URI (e.g., `api://biotrackr-apim`)
- More control over token claims
- Required for user authentication flows

**Decision**: Use **Option A** (default Azure Management audience) for initial implementation:
- UI managed identity requests token with resource `https://management.azure.com/`
- APIM validates token audience matches `https://management.azure.com/`
- Simplifies configuration, no additional App Registration needed

**Azure CLI Example**:
```bash
# Get token for Azure Management API (default APIM audience)
az account get-access-token --resource https://management.azure.com/
```

**References**:
- [Managed identities authentication flows](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/how-managed-identities-work-vm)
- [Azure AD token scopes and audiences](https://learn.microsoft.com/en-us/entra/identity-platform/access-tokens)

### 3. RBAC Requirements

For UI managed identity to successfully authenticate to APIM:

**Minimum Required Role**: None for token validation
- Managed identity only needs to acquire token with correct audience
- APIM validates token without requiring RBAC on the APIM resource itself

**Optional Roles** (for additional operations):
- `API Management Service Reader`: If UI needs to discover APIM metadata
- `API Management Service Contributor`: If UI needs to manage APIM configuration (not typical)

**Decision**: No RBAC role assignment required for basic JWT validation
- UI acquires token with `https://management.azure.com/` audience
- APIM validates token signature and claims
- Backend services receive authenticated requests

**Important Note**: RBAC roles are needed if implementing authorization logic (e.g., which APIs the identity can access). For this feature, we only implement authentication.

**References**:
- [Azure RBAC built-in roles](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles)
- [APIM access control](https://learn.microsoft.com/en-us/azure/api-management/api-management-role-based-access-control)

### 4. Backward Compatibility with Subscription Keys

APIM supports multiple authentication methods simultaneously using `choose` policy:

**Strategy**: Use `choose-when-otherwise` to accept either JWT or subscription key

```xml
<inbound>
    <choose>
        <when condition="@(context.Request.Headers.GetValueOrDefault("Authorization","") != "")">
            <!-- JWT token present - validate it -->
            <validate-jwt header-name="Authorization" failed-validation-httpcode="401">
                <openid-config url="https://login.microsoftonline.com/{tenant-id}/v2.0/.well-known/openid-configuration" />
                <audiences>
                    <audience>https://management.azure.com/</audience>
                </audiences>
            </validate-jwt>
        </when>
        <otherwise>
            <!-- No JWT token - require subscription key -->
            <check-header name="Ocp-Apim-Subscription-Key" failed-check-httpcode="401" />
        </otherwise>
    </choose>
    <base />
</inbound>
```

**Alternative**: Accept both simultaneously (OR logic)
- Check for Authorization header first
- If present, validate JWT
- If JWT validation fails, check for subscription key
- Reject only if both fail

**Decision**: Use `choose` approach (Option 1) for clearer authentication flow:
- Presence of Authorization header indicates JWT authentication intent
- Missing Authorization header falls back to subscription key
- Prevents ambiguity when both are present

**References**:
- [APIM choose policy](https://learn.microsoft.com/en-us/azure/api-management/api-management-advanced-policies#choose)
- [APIM check-header policy](https://learn.microsoft.com/en-us/azure/api-management/api-management-access-restriction-policies#CheckHTTPHeader)

## Technical Decisions

### Decision 1: Use Default Azure Management Audience
**Rationale**: Simplifies configuration, no App Registration required, sufficient for service-to-service authentication

### Decision 2: No RBAC Role Assignment Required
**Rationale**: JWT validation only requires valid token signature and claims, not RBAC permissions on APIM resource

### Decision 3: Choose-based Authentication Policy
**Rationale**: Clear separation of authentication methods, easier to debug, prevents ambiguity

### Decision 4: Apply Policy at API Level
**Rationale**: Allows granular control per API, easier to test and roll out incrementally

## Bicep Implementation Approach

### Parameters to Add
```bicep
@description('Enable JWT validation for managed identity authentication')
param enableManagedIdentityAuth bool = true

@description('Azure AD tenant ID for JWT issuer validation')
param tenantId string

@description('JWT audience to validate (default: Azure Management API)')
param jwtAudience string = 'https://management.azure.com/'
```

### Policy Application
- Update each API resource in `apim-consumption.bicep`
- Inject policy XML via Bicep string interpolation
- Parameterize tenant ID and audience for environment flexibility

### Testing Strategy
1. Deploy updated infrastructure to dev environment
2. Acquire JWT token via Azure CLI: `az account get-access-token --resource https://management.azure.com/`
3. Call APIM endpoint with Bearer token: `curl -H "Authorization: Bearer <token>" https://<apim>.azure-api.net/weight/api/weight`
4. Verify 200 OK response with valid token
5. Verify 401 Unauthorized with invalid/expired token
6. Verify 200 OK with subscription key (backward compatibility)

## Open Questions

1. **Should we log authentication method used?** - Useful for metrics, but adds complexity
   - **Decision**: Add context variable to track auth method for Application Insights

2. **Should we extract JWT claims for backend services?** - Useful for user identification
   - **Decision**: Extract `oid` (object ID) claim and forward in `X-MS-Identity-ObjectId` header

## Next Steps

1. Create `tasks.md` with implementation breakdown
2. Update `apim-consumption.bicep` with JWT parameters
3. Test policy XML syntax in Azure portal first (validation)
4. Implement Bicep changes
5. Deploy and test
6. Document in decision record
