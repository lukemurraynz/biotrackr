# Implementation Tasks: APIM Managed Identity Authentication

**Feature**: 013-apim-managed-identity  
**Branch**: `013-apim-managed-identity`  
**Estimated Total**: 4-6 hours

## Task Execution Rules

- **Sequential**: Tasks must be completed in order within each phase unless marked [P]
- **Parallel [P]**: Tasks marked [P] can be executed simultaneously with other [P] tasks
- **Dependencies**: Each task lists prerequisites that must be completed first
- **Validation**: Each task includes acceptance criteria for verification

---

## Phase 0: Setup (30 minutes)

### Task 0.1: Verify APIM Instance Exists
**ID**: `013-apim-001`  
**Dependencies**: None  
**Files**: N/A  
**Description**: Confirm APIM instance is deployed and accessible

**Steps**:
1. Run: `az apim list --resource-group rg-biotrackr-dev --query "[].{name:name,state:state}" -o table`
2. Verify output shows APIM instance in "Active" state
3. Note APIM instance name for subsequent tasks

**Acceptance Criteria**:
- [ ] APIM instance exists in resource group
- [ ] APIM instance state is "Active"
- [ ] APIM instance name documented

**Estimated Time**: 5 minutes

---

### Task 0.2: Get Azure AD Tenant ID
**ID**: `013-apim-002`  
**Dependencies**: None  
**Files**: N/A  
**Description**: Retrieve tenant ID for JWT issuer configuration

**Steps**:
1. Run: `az account show --query tenantId -o tsv`
2. Save tenant ID for use in Bicep parameters

**Acceptance Criteria**:
- [ ] Tenant ID retrieved successfully
- [ ] Tenant ID documented for parameter configuration

**Estimated Time**: 5 minutes

---

### Task 0.3: Review Existing APIM Bicep Module
**ID**: `013-apim-003`  
**Dependencies**: None  
**Files**: 
- `infra/modules/apim/apim-consumption.bicep`

**Description**: Analyze current APIM Bicep structure to plan changes

**Steps**:
1. Read `infra/modules/apim/apim-consumption.bicep`
2. Identify API resource definitions (Weight, Activity, Sleep, Food)
3. Document current policy structure
4. Identify where to add JWT validation parameters

**Acceptance Criteria**:
- [ ] Current APIM module structure understood
- [ ] API resources identified
- [ ] Policy injection points identified

**Estimated Time**: 20 minutes

---

## Phase 1: Bicep Infrastructure Updates (2-3 hours)

### Task 1.1: Add JWT Validation Parameters to APIM Module
**ID**: `013-apim-004`  
**Dependencies**: `013-apim-003`  
**Files**: 
- `infra/modules/apim/apim-consumption.bicep`

**Description**: Add Bicep parameters for JWT validation configuration

**Changes**:
```bicep
@description('Enable JWT validation for managed identity authentication')
param enableManagedIdentityAuth bool = true

@description('Azure AD tenant ID for JWT issuer validation')
param tenantId string

@description('JWT audience to validate (default: Azure Management API)')
param jwtAudience string = 'https://management.azure.com/'
```

**Acceptance Criteria**:
- [ ] Parameters added to APIM module
- [ ] Parameter descriptions are clear
- [ ] Default values are appropriate
- [ ] Bicep syntax is valid (run `az bicep build`)

**Estimated Time**: 15 minutes

---

### Task 1.2: Create JWT Validation Policy Fragment
**ID**: `013-apim-005`  
**Dependencies**: `013-apim-004`  
**Files**: 
- `infra/modules/apim/apim-consumption.bicep`

**Description**: Define reusable JWT policy XML as Bicep variable

**Changes**:
```bicep
var jwtValidationPolicy = enableManagedIdentityAuth ? '''
<validate-jwt header-name="Authorization" failed-validation-httpcode="401" failed-validation-error-message="Unauthorized: Invalid or missing JWT token">
    <openid-config url="https://login.microsoftonline.com/${tenantId}/v2.0/.well-known/openid-configuration" />
    <audiences>
        <audience>${jwtAudience}</audience>
    </audiences>
    <issuers>
        <issuer>https://sts.windows.net/${tenantId}/</issuer>
    </issuers>
</validate-jwt>
''' : ''
```

**Acceptance Criteria**:
- [ ] Policy XML syntax is valid
- [ ] Tenant ID is properly interpolated
- [ ] Audience is parameterized
- [ ] Policy only applies when `enableManagedIdentityAuth` is true

