metadata name = 'API Management Named Values'
metadata description = 'Creates Named Values in APIM for JWT validation configuration'

@description('The name of the API Management instance')
param apimName string

@description('The Azure AD tenant ID for JWT validation')
param tenantId string

@description('The JWT audience (default: Azure Management API)')
param jwtAudience string

@description('Whether to create the Named Values (controls conditional deployment)')
param enableManagedIdentityAuth bool

resource apim 'Microsoft.ApiManagement/service@2024-06-01-preview' existing = {
  name: apimName
}

// OpenID Configuration URL Named Value
resource openidConfigUrlNamedValue 'Microsoft.ApiManagement/service/namedValues@2024-06-01-preview' = if (enableManagedIdentityAuth) {
  name: 'openid-config-url'
  parent: apim
  properties: {
    displayName: 'openid-config-url'
    value: '${environment().authentication.loginEndpoint}${tenantId}/v2.0/.well-known/openid-configuration'
    secret: false
    tags: ['jwt', 'authentication']
  }
}

// JWT Audience Named Value
resource jwtAudienceNamedValue 'Microsoft.ApiManagement/service/namedValues@2024-06-01-preview' = if (enableManagedIdentityAuth) {
  name: 'jwt-audience'
  parent: apim
  properties: {
    displayName: 'jwt-audience'
    value: jwtAudience
    secret: false
    tags: ['jwt', 'authentication']
  }
}

// JWT Issuer Named Value
resource jwtIssuerNamedValue 'Microsoft.ApiManagement/service/namedValues@2024-06-01-preview' = if (enableManagedIdentityAuth) {
  name: 'jwt-issuer'
  parent: apim
  properties: {
    displayName: 'jwt-issuer'
    value: '${environment().authentication.loginEndpoint}${tenantId}/v2.0'
    secret: false
    tags: ['jwt', 'authentication']
  }
}

@description('The name of the OpenID Configuration URL Named Value')
output openidConfigUrlName string = enableManagedIdentityAuth ? openidConfigUrlNamedValue.name : ''

@description('The name of the JWT Audience Named Value')
output jwtAudienceName string = enableManagedIdentityAuth ? jwtAudienceNamedValue.name : ''

@description('The name of the JWT Issuer Named Value')
output jwtIssuerName string = enableManagedIdentityAuth ? jwtIssuerNamedValue.name : ''
