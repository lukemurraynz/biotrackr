# Decision Record: APIM Named Values for JWT Configuration

- **Status**: Accepted
- **Deciders**: Development Team
- **Date**: 12 November 2025
- **Related Issue**: #152 - Enable APIM Managed Identity Authentication

## Context

We need to configure JWT validation in Azure API Management (APIM) for all APIs (Weight, Activity, Sleep, Food) to support Managed Identity authentication from the Blazor UI. The JWT validation requires three configuration values:

1. **OpenID Configuration URL** - `https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration`
2. **JWT Audience** - `https://management.azure.com/` (or custom App ID URI)
3. **JWT Issuer** - `https://login.microsoftonline.com/{tenantId}/v2.0`

We evaluated two approaches for managing these configuration values:

### Approach 1: String Replacement in Policy Files (Initial Implementation)
Use Bicep's `replace()` function to inject values into XML policy files during deployment:

```bicep
resource apiPolicy 'Microsoft.ApiManagement/service/apis/policies@2024-06-01-preview' = {
  name: 'policy'
  parent: api
  properties: {
    format: 'rawxml'
    value: replace(
      replace(
        replace(
          loadTextContent('policy-jwt-auth.xml'),
          '{{OPENID_CONFIG_URL}}', openidConfigUrl
        ),
        '{{JWT_AUDIENCE}}', jwtAudience
      ),
      '{{JWT_ISSUER}}', jwtIssuer
    )
  }
}
```

**Issues encountered:**
- Empty `tenantId` parameter caused double-slash in URLs: `https://login.microsoftonline.com//v2.0/`
- GitHub Actions secrets couldn't be passed through job outputs
- Complex nested `replace()` calls reduced readability
- Required Bicep linter suppressions (`#disable-next-line prefer-interpolation`)
- No runtime visibility into configuration values
- Couldn't update values without Bicep redeployment

### Approach 2: APIM Named Values (Chosen Solution)
Use APIM's native Named Values feature to store configuration centrally and reference them in policies:

```bicep
// Create Named Values in APIM
resource openidConfigUrlNamedValue 'Microsoft.ApiManagement/service/namedValues@2024-06-01-preview' = {
  name: 'openid-config-url'
  parent: apim
  properties: {
    value: '${environment().authentication.loginEndpoint}${tenantId}/v2.0/.well-known/openid-configuration'
  }
}

// Reference in policy XML
resource apiPolicy 'Microsoft.ApiManagement/service/apis/policies@2024-06-01-preview' = {
  name: 'policy'
  parent: api
  properties: {
    format: 'rawxml'
    value: loadTextContent('policy-jwt-auth.xml')  // No string replacement needed
  }
}
```

Policy XML references Named Values using APIM's native syntax:
```xml
<validate-jwt header-name="Authorization">
  <openid-config url="{{openid-config-url}}" />
  <audiences>
    <audience>{{jwt-audience}}</audience>
  </audiences>
  <issuers>
    <issuer>{{jwt-issuer}}</issuer>
  </issuers>
</validate-jwt>
```

## Decision

**We will use APIM Named Values to manage JWT configuration** instead of Bicep string replacement.

## Rationale

### Technical Benefits

1. **APIM-Native Approach**
   - Named Values are a first-class APIM feature designed for configuration management
   - Uses `{{named-value}}` syntax that APIM resolves at runtime
   - No Bicep workarounds or linter suppressions needed

2. **Runtime Flexibility**
   - Named Values can be updated through Azure Portal without Bicep redeployment
   - Useful for troubleshooting and quick configuration changes
   - Values visible in APIM Portal for debugging

3. **Cleaner Code**
   - Eliminates nested `replace()` function calls
   - Policy XML files contain readable `{{named-value}}` references
   - Bicep templates are more maintainable

4. **Better Secret Handling**
   - Named Values support secret storage (though not used in this implementation)
   - Can be sourced from Key Vault if needed
   - Easier to rotate secrets without code changes

5. **Centralized Configuration**
   - Single source of truth for JWT config in APIM
   - All APIs reference the same Named Values
   - Consistent configuration across environments

### Workflow Template Enhancement

To pass `tenantId` from GitHub Actions secrets to Bicep, we enhanced workflow templates with a parameter preparation step:

