# Quickstart: APIM Managed Identity Authentication

**Feature**: 013-apim-managed-identity  
**Date**: 2025-11-12

## Overview

This quickstart guide shows how to deploy and test APIM JWT validation for managed identity authentication.

## Prerequisites

- Azure CLI installed and authenticated (`az login`)
- Access to the biotrackr Azure subscription
- PowerShell 7+ (for Bicep deployment)
- Existing APIM instance deployed

## Deployment

### Step 1: Update Bicep Parameters

Edit `infra/core/main.dev.bicepparam`:

```bicep
param enableManagedIdentityAuth = true
param tenantId = '<your-azure-ad-tenant-id>'
param jwtAudience = 'https://management.azure.com/'
```

**Get your tenant ID**:
```bash
az account show --query tenantId -o tsv
```

### Step 2: Deploy Infrastructure

From repository root:

```powershell
# Deploy to dev environment
az deployment group create `
  --resource-group rg-biotrackr-dev `
  --template-file infra/core/main.bicep `
  --parameters infra/core/main.dev.bicepparam
```

**Expected output**: Deployment succeeds, APIM policies updated

### Step 3: Verify Deployment

Check that APIM policies include JWT validation:

```bash
# Get APIM instance name
az apim list --resource-group rg-biotrackr-dev --query "[0].name" -o tsv

# Export Weight API policy (example)
az apim api show --resource-group rg-biotrackr-dev \
  --service-name <apim-name> \
  --api-id weight-api \
  --query "policy" -o json
```

**Expected**: Policy XML contains `<validate-jwt>` element

## Testing

### Test 1: JWT Token Authentication

**Step 1: Acquire JWT Token**

```bash
# Get token with Azure Management audience
TOKEN=$(az account get-access-token --resource https://management.azure.com/ --query accessToken -o tsv)
echo $TOKEN
```

**Step 2: Call APIM Endpoint with JWT**

```bash
# Replace <apim-name> with your APIM instance name
curl -H "Authorization: Bearer $TOKEN" \
  https://<apim-name>.azure-api.net/weight/api/weight
```

**Expected Result**: 
- Status: 200 OK
- Response: JSON data from Weight API

**Troubleshooting**:
- **401 Unauthorized**: Token expired or invalid audience
  - Re-acquire token: `TOKEN=$(az account get-access-token --resource https://management.azure.com/ --query accessToken -o tsv)`
- **403 Forbidden**: Backend authorization issue (separate from APIM auth)
- **500 Internal Server Error**: Check APIM policy syntax in Azure portal

### Test 2: Subscription Key Authentication (Backward Compatibility)

**Step 1: Get Subscription Key**

```bash
# Get primary subscription key
SUBSCRIPTION_KEY=$(az apim subscription list \
  --resource-group rg-biotrackr-dev \
  --service-name <apim-name> \
  --query "[0].primaryKey" -o tsv)
```

**Step 2: Call APIM Endpoint with Subscription Key**

```bash
curl -H "Ocp-Apim-Subscription-Key: $SUBSCRIPTION_KEY" \
  https://<apim-name>.azure-api.net/weight/api/weight
```

**Expected Result**:
- Status: 200 OK
- Response: JSON data from Weight API

### Test 3: Invalid Authentication

**Test with no credentials**:

```bash
curl -i https://<apim-name>.azure-api.net/weight/api/weight
```

**Expected Result**:
- Status: 401 Unauthorized
- Response: Error message about missing authentication

## Validation Checklist

- [ ] JWT token authentication works for Weight API
- [ ] JWT token authentication works for Activity API
- [ ] JWT token authentication works for Sleep API
- [ ] JWT token authentication works for Food API
- [ ] Subscription key authentication still works (backward compatibility)
- [ ] Invalid tokens return 401 Unauthorized
- [ ] Missing authentication returns 401 Unauthorized
- [ ] JWT validation adds < 50ms latency (check Application Insights)

## Monitoring

### Check Authentication Metrics

View APIM metrics in Azure portal:

1. Navigate to APIM instance → Metrics
2. Select metric: "Requests"
3. Add filter: "Authentication Type"
4. Observe JWT vs Subscription Key usage

### Check Latency Impact

1. Navigate to APIM instance → Application Insights
2. View "Performance" blade
3. Compare response times before/after JWT validation
4. **Expected**: < 50ms increase in p95 latency

## Rollback Procedure

If issues arise, revert JWT validation:

### Option 1: Disable via Parameter

Update `main.dev.bicepparam`:

```bicep
param enableManagedIdentityAuth = false
```

Redeploy infrastructure.

### Option 2: Remove Policy (Emergency)

Use Azure portal:

1. Navigate to APIM → APIs → [API Name] → Policies
2. Remove `<validate-jwt>` block from inbound policy
3. Save policy

**Note**: Manual changes will be overwritten by next Bicep deployment.

## Common Issues

### Issue: "Invalid JWT signature"
**Cause**: Token audience mismatch  
**Solution**: Ensure token acquired with `--resource https://management.azure.com/`

### Issue: "Token expired"
**Cause**: JWT token lifetime (typically 1 hour)  
**Solution**: Re-acquire token using `az account get-access-token`

### Issue: "Subscription key still required"
**Cause**: Policy logic error, both methods being required  
**Solution**: Verify `choose` policy logic, ensure `otherwise` handles subscription key

### Issue: "Backend service not authenticated"
**Cause**: Backend expects additional authentication beyond APIM  
**Solution**: Check backend service configuration, may need to forward claims

## Next Steps

- **For UI integration**: See parent issue #151 for UI Container App setup
- **For RBAC refinement**: Consider custom roles if default permissions too broad
- **For multi-environment**: Update staging/prod parameter files
- **For monitoring**: Set up alerts on 401 response rate spike

## References

- [APIM Managed Identity Authentication](https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-protect-backend-with-aad)
- [Azure CLI APIM commands](https://learn.microsoft.com/en-us/cli/azure/apim)
- [JWT token debugging](https://jwt.ms/) - paste token to inspect claims
