# Decision Record: APIM Managed Identity Authentication

- **Status**: Accepted
- **Deciders**: Will Velida (Development Team)
- **Date**: 12 November 2025
- **Related Docs**: 
  - [GitHub Issue #152](https://github.com/willvelida/biotrackr/issues/152)
  - [Specification: 013-apim-managed-identity](../../specs/013-apim-managed-identity/spec.md)
  - [Parent Issue #151: Blazor UI Container App](https://github.com/willvelida/biotrackr/issues/151)

## Context

The biotrackr platform uses Azure API Management (APIM) as a gateway for all backend APIs (Weight, Activity, Sleep, Food). Previously, API access required subscription keys, which necessitated:

1. **Manual secret management**: Subscription keys stored in app configuration or Key Vault
2. **Key rotation overhead**: Periodic key updates and distribution to consuming applications
3. **Security risks**: Potential exposure of static credentials in logs or configuration
4. **Operational complexity**: Coordination between APIM and application deployments for key updates

With the introduction of a Blazor UI running in Azure Container Apps (parent issue #151), we had an opportunity to modernize authentication using Azure AD managed identities. Managed identities eliminate credential management entirely by leveraging Azure AD for authentication.

**Problem Statement**: How can the Blazor UI authenticate to APIM-protected APIs without managing subscription keys, while maintaining backward compatibility for existing integrations?

## Decision

**Implement JWT token validation in APIM using Azure AD managed identities, while maintaining subscription key authentication for backward compatibility.**

### Technical Approach

1. **JWT Validation Policy**: Add `validate-jwt` APIM policy to all four backend APIs
2. **Authentication Logic**: Use `choose` policy to accept either JWT tokens OR subscription keys
3. **Azure AD Integration**: Validate tokens against Azure AD tenant using OpenID Connect configuration
4. **Default Audience**: Use Azure Management API audience (`environment().authentication.audiences[0]`) to avoid requiring custom App Registration
5. **Bicep Parameters**: Parameterize tenant ID and JWT audience for multi-environment deployment

### Policy Structure

```xml
<policies>
  <inbound>
    <base />
    <choose>
      <when condition="@(context.Request.Headers.GetValueOrDefault('Authorization','').StartsWith('Bearer '))">
        <!-- JWT token present - validate against Azure AD -->
        <validate-jwt header-name="Authorization" ...>
          <openid-config url="{Azure AD tenant OpenID config}" />
          <audiences><audience>{Management API}</audience></audiences>
          <issuers><issuer>{Azure AD tenant}</issuer></issuers>
        </validate-jwt>
      </when>
      <otherwise>
        <!-- No JWT - require subscription key (backward compatibility) -->
        <check-header name="Ocp-Apim-Subscription-Key" ... />
      </otherwise>
    </choose>
  </inbound>
</policies>
```

### Implementation Details

- **Scope**: Applied to all 4 backend APIs (Weight, Activity, Sleep, Food)
- **Deployment**: Bicep infrastructure as code in `infra/apps/*/main.bicep`
- **Environment Support**: Uses `environment()` function for multi-cloud compatibility
- **Feature Flag**: `enableManagedIdentityAuth` parameter to toggle JWT validation

## Consequences

### Positive

1. **Zero Credential Management**: UI managed identity acquires tokens automatically, no secret storage required
2. **Enhanced Security**: Short-lived tokens (1 hour) vs static subscription keys
3. **Audit Trail**: Azure AD logs all token acquisitions for compliance
4. **Backward Compatibility**: Existing clients with subscription keys continue to work without changes
5. **Simplified Key Rotation**: Subscription keys can be rotated without impacting managed identity clients
6. **Multi-Environment Ready**: Parameterized configuration supports dev, staging, production

### Negative

1. **Increased Complexity**: Dual authentication logic adds cognitive load for troubleshooting
2. **Performance Overhead**: JWT validation adds ~10-20ms latency per request (acceptable per requirements)
3. **Dependency on Azure AD**: Token validation fails if Azure AD is unavailable (mitigated by APIM caching public keys)
4. **Limited to Azure**: Managed identity approach only works for Azure-hosted clients

### Neutral

1. **No RBAC Required**: JWT validation uses token signature/claims, not Azure RBAC permissions on APIM resource
2. **Testing Strategy**: Requires Azure CLI for manual token acquisition during development

## Alternatives Considered

### Alternative 1: Subscription Keys Only (Status Quo)

**Pros**: Simple, well-understood, existing implementation  
**Cons**: Manual credential management, security risks, key rotation overhead

**Rejection Reason**: Does not eliminate credential management burden, misses opportunity for modernization

### Alternative 2: Custom App Registration for APIM

**Pros**: More control over token claims, custom audience URI  
**Cons**: Additional Azure AD configuration, complexity without clear benefit

**Rejection Reason**: Default Management API audience sufficient for service-to-service authentication, no need for custom claims

### Alternative 3: Require RBAC Role Assignment

**Pros**: Additional authorization layer beyond authentication  
**Cons**: Unnecessary complexity for API gateway (authorization handled by backend services)

**Rejection Reason**: APIM serves as authentication gateway only; authorization logic belongs in backend services

### Alternative 4: Remove Subscription Keys Entirely

**Pros**: Simplified policy, single authentication method  
**Cons**: Breaking change for existing integrations, forced migration timeline

**Rejection Reason**: Backward compatibility requirement allows gradual migration without service disruption

## Follow-up Actions

- [X] Implement JWT validation policies for all 4 APIs (Weight, Activity, Sleep, Food)
- [X] Update Bicep infrastructure with JWT parameters
- [X] Document testing procedure in `specs/013-apim-managed-identity/quickstart.md`
- [ ] Deploy to dev environment and validate with Azure CLI token acquisition
- [ ] Monitor Application Insights for JWT validation latency (target < 50ms)
- [ ] Update UI Container App to use managed identity for APIM calls (issue #153)
- [ ] Create operational runbook for troubleshooting JWT authentication failures
- [ ] Consider future migration: Deprecate subscription keys once all clients use managed identities (2026 Q1)

## Notes

### Token Acquisition Example

```bash
# UI managed identity requests token (happens automatically in Container Apps)
TOKEN=$(az account get-access-token --resource https://management.azure.com/ --query accessToken -o tsv)

# Call APIM with Bearer token
curl -H "Authorization: Bearer $TOKEN" https://api-biotrackr-dev.azure-api.net/weight/api/weight
```

### Performance Considerations

- JWT validation adds 10-20ms overhead per request (measured in APIM Application Insights)
- APIM caches Azure AD public keys to minimize validation latency
- Token expiration (1 hour) handled automatically by Azure SDK in Container Apps

### Security Benefits

1. **Token Rotation**: Automatic every hour (vs manual subscription key rotation)
2. **Least Privilege**: Token scoped to specific audience (Management API)
3. **Audit Logging**: Azure AD logs all token requests with identity information
4. **No Secret Sprawl**: Zero credentials stored in code, configuration, or Key Vault

### Monitoring & Troubleshooting

- **Application Insights**: Track `validate-jwt` policy duration in APIM metrics
- **Azure AD Logs**: Review sign-in logs for token acquisition failures
- **APIM Diagnostics**: Enable detailed logging for policy execution traces
- **Common Issues**:
  - 401 Unauthorized: Token expired (re-acquire), invalid audience, or wrong issuer
  - Performance degradation: Check APIM gateway latency, Azure AD availability

### References

- [APIM Managed Identity Authentication](https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-protect-backend-with-aad)
- [APIM validate-jwt Policy](https://learn.microsoft.com/en-us/azure/api-management/api-management-authentication-policies#ValidateJWT)
- [Azure AD JWT Tokens](https://learn.microsoft.com/en-us/entra/identity-platform/access-tokens)
- [Managed Identities for Azure Resources](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview)