```yaml
- name: Prepare Parameters with Tenant ID
  id: prepare-params
  shell: bash
  run: |
    if [ -n "${{ inputs.parameters }}" ]; then
      # Merge user parameters with tenantId
      PARAMS=$(echo '${{ inputs.parameters }}' | jq -c '. + {"tenantId": "${{ secrets.tenant-id }}"}')
    else
      # Only tenantId
      PARAMS='{"tenantId": "${{ secrets.tenant-id }}"}'
    fi
    echo "params=$PARAMS" >> "$GITHUB_OUTPUT"

- uses: azure/bicep-deploy@v2
  with:
    parameters: ${{ steps.prepare-params.outputs.params }}
```

This solved the "double-slash" issue by ensuring `tenantId` is always populated from secrets.

## Consequences

### Positive
- **Simpler Bicep code** - Removed nested `replace()` calls and linter suppressions
- **Better debugging** - Named Values visible in Azure Portal
- **Runtime updates** - Can change values without redeployment
- **Multi-cloud ready** - Uses `environment().authentication.loginEndpoint` for Azure environments
- **Consistent pattern** - All 4 APIs use the same approach

### Negative
- **Additional resources** - Creates 3 Named Values per APIM instance (minimal cost impact)
- **Two-step deployment** - Named Values must exist before policies can reference them (handled by Bicep module dependencies)

### Neutral
- **Learning curve** - Team needs to understand Named Values feature
- **Template updates** - Workflow templates now inject `tenantId` from secrets (one-time change)

## Implementation Details

### Module Structure
Created `infra/modules/apim/apim-named-values.bicep`:
- Conditionally creates Named Values when `enableManagedIdentityAuth = true`
- Uses `environment().authentication.loginEndpoint` for multi-cloud support
- Returns Named Value names as outputs (for dependency management)

### Affected Files
- **Created**: `infra/modules/apim/apim-named-values.bicep`
- **Updated**: All 4 API Bicep files (`infra/apps/*/main.bicep`)
- **Updated**: All 4 policy XML files (`infra/apps/*/policy-jwt-auth.xml`)
- **Updated**: 3 workflow templates (`template-bicep-validate.yml`, `template-bicep-whatif.yml`, `template-bicep-deploy.yml`)
- **Updated**: All 4 API workflows (`deploy-*-api.yml`)

### Validation
```bash
# Verify Named Values created
az rest --method get \
  --url "/subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.ApiManagement/service/{apim}/namedValues?api-version=2024-06-01-preview" \
  --query "value[?contains(name, 'jwt') || contains(name, 'openid')]"

# Test JWT authentication
TOKEN=$(az account get-access-token --resource https://management.azure.com/ --query accessToken -o tsv)
curl -H "Authorization: Bearer $TOKEN" https://api-biotrackr-dev.azure-api.net/weight/api/weight
```

## Alternatives Considered

### Alternative 1: Keep String Replacement with Better Secret Handling
- **Rejected**: Still requires complex `replace()` calls and linter suppressions
- Doesn't solve runtime visibility or update flexibility issues

### Alternative 2: Policy Fragments
- **Considered**: APIM Policy Fragments for reusable policy sections
- **Decision**: Named Values are simpler for configuration values (vs. entire policy blocks)
- Policy Fragments better suited for common policy logic, not configuration

### Alternative 3: Azure Key Vault Integration
- **Future Enhancement**: Named Values can reference Key Vault secrets
- Not needed for current implementation (JWT config is not sensitive)
- Could be added later for truly secret values (API keys, connection strings)

## Follow-up Actions

- [x] Create Named Values module (`apim-named-values.bicep`)
- [x] Update all 4 API Bicep files to use module
- [x] Update all 4 policy XML files with `{{named-value}}` syntax
- [x] Enhance workflow templates with parameter preparation
- [x] Test deployment and JWT validation
- [x] Verify Named Values in Azure Portal
- [ ] Apply same pattern to future APIs (Food, Auth services)
- [ ] Document Named Values pattern in project README

## References
- [APIM Named Values Documentation](https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-properties)
- [APIM Policy Expressions](https://learn.microsoft.com/en-us/azure/api-management/api-management-policy-expressions)
- [validate-jwt Policy Reference](https://learn.microsoft.com/en-us/azure/api-management/validate-jwt-policy)
- [GitHub Actions: Reusable Workflows](https://docs.github.com/en/actions/using-workflows/reusing-workflows)
- Issue #152: Enable APIM Managed Identity Authentication
- PR #154: feat: Add JWT validation to APIM for managed identity authentication
