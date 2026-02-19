targetScope = 'resourceGroup'

param location string = resourceGroup().location
param environment string = 'dev'
param suffix string
param tags object = {
  environment: environment
  project: 'hl7-endpoint-mirror'
  managedBy: 'bicep'
}

// ── Resource names ────────────────────────────────────────────────
var storageAccountName = 'sthl7mirror${suffix}${environment}'
var appInsightsName = 'appi-hl7-mirror-${environment}'
var logAnalyticsName = 'log-hl7-mirror-${environment}'
var keyVaultName = 'kv-hl7-${suffix}-${environment}'
var appConfigName = 'appcs-hl7-mirror-${environment}'
var functionAppName = 'func-hl7-mirror-${suffix}-${environment}'
var staticWebAppName = 'stapp-hl7-mirror-${environment}'

// ── Storage ───────────────────────────────────────────────────────
module storage 'modules/storageAccount.bicep' = {
  name: 'storage'
  params: {
    location: location
    storageAccountName: storageAccountName
    tags: tags
  }
}

// ── Application Insights ──────────────────────────────────────────
module appInsights 'modules/appInsights.bicep' = {
  name: 'appInsights'
  params: {
    location: location
    appInsightsName: appInsightsName
    logAnalyticsWorkspaceName: logAnalyticsName
    tags: tags
  }
}

// ── Function App (no role assignments yet — principal ID not known) ─
module functionApp 'modules/functionApp.bicep' = {
  name: 'functionApp'
  params: {
    location: location
    functionAppName: functionAppName
    storageAccountName: storageAccountName
    appInsightsConnectionString: appInsights.outputs.appInsightsConnectionString
    appConfigEndpoint: appConfig.outputs.appConfigEndpoint
    tags: tags
  }
  dependsOn: [storage]
}

// ── App Configuration ─────────────────────────────────────────────
module appConfig 'modules/appConfiguration.bicep' = {
  name: 'appConfig'
  params: {
    location: location
    appConfigName: appConfigName
    functionAppPrincipalId: functionApp.outputs.functionAppPrincipalId
    tags: tags
  }
}

// ── Key Vault ─────────────────────────────────────────────────────
module keyVault 'modules/keyVault.bicep' = {
  name: 'keyVault'
  params: {
    location: location
    keyVaultName: keyVaultName
    functionAppPrincipalId: functionApp.outputs.functionAppPrincipalId
    tags: tags
  }
}

// ── Static Web App ────────────────────────────────────────────────
module staticWebApp 'modules/staticWebApp.bicep' = {
  name: 'staticWebApp'
  params: {
    location: location
    staticWebAppName: staticWebAppName
    tags: tags
  }
}

// ── Outputs ───────────────────────────────────────────────────────
output functionAppName string = functionApp.outputs.functionAppName
output functionAppHostname string = functionApp.outputs.functionAppHostname
output staticWebAppHostname string = staticWebApp.outputs.staticWebAppHostname
output appInsightsConnectionString string = appInsights.outputs.appInsightsConnectionString
output appConfigEndpoint string = appConfig.outputs.appConfigEndpoint
output keyVaultUri string = keyVault.outputs.keyVaultUri