**Estimated Time**: 30 minutes

---

### Task 1.3: Create Authentication Choose Logic
**ID**: `013-apim-006`  
**Dependencies**: `013-apim-005`  
**Files**: 
- `infra/modules/apim/apim-consumption.bicep`

**Description**: Implement choose policy for JWT or subscription key authentication

**Changes**:
```bicep
var authenticationPolicy = '''
<inbound>
    <base />
    <choose>
        <when condition="@(context.Request.Headers.GetValueOrDefault("Authorization","").StartsWith("Bearer "))">
            ${jwtValidationPolicy}
        </when>
        <otherwise>
            <check-header name="Ocp-Apim-Subscription-Key" failed-check-httpcode="401" failed-check-error-message="Unauthorized: Missing or invalid subscription key" />
        </otherwise>
    </choose>
</inbound>
'''
```

**Acceptance Criteria**:
- [ ] Choose logic prioritizes JWT when Authorization header present
- [ ] Falls back to subscription key check when no JWT
- [ ] Error messages are clear
- [ ] Policy XML is valid

**Estimated Time**: 30 minutes

---

### Task 1.4: [P] Apply Policy to Weight API
**ID**: `013-apim-007`  
**Dependencies**: `013-apim-006`  
**Files**: 
- `infra/apps/weight-api/main.bicep`

**Description**: Update Weight API resource with authentication policy

**Changes**:
Update Weight API resource definition to include policy

**Acceptance Criteria**:
- [X] Weight API policy includes authentication logic
- [X] Policy is properly formatted XML
- [X] Bicep compiles without errors

**Estimated Time**: 15 minutes
**Status**: ✅ COMPLETED

---

### Task 1.5: [P] Apply Policy to Activity API
**ID**: `013-apim-008`  
**Dependencies**: `013-apim-006`  
**Files**: 
- `infra/modules/apim/apim-consumption.bicep`

**Description**: Update Activity API resource with authentication policy

**Changes**:
Update Activity API resource definition to include policy

**Acceptance Criteria**:
- [ ] Activity API policy includes authentication logic
- [ ] Policy is properly formatted XML
- [ ] Bicep compiles without errors

**Estimated Time**: 15 minutes

---

### Task 1.6: [P] Apply Policy to Sleep API
**ID**: `013-apim-009`  
**Dependencies**: `013-apim-006`  
**Files**: 
- `infra/modules/apim/apim-consumption.bicep`

**Description**: Update Sleep API resource with authentication policy

**Changes**:
Update Sleep API resource definition to include policy

**Acceptance Criteria**:
- [ ] Sleep API policy includes authentication logic
- [ ] Policy is properly formatted XML
- [ ] Bicep compiles without errors

**Estimated Time**: 15 minutes

---

### Task 1.7: [P] Apply Policy to Food API
**ID**: `013-apim-010`  
**Dependencies**: `013-apim-006`  
**Files**: 
- `infra/modules/apim/apim-consumption.bicep`

**Description**: Update Food API resource with authentication policy

**Changes**:
Update Food API resource definition to include policy

**Acceptance Criteria**:
- [ ] Food API policy includes authentication logic
- [ ] Policy is properly formatted XML
- [ ] Bicep compiles without errors

**Estimated Time**: 15 minutes

---

### Task 1.8: Update Main Bicep to Pass JWT Parameters
**ID**: `013-apim-011`  
**Dependencies**: `013-apim-007`, `013-apim-008`, `013-apim-009`, `013-apim-010`  
**Files**: 
- `infra/core/main.bicep`

**Description**: Update main Bicep to accept and pass JWT parameters to APIM module

**Changes**:
```bicep
@description('Enable JWT validation for managed identity authentication')
param enableManagedIdentityAuth bool = true

@description('Azure AD tenant ID for JWT issuer validation')
param tenantId string

module apimModule 'modules/apim/apim-consumption.bicep' = {
  name: 'apim-deployment'
  params: {
    enableManagedIdentityAuth: enableManagedIdentityAuth
    tenantId: tenantId
    jwtAudience: 'https://management.azure.com/'
    // ... other existing params
  }
}
```

**Acceptance Criteria**:
- [ ] Parameters added to main.bicep
- [ ] Parameters passed to APIM module
- [ ] Bicep compiles without errors

**Estimated Time**: 15 minutes

---

