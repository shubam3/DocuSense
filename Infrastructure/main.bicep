@description('The name of the resource group')
param resourceGroupName string

@description('The location for all resources')
param location string = resourceGroup().location

@description('The name of the application')
param applicationName string = 'azure-doc-iq'

@description('The environment (dev, staging, prod)')
param environment string = 'dev'

@description('The SKU for the App Service Plan')
param appServicePlanSku string = 'B1'

@description('The SKU for the Azure SQL Database')
param sqlDatabaseSku string = 'Basic'

@description('The admin username for SQL Server')
param sqlAdminUsername string

@description('The admin password for SQL Server')
@secure()
param sqlAdminPassword string

@description('The Azure AD B2C tenant name')
param b2cTenantName string

@description('The Azure AD B2C application client ID')
param b2cClientId string

// Variables
var appName = '${applicationName}-${environment}'
var storageAccountName = '${replace(applicationName, '-', '')}${environment}'
var sqlServerName = '${applicationName}-sql-${environment}'
var keyVaultName = '${applicationName}-kv-${environment}'
var cognitiveServicesName = '${applicationName}-cs-${environment}'
var functionAppName = '${applicationName}-func-${environment}'
var cosmosDbName = '${applicationName}-cosmos-${environment}'

// Resource Group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: resourceGroupName
  location: location
}

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2021-09-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
  }
}

// Storage Account Blob Service
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2021-09-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: [
        {
          allowedOrigins: ['*']
          allowedMethods: ['GET', 'POST', 'PUT', 'DELETE']
          allowedHeaders: ['*']
          exposedHeaders: ['*']
          maxAgeInSeconds: 86400
        }
      ]
    }
  }
}

// Storage Account Container
resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-09-01' = {
  parent: blobService
  name: 'documents'
  properties: {
    publicAccess: 'None'
  }
}

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
  }
}

// SQL Server Firewall Rule
resource sqlFirewallRule 'Microsoft.Sql/servers/firewallRules@2021-11-01' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// SQL Database
resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-11-01' = {
  parent: sqlServer
  name: 'DocuSense'
  location: location
  sku: {
    name: sqlDatabaseSku
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }
}

// Cognitive Services
resource cognitiveServices 'Microsoft.CognitiveServices/accounts@2021-10-01' = {
  name: cognitiveServicesName
  location: location
  sku: {
    name: 'S0'
  }
  kind: 'CognitiveServices'
  properties: {
    customSubDomainName: cognitiveServicesName
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: '${appName}-plan'
  location: location
  sku: {
    name: appServicePlanSku
    tier: appServicePlanSku == 'F1' ? 'Free' : 'Basic'
  }
  properties: {
    reserved: false
  }
}

// Web App
resource webApp 'Microsoft.Web/sites@2021-03-01' = {
  name: appName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageAccount.properties.primaryEndpoints.blob
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: storageAccount.properties.primaryEndpoints.blob
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: appName
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabase.name};Persist Security Info=False;User ID=${sqlAdminUsername};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
        {
          name: 'Azure__BlobStorage__ConnectionString'
          value: storageAccount.properties.primaryEndpoints.blob
        }
        {
          name: 'Azure__CognitiveServices__FormRecognizer__Endpoint'
          value: cognitiveServices.properties.endpoint
        }
        {
          name: 'Azure__CognitiveServices__FormRecognizer__Key'
          value: listKeys('${cognitiveServices.id}/listKeys', '2021-10-01').key1
        }
        {
          name: 'Azure__KeyVault__Url'
          value: keyVault.properties.vaultUri
        }
        {
          name: 'AzureAdB2C__Instance'
          value: 'https://${b2cTenantName}.b2clogin.com/'
        }
        {
          name: 'AzureAdB2C__Domain'
          value: '${b2cTenantName}.onmicrosoft.com'
        }
        {
          name: 'AzureAdB2C__ClientId'
          value: b2cClientId
        }
      ]
    }
  }
}

// Function App
resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: storageAccount.properties.primaryEndpoints.blob
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: storageAccount.properties.primaryEndpoints.blob
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: functionAppName
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabase.name};Persist Security Info=False;User ID=${sqlAdminUsername};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
        {
          name: 'Azure__BlobStorage__ConnectionString'
          value: storageAccount.properties.primaryEndpoints.blob
        }
        {
          name: 'Azure__CognitiveServices__FormRecognizer__Endpoint'
          value: cognitiveServices.properties.endpoint
        }
        {
          name: 'Azure__CognitiveServices__FormRecognizer__Key'
          value: listKeys('${cognitiveServices.id}/listKeys', '2021-10-01').key1
        }
      ]
    }
  }
}

// Cosmos DB Account
resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts@2021-11-15' = {
  name: cosmosDbName
  location: location
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
    ]
  }
}

// Cosmos DB Database
resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2021-11-15' = {
  parent: cosmosDb
  name: 'DocuSense'
  properties: {
    resource: {
      id: 'DocuSense'
    }
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${appName}-ai'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: ''
  }
}

// Outputs
output webAppUrl string = webApp.properties.defaultHostName
output functionAppUrl string = functionApp.properties.defaultHostName
output storageAccountName string = storageAccount.name
output sqlServerName string = sqlServer.name
output keyVaultName string = keyVault.name
output cognitiveServicesName string = cognitiveServices.name
output cosmosDbName string = cosmosDb.name
output appInsightsName string = appInsights.name 