param location string
param appConfigName string
param functionAppPrincipalId string = ''
param tags object = {}

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: appConfigName
  location: location
  tags: tags
  sku: {
    name: 'free'
  }
  properties: {
    disableLocalAuth: false
  }
}

// Seed default chaos config values
resource chaosEnabled 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfig
  name: 'ChaosMode:IsEnabled'
  properties: {
    value: 'false'
  }
}

resource chaosFailureRate 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfig
  name: 'ChaosMode:FailureRatePercent'
  properties: {
    value: '10'
  }
}

resource chaosErrorType 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfig
  name: 'ChaosMode:DefaultErrorType'
  properties: {
    value: '500'
  }
}

resource chaosLatencyEnabled 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfig
  name: 'ChaosMode:LatencySimulation:IsEnabled'
  properties: {
    value: 'false'
  }
}

resource chaosLatencyDelay 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfig
  name: 'ChaosMode:LatencySimulation:DelayMs'
  properties: {
    value: '0'
  }
}

// Grant Function App managed identity read access to App Configuration
resource appConfigDataReaderRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(functionAppPrincipalId)) {
  name: guid(appConfig.id, functionAppPrincipalId, 'App Configuration Data Reader')
  scope: appConfig
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '516239f1-63e1-4d78-a4de-a74fb236a071' // App Configuration Data Reader
    )
    principalId: functionAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output appConfigEndpoint string = appConfig.properties.endpoint
output appConfigId string = appConfig.id