### Task 1.9: Update Dev Parameter File
**ID**: `013-apim-012`  
**Dependencies**: `013-apim-011`  
**Files**: 
- `infra/core/main.dev.bicepparam`

**Description**: Add JWT configuration values for dev environment

**Changes**:
```bicep
using 'main.bicep'

param enableManagedIdentityAuth = true
param tenantId = '<tenant-id-from-task-0.2>'
// ... other existing params
```

**Acceptance Criteria**:
- [ ] Dev parameter file includes JWT configuration
- [ ] Tenant ID is correct for dev environment
- [ ] Parameter file is valid Bicep syntax

**Estimated Time**: 10 minutes

---

## Phase 2: Deployment & Testing (1-2 hours)

### Task 2.1: Deploy Infrastructure to Dev Environment
**ID**: `013-apim-013`  
**Dependencies**: `013-apim-012`  
**Files**: N/A

**Description**: Deploy updated APIM infrastructure with JWT validation

**Steps**:
1. Run Bicep validation: `az bicep build --file infra/core/main.bicep`
2. Run what-if analysis: `az deployment group what-if --resource-group rg-biotrackr-dev --template-file infra/core/main.bicep --parameters infra/core/main.dev.bicepparam`
3. Deploy: `az deployment group create --resource-group rg-biotrackr-dev --template-file infra/core/main.bicep --parameters infra/core/main.dev.bicepparam`

**Acceptance Criteria**:
- [ ] Bicep validation passes
- [ ] What-if shows expected policy updates
- [ ] Deployment completes successfully
- [ ] No deployment errors in output

**Estimated Time**: 20 minutes

---

### Task 2.2: Test JWT Authentication - Weight API
**ID**: `013-apim-014`  
**Dependencies**: `013-apim-013`  
**Files**: N/A

**Description**: Verify Weight API accepts JWT tokens

**Steps**:
1. Acquire JWT: `TOKEN=$(az account get-access-token --resource https://management.azure.com/ --query accessToken -o tsv)`
2. Call API: `curl -H "Authorization: Bearer $TOKEN" https://<apim>.azure-api.net/weight/api/weight`
3. Verify 200 OK response

**Acceptance Criteria**:
- [ ] JWT token acquired successfully
- [ ] API returns 200 OK with valid token
- [ ] API returns 401 Unauthorized with invalid token
- [ ] Response contains expected data

**Estimated Time**: 10 minutes

---

### Task 2.3: [P] Test JWT Authentication - Activity API
**ID**: `013-apim-015`  
**Dependencies**: `013-apim-013`  
**Files**: N/A

**Description**: Verify Activity API accepts JWT tokens

**Steps**:
Same as Task 2.2, replace endpoint with Activity API

**Acceptance Criteria**:
- [ ] API returns 200 OK with valid JWT token
- [ ] API returns 401 Unauthorized with invalid token

**Estimated Time**: 10 minutes

---

### Task 2.4: [P] Test JWT Authentication - Sleep API
**ID**: `013-apim-016`  
**Dependencies**: `013-apim-013`  
**Files**: N/A

**Description**: Verify Sleep API accepts JWT tokens

**Steps**:
Same as Task 2.2, replace endpoint with Sleep API

**Acceptance Criteria**:
- [ ] API returns 200 OK with valid JWT token
- [ ] API returns 401 Unauthorized with invalid token

**Estimated Time**: 10 minutes

---

### Task 2.5: [P] Test JWT Authentication - Food API
**ID**: `013-apim-017`  
**Dependencies**: `013-apim-013`  
**Files**: N/A

**Description**: Verify Food API accepts JWT tokens

**Steps**:
Same as Task 2.2, replace endpoint with Food API

**Acceptance Criteria**:
- [ ] API returns 200 OK with valid JWT token
- [ ] API returns 401 Unauthorized with invalid token

**Estimated Time**: 10 minutes

---

### Task 2.6: Test Subscription Key Backward Compatibility
**ID**: `013-apim-018`  
**Dependencies**: `013-apim-014`, `013-apim-015`, `013-apim-016`, `013-apim-017`  
**Files**: N/A

**Description**: Verify subscription key authentication still works

**Steps**:
1. Get subscription key: `KEY=$(az apim subscription list --resource-group rg-biotrackr-dev --service-name <apim> --query "[0].primaryKey" -o tsv)`
2. Call each API with subscription key header
3. Verify 200 OK responses

**Acceptance Criteria**:
- [ ] All 4 APIs accept subscription key authentication
- [ ] Responses are identical to JWT authentication
- [ ] No regression in existing functionality

**Estimated Time**: 15 minutes

---

### Task 2.7: Test Authentication Failure Scenarios
**ID**: `013-apim-019`  
**Dependencies**: `013-apim-018`  
**Files**: N/A

**Description**: Verify proper error handling for invalid authentication

**Steps**:
1. Call API with no authentication → expect 401
2. Call API with expired JWT → expect 401
3. Call API with invalid subscription key → expect 401
4. Call API with malformed Authorization header → expect 401

**Acceptance Criteria**:
- [ ] All invalid authentication attempts return 401
- [ ] Error messages are clear and informative
- [ ] No stack traces or sensitive information exposed

**Estimated Time**: 15 minutes

---

### Task 2.8: Measure JWT Validation Performance Impact
**ID**: `013-apim-020`  
**Dependencies**: `013-apim-019`  
**Files**: N/A

**Description**: Verify JWT validation latency is acceptable

**Steps**:
1. Navigate to APIM → Application Insights
2. View "Performance" metrics
3. Compare p50, p95, p99 latency before/after JWT validation
4. Verify < 50ms overhead

**Acceptance Criteria**:
- [ ] JWT validation adds < 50ms to p95 latency
- [ ] No significant performance degradation
- [ ] Application Insights shows authentication metrics

**Estimated Time**: 10 minutes

---

## Phase 3: Documentation (30-60 minutes)

### Task 3.1: Create Decision Record
**ID**: `013-apim-021`  
**Dependencies**: `013-apim-020`  
**Files**: 
- `docs/decision-records/2025-11-12-apim-managed-identity-auth.md`

**Description**: Document architectural decision for APIM JWT validation

**Content Structure**:
- Context: Why we need managed identity authentication
- Decision: Use validate-jwt policy with Azure AD
- Alternatives Considered: Custom auth middleware, subscription keys only
- Consequences: Security improvement, maintenance reduction
- Implementation Details: Policy structure, parameters

**Acceptance Criteria**:
- [ ] Decision record follows project template
- [ ] Context clearly explains the problem
- [ ] Decision rationale is documented
- [ ] Alternatives are listed with reasons for rejection
- [ ] Consequences (positive and negative) are identified

**Estimated Time**: 30 minutes

---

### Task 3.2: Update Main README with APIM Authentication Info
**ID**: `013-apim-022`  
**Dependencies**: `013-apim-021`  
**Files**: 
- `README.md`

**Description**: Add brief note about APIM authentication in Tech Stack section

**Changes**:
Under "Infrastructure" section, add:
- **Azure API Management**: API gateway with JWT validation for managed identity authentication

**Acceptance Criteria**:
- [ ] README mentions APIM authentication capability
- [ ] Description is concise and user-friendly
- [ ] Links to decision record

**Estimated Time**: 10 minutes

---

### Task 3.3: Update GitHub Issue #152 with Completion Status
**ID**: `013-apim-023`  
**Dependencies**: `013-apim-022`  
**Files**: N/A

**Description**: Mark acceptance criteria complete in issue #152

**Steps**:
1. Navigate to GitHub issue #152
2. Check off all completed acceptance criteria:
   - [X] validate-jwt policy added to all APIM APIs
   - [X] APIM Bicep module updated to support JWT validation configuration
   - [X] Successfully tested APIM call with Managed Identity token via Azure CLI
   - [X] Existing subscription key authentication still functional
   - [X] Documentation updated with Managed Identity authentication pattern
   - [X] Decision record created for APIM Managed Identity pattern
3. Add comment summarizing implementation

**Acceptance Criteria**:
- [ ] All acceptance criteria in issue #152 are checked
- [ ] Comment added with deployment details
- [ ] Issue ready for review/closure

**Estimated Time**: 10 minutes

---

## Summary

**Total Tasks**: 23  
**Estimated Total Time**: 4-6 hours  
**Parallel Opportunities**: Tasks 1.4-1.7 (API policy updates), Tasks 2.3-2.5 (API testing)

**Critical Path**:
1. Phase 0: Setup and research (30 min)
2. Phase 1: Bicep updates (2-3 hours)
3. Phase 2: Deployment and testing (1-2 hours)
4. Phase 3: Documentation (30-60 min)

**Success Criteria**:
- All 4 backend APIs authenticate with JWT tokens
- Subscription key authentication still works
- Performance impact < 50ms
- Documentation complete
- Issue #152 ready for closure
