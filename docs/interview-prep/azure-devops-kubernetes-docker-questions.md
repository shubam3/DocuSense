# AzureDocIQ - Azure Cloud, DevOps, Kubernetes & Docker Questions

## 🎯 Comprehensive Cloud & DevOps Q&A Guide

This document contains detailed questions and answers about Azure Cloud services, DevOps practices, Kubernetes, and Docker implementation in the AzureDocIQ project.

## ☁️ Azure Cloud Services Questions

### **Q1: Explain the Azure services used in AzureDocIQ and why you chose each one**

**Answer:**
AzureDocIQ leverages multiple Azure services for a comprehensive cloud-native solution:

**Azure App Service:**
- **Purpose**: Hosting the ASP.NET Core MVC web application
- **Why Chosen**: 
  - Built-in scaling and load balancing
  - Integrated with Azure AD B2C for authentication
  - Automatic SSL/TLS certificate management
  - Deployment slots for blue-green deployments
  - Built-in monitoring and diagnostics

**Azure Functions:**
- **Purpose**: Serverless document processing
- **Why Chosen**:
  - Event-driven architecture with blob triggers
  - Pay-per-execution pricing model
  - Automatic scaling to zero when not in use
  - Built-in retry logic and error handling
  - Seamless integration with other Azure services

**Azure SQL Database:**
- **Purpose**: Primary relational database for structured data
- **Why Chosen**:
  - Built-in high availability and disaster recovery
  - Automatic backups and point-in-time restore
  - Advanced threat protection
  - Elastic pools for cost optimization
  - Geo-replication capabilities

**Azure Cosmos DB:**
- **Purpose**: NoSQL database for audit logs and analytics
- **Why Chosen**:
  - High throughput for audit event ingestion
  - Schema flexibility for evolving requirements
  - Global distribution capabilities
  - Sub-millisecond response times
  - Built-in security and compliance

**Azure Blob Storage:**
- **Purpose**: Document file storage and processing
- **Why Chosen**:
  - Cost-effective storage for large files
  - CDN integration for global content delivery
  - Lifecycle management for cost optimization
  - Event triggers for Azure Functions
  - Geo-redundant storage options

**Azure Cognitive Services:**
- **Purpose**: AI-powered document analysis
- **Why Chosen**:
  - Pre-trained models for document processing
  - High accuracy in text extraction and form recognition
  - Multi-language support
  - Pay-per-use pricing model
  - Continuous model improvements

**Azure Key Vault:**
- **Purpose**: Secure storage of secrets and certificates
- **Why Chosen**:
  - Centralized secret management
  - Access control with RBAC
  - Audit logging for compliance
  - Integration with managed identities
  - Automatic key rotation capabilities

**Azure Application Insights:**
- **Purpose**: Application monitoring and telemetry
- **Why Chosen**:
  - Real-time application performance monitoring
  - Custom telemetry and metrics
  - Error tracking and diagnostics
  - User behavior analytics
  - Integration with Azure Monitor

### **Q2: How did you implement Azure AD B2C for authentication in AzureDocIQ?**

**Answer:**
Azure AD B2C was implemented for enterprise-grade authentication:

**Configuration Setup:**
```json
{
  "AzureAdB2C": {
    "Instance": "https://your-tenant.b2clogin.com/",
    "Domain": "your-tenant.onmicrosoft.com",
    "ClientId": "your-client-id",
    "SignUpSignInPolicyId": "B2C_1_signupsignin",
    "ResetPasswordPolicyId": "B2C_1_reset",
    "EditProfilePolicyId": "B2C_1_edit"
  }
}
```

**Service Registration:**
```csharp
services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = configuration["AzureAdB2C:Instance"] + "/" + configuration["AzureAdB2C:Domain"] + "/v2.0/";
    options.Audience = configuration["AzureAdB2C:ClientId"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        NameClaimType = "name"
    };
});
```

**User Flow Configuration:**
- **Sign-up and Sign-in**: Custom user flow with email verification
- **Password Reset**: Self-service password reset flow
- **Profile Editing**: User profile management
- **Multi-factor Authentication**: Optional MFA for enhanced security

**Benefits:**
- Social identity provider integration (Google, Facebook, Microsoft)
- Custom branding and user experience
- Comprehensive audit logging
- Compliance with security standards
- Scalable user management

### **Q3: Explain the Azure Functions architecture and implementation in AzureDocIQ**

**Answer:**
Azure Functions were implemented for serverless document processing:

**Function Types and Triggers:**
```csharp
// Blob trigger for document processing
[FunctionName("ProcessDocument")]
public async Task ProcessDocument(
    [BlobTrigger("documents/{name}")] Stream blobStream,
    string name,
    ILogger log)
{
    // Document processing logic
}

// HTTP trigger for manual processing
[FunctionName("ManualProcess")]
public async Task<IActionResult> ManualProcess(
    [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req,
    ILogger log)
{
    // Manual processing logic
}

// Timer trigger for cleanup tasks
[FunctionName("CleanupTempFiles")]
public async Task CleanupTempFiles(
    [TimerTrigger("0 0 2 * * *")] TimerInfo myTimer,
    ILogger log)
{
    // Cleanup logic
}

// Event Grid trigger for notifications
[FunctionName("SendNotification")]
public async Task SendNotification(
    [EventGridTrigger] EventGridEvent eventGridEvent,
    ILogger log)
{
    // Notification logic
}
```

**Configuration and Dependencies:**
```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=...",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "ConnectionStrings__DefaultConnection": "...",
    "Azure__CognitiveServices__FormRecognizer__Endpoint": "...",
    "Azure__CognitiveServices__FormRecognizer__Key": "..."
  }
}
```

**Benefits of This Architecture:**
- **Event-Driven**: Automatic processing when documents are uploaded
- **Scalable**: Handles variable workloads automatically
- **Cost-Effective**: Pay only for actual processing time
- **Reliable**: Built-in retry logic and error handling
- **Isolated**: Processing failures don't affect the web application

### **Q4: How did you implement Azure Key Vault for secret management?**

**Answer:**
Azure Key Vault was implemented for secure secret management:

**Key Vault Setup:**
```bash
# Create Key Vault
az keyvault create \
  --name azure-doc-iq-kv-dev \
  --resource-group azure-doc-iq-rg \
  --location eastus \
  --sku standard

# Store secrets
az keyvault secret set \
  --vault-name azure-doc-iq-kv-dev \
  --name "CognitiveServicesKey" \
  --value "your-key-here"

az keyvault secret set \
  --vault-name azure-doc-iq-kv-dev \
  --name "DatabaseConnectionString" \
  --value "your-connection-string"
```

**Application Integration:**
```csharp
// Startup configuration
services.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());

// Secret retrieval
public class SecretService : ISecretService
{
    private readonly SecretClient _secretClient;
    
    public SecretService(SecretClient secretClient)
    {
        _secretClient = secretClient;
    }
    
    public async Task<string> GetSecretAsync(string secretName)
    {
        var secret = await _secretClient.GetSecretAsync(secretName);
        return secret.Value.Value;
    }
}
```

**Managed Identity Configuration:**
```bash
# Enable managed identity for App Service
az webapp identity assign \
  --resource-group azure-doc-iq-rg \
  --name azure-doc-iq-dev

# Grant Key Vault access
az keyvault set-policy \
  --name azure-doc-iq-kv-dev \
  --resource-group azure-doc-iq-rg \
  --object-id "managed-identity-principal-id" \
  --secret-permissions get list
```

**Security Benefits:**
- Centralized secret management
- Access control with RBAC
- Audit logging for compliance
- Automatic key rotation
- Integration with managed identities

## 🚀 DevOps & CI/CD Questions

### **Q5: Explain the CI/CD pipeline implementation for AzureDocIQ**

**Answer:**
The CI/CD pipeline was implemented using Azure DevOps with multiple stages:

**Pipeline Structure:**
```yaml
# azure-pipelines.yml
trigger:
- main
- develop

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'
  dotNetVersion: '8.0.x'

stages:
- stage: Build
  displayName: 'Build and Test'
  jobs:
  - job: Build
    pool:
      vmImage: 'ubuntu-latest'
    steps:
    - task: UseDotNet@2
      inputs:
        version: '$(dotNetVersion)'
        
    - task: DotNetCoreCLI@2
      displayName: 'Restore NuGet packages'
      inputs:
        command: 'restore'
        projects: '$(solution)'
        
    - task: DotNetCoreCLI@2
      displayName: 'Build solution'
      inputs:
        command: 'build'
        projects: '$(solution)'
        arguments: '--configuration $(buildConfiguration) --no-restore'
        
    - task: DotNetCoreCLI@2
      displayName: 'Run unit tests'
      inputs:
        command: 'test'
        projects: '**/*Tests/*.csproj'
        arguments: '--configuration $(buildConfiguration) --no-build --collect "Code coverage"'
        
    - task: DotNetCoreCLI@2
      displayName: 'Publish application'
      inputs:
        command: 'publish'
        projects: '$(solution)'
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory) --no-build'
        
    - task: PublishBuildArtifacts@1
      displayName: 'Publish artifacts'
      inputs:
        pathToPublish: '$(Build.ArtifactStagingDirectory)'
        artifactName: 'drop'

- stage: DeployInfrastructure
  displayName: 'Deploy Infrastructure'
  dependsOn: Build
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
  - deployment: DeployInfrastructure
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureResourceManagerTemplateDeployment@3
            displayName: 'Deploy Bicep template'
            inputs:
              deploymentScope: 'Resource Group'
              azureResourceManagerConnection: 'Azure Subscription'
              subscriptionId: '$(AZURE_SUBSCRIPTION_ID)'
              action: 'Create Or Update Resource Group'
              resourceGroupName: '$(RESOURCE_GROUP_NAME)'
              location: '$(AZURE_LOCATION)'
              templateLocation: 'Linked artifact'
              csmFile: '$(Pipeline.Workspace)/drop/Infrastructure/main.bicep'
              csmParametersFile: '$(Pipeline.Workspace)/drop/Infrastructure/parameters.json'
              overrideParameters: '-environment $(Environment) -location $(AZURE_LOCATION)'
              deploymentMode: 'Incremental'

- stage: DeployApplication
  displayName: 'Deploy Application'
  dependsOn: DeployInfrastructure
  jobs:
  - deployment: DeployWebApp
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            displayName: 'Deploy Web App'
            inputs:
              azureSubscription: 'Azure Subscription'
              appName: '$(WEB_APP_NAME)'
              package: '$(Pipeline.Workspace)/drop/**/*.zip'
              appType: 'webApp'
              
  - deployment: DeployFunctions
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureFunctionApp@1
            displayName: 'Deploy Azure Functions'
            inputs:
              azureSubscription: 'Azure Subscription'
              appName: '$(FUNCTION_APP_NAME)'
              package: '$(Pipeline.Workspace)/drop/**/*.zip'
              appType: 'functionApp'

- stage: PostDeployment
  displayName: 'Post-Deployment Tests'
  dependsOn: DeployApplication
  jobs:
  - job: SmokeTests
    pool:
      vmImage: 'ubuntu-latest'
    steps:
    - task: DotNetCoreCLI@2
      displayName: 'Run smoke tests'
      inputs:
        command: 'test'
        projects: '**/*SmokeTests/*.csproj'
        arguments: '--configuration $(buildConfiguration) --no-build'
```

**Pipeline Features:**
- **Multi-stage deployment**: Build, Infrastructure, Application, Testing
- **Environment protection**: Production environment with approval gates
- **Infrastructure as Code**: Bicep templates for Azure resources
- **Automated testing**: Unit tests, integration tests, and smoke tests
- **Artifact management**: Secure storage and versioning of build artifacts

### **Q6: How did you implement Infrastructure as Code (IaC) for AzureDocIQ?**

**Answer:**
Infrastructure as Code was implemented using Azure Bicep:

**Main Bicep Template Structure:**
```bicep
// main.bicep
@description('Environment name (dev, staging, prod)')
param environment string

@description('Azure region for resources')
param location string = resourceGroup().location

@description('SQL Server admin username')
param sqlAdminUsername string

@description('SQL Server admin password')
@secure()
param sqlAdminPassword string

// Resource Group (already exists)
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' existing = {
  name: resourceGroup().name
}

// Storage Account
resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: 'azuredociq${environment}${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    networkAcls: {
      defaultAction: 'Deny'
      ipRules: []
      virtualNetworkRules: []
    }
  }
}

// Blob Container
resource blobContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2021-08-01' = {
  name: '${storageAccount.name}/default/documents'
  properties: {
    publicAccess: 'None'
  }
}

// SQL Server
resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: 'azure-doc-iq-sql-${environment}'
  location: location
  properties: {
    administratorLogin: sqlAdminUsername
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
    minimalTlsVersion: '1.2'
  }
}

// SQL Database
resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-11-01' = {
  name: '${sqlServer.name}/AzureDocIQ'
  location: location
  sku: {
    name: environment == 'prod' ? 'S1' : 'S0'
    tier: environment == 'prod' ? 'Standard' : 'Basic'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: environment == 'prod' ? 2147483648 : 1073741824 // 2GB or 1GB
  }
}

// Cognitive Services
resource cognitiveServices 'Microsoft.CognitiveServices/accounts@2021-10-01' = {
  name: 'azure-doc-iq-cs-${environment}'
  location: location
  sku: {
    name: 'S0'
  }
  kind: 'CognitiveServices'
  properties: {
    customSubDomainName: 'azure-doc-iq-cs-${environment}'
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
}

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2021-02-01' = {
  name: 'azure-doc-iq-plan-${environment}'
  location: location
  sku: {
    name: environment == 'prod' ? 'S1' : 'B1'
    tier: environment == 'prod' ? 'Standard' : 'Basic'
  }
  properties: {
    reserved: false
  }
}

// Web App
resource webApp 'Microsoft.Web/sites@2021-02-01' = {
  name: 'azure-doc-iq-${environment}'
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      appSettings: [
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
        {
          name: 'AzureWebJobsStorage'
          value: storageAccount.properties.primaryEndpoints.blob
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabase.name};Persist Security Info=False;User ID=${sqlAdminUsername};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
      ]
    }
  }
}

// Function App
resource functionApp 'Microsoft.Web/sites@2021-02-01' = {
  name: 'azure-doc-iq-func-${environment}'
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
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
      ]
    }
  }
}

// Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2021-06-01' = {
  name: 'azure-doc-iq-kv-${environment}'
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
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

// Application Insights
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'azure-doc-iq-ai-${environment}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// Log Analytics Workspace
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'azure-doc-iq-law-${environment}'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Outputs
output webAppUrl string = webApp.properties.defaultHostName
output functionAppUrl string = functionApp.properties.defaultHostName
output storageAccountName string = storageAccount.name
output keyVaultName string = keyVault.name
output cognitiveServicesEndpoint string = cognitiveServices.properties.endpoint
```

**Benefits of IaC:**
- **Version Control**: Infrastructure changes are tracked in Git
- **Consistency**: Same infrastructure across environments
- **Automation**: Automated deployment and updates
- **Compliance**: Infrastructure meets security and compliance requirements
- **Cost Control**: Predictable resource provisioning

### **Q7: How did you implement monitoring and alerting in the DevOps pipeline?**

**Answer:**
Monitoring and alerting were implemented using Azure Monitor and Application Insights:

**Application Insights Configuration:**
```csharp
// Startup configuration
services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
    options.EnablePerformanceCounterCollectionModule = true;
    options.EnableEventCounterCollectionModule = true;
});

// Custom telemetry
public class DocumentProcessingTelemetry
{
    private readonly TelemetryClient _telemetryClient;
    
    public DocumentProcessingTelemetry(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }
    
    public void TrackDocumentProcessed(string documentType, double processingTime, bool success)
    {
        _telemetryClient.TrackMetric("DocumentProcessingTime", processingTime, new Dictionary<string, string>
        {
            ["DocumentType"] = documentType,
            ["Success"] = success.ToString()
        });
        
        _telemetryClient.TrackEvent("DocumentProcessed", new Dictionary<string, string>
        {
            ["DocumentType"] = documentType,
            ["ProcessingTime"] = processingTime.ToString(),
            ["Success"] = success.ToString()
        });
    }
}
```

**Azure Monitor Alerts:**
```json
{
  "type": "Microsoft.Insights/alertrules",
  "name": "HighErrorRate",
  "properties": {
    "name": "High Error Rate Alert",
    "description": "Alert when error rate exceeds 5%",
    "isEnabled": true,
    "condition": {
      "odata.type": "Microsoft.Azure.Management.Insights.Models.ThresholdRuleCondition",
      "dataSource": {
        "odata.type": "Microsoft.Azure.Management.Insights.Models.RuleMetricDataSource",
        "resourceUri": "/subscriptions/{subscription-id}/resourceGroups/{resource-group}/providers/Microsoft.Web/sites/{web-app-name}",
        "metricName": "Http5xx"
      },
      "operator": "GreaterThan",
      "threshold": 5,
      "windowSize": "PT5M"
    },
    "actions": [
      {
        "odata.type": "Microsoft.Azure.Management.Insights.Models.RuleEmailAction",
        "sendToServiceOwners": true,
        "customEmails": ["admin@company.com"]
      }
    ]
  }
}
```

**Pipeline Monitoring:**
```yaml
- task: AzureMonitor@0
  displayName: 'Monitor deployment'
  inputs:
    connectedServiceName: 'Azure Subscription'
    resourceGroupName: '$(RESOURCE_GROUP_NAME)'
    resourceName: '$(WEB_APP_NAME)'
    resourceType: 'Microsoft.Web/sites'
    metricName: 'Requests'
    operator: 'GreaterThan'
    threshold: '100'
    timeWindow: '5'
```

## 🐳 Docker & Containerization Questions

### **Q8: How would you containerize the AzureDocIQ application using Docker?**

**Answer:**
The AzureDocIQ application can be containerized using Docker for consistent deployment:

**Multi-Stage Dockerfile:**
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["AzureDocIQ.csproj", "./"]
COPY ["Services/*.csproj", "./Services/"]
COPY ["Models/*.csproj", "./Models/"]
COPY ["Controllers/*.csproj", "./Controllers/"]

# Restore dependencies
RUN dotnet restore "AzureDocIQ.csproj"

# Copy source code
COPY . .

# Build application
RUN dotnet build "AzureDocIQ.csproj" -c Release -o /app/build

# Publish application
RUN dotnet publish "AzureDocIQ.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install necessary packages
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Start application
ENTRYPOINT ["dotnet", "AzureDocIQ.dll"]
```

**Docker Compose for Local Development:**
```yaml
# docker-compose.yml
version: '3.8'

services:
  webapp:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=AzureDocIQ;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
      - Azure__BlobStorage__ConnectionString=UseDevelopmentStorage=true
    depends_on:
      - db
    networks:
      - azuredociq-network

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    networks:
      - azuredociq-network

  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    ports:
      - "10000:10000"
      - "10001:10001"
      - "10002:10002"
    command: "azurite --blobHost 0.0.0.0 --queueHost 0.0.0.0 --tableHost 0.0.0.0"
    networks:
      - azuredociq-network

volumes:
  sqlserver_data:

networks:
  azuredociq-network:
    driver: bridge
```

**Azure Container Registry Integration:**
```bash
# Build and push to Azure Container Registry
az acr build --registry azuredociqacr --image azuredociq:latest .

# Deploy to Azure Container Instances
az container create \
  --resource-group azure-doc-iq-rg \
  --name azuredociq-container \
  --image azuredociqacr.azurecr.io/azuredociq:latest \
  --dns-name-label azuredociq-app \
  --ports 8080 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="your-connection-string"
```

**Benefits of Containerization:**
- **Consistency**: Same environment across development, testing, and production
- **Portability**: Can run on any platform that supports Docker
- **Isolation**: Application dependencies are isolated
- **Scalability**: Easy horizontal scaling with container orchestration
- **Version Control**: Container images are versioned and tracked

## ☸️ Kubernetes Questions

### **Q9: How would you deploy AzureDocIQ to Azure Kubernetes Service (AKS)?**

**Answer:**
AzureDocIQ can be deployed to AKS for enterprise-grade container orchestration:

**Kubernetes Manifests:**

**Namespace:**
```yaml
# namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: azuredociq
  labels:
    name: azuredociq
```

**ConfigMap:**
```yaml
# configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: azuredociq-config
  namespace: azuredociq
data:
  ASPNETCORE_ENVIRONMENT: "Production"
  Logging__LogLevel__Default: "Information"
  Logging__LogLevel__Microsoft: "Warning"
```

**Secret:**
```yaml
# secret.yaml
apiVersion: v1
kind: Secret
metadata:
  name: azuredociq-secrets
  namespace: azuredociq
type: Opaque
data:
  ConnectionStrings__DefaultConnection: <base64-encoded-connection-string>
  Azure__CognitiveServices__FormRecognizer__Key: <base64-encoded-key>
  Azure__CognitiveServices__ComputerVision__Key: <base64-encoded-key>
```

**Deployment:**
```yaml
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: azuredociq-webapp
  namespace: azuredociq
  labels:
    app: azuredociq-webapp
spec:
  replicas: 3
  selector:
    matchLabels:
      app: azuredociq-webapp
  template:
    metadata:
      labels:
        app: azuredociq-webapp
    spec:
      containers:
      - name: azuredociq-webapp
        image: azuredociqacr.azurecr.io/azuredociq:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_URLS
          value: "http://+:8080"
        - name: ASPNETCORE_ENVIRONMENT
          valueFrom:
            configMapKeyRef:
              name: azuredociq-config
              key: ASPNETCORE_ENVIRONMENT
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: azuredociq-secrets
              key: ConnectionStrings__DefaultConnection
        - name: Azure__CognitiveServices__FormRecognizer__Key
          valueFrom:
            secretKeyRef:
              name: azuredociq-secrets
              key: Azure__CognitiveServices__FormRecognizer__Key
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
      imagePullSecrets:
      - name: acr-secret
```

**Service:**
```yaml
# service.yaml
apiVersion: v1
kind: Service
metadata:
  name: azuredociq-service
  namespace: azuredociq
spec:
  selector:
    app: azuredociq-webapp
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: ClusterIP
```

**Ingress:**
```yaml
# ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: azuredociq-ingress
  namespace: azuredociq
  annotations:
    kubernetes.io/ingress.class: "nginx"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
spec:
  tls:
  - hosts:
    - azuredociq.yourdomain.com
    secretName: azuredociq-tls
  rules:
  - host: azuredociq.yourdomain.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: azuredociq-service
            port:
              number: 80
```

**Horizontal Pod Autoscaler:**
```yaml
# hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: azuredociq-hpa
  namespace: azuredociq
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: azuredociq-webapp
  minReplicas: 3
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

**Deployment Script:**
```bash
#!/bin/bash
# deploy-to-aks.sh

# Set variables
RESOURCE_GROUP="azure-doc-iq-rg"
AKS_CLUSTER="azure-doc-iq-aks"
NAMESPACE="azuredociq"

# Get AKS credentials
az aks get-credentials --resource-group $RESOURCE_GROUP --name $AKS_CLUSTER

# Create namespace
kubectl apply -f namespace.yaml

# Apply Kubernetes manifests
kubectl apply -f configmap.yaml
kubectl apply -f secret.yaml
kubectl apply -f deployment.yaml
kubectl apply -f service.yaml
kubectl apply -f ingress.yaml
kubectl apply -f hpa.yaml

# Wait for deployment to be ready
kubectl wait --for=condition=available --timeout=300s deployment/azuredociq-webapp -n $NAMESPACE

# Check deployment status
kubectl get pods -n $NAMESPACE
kubectl get services -n $NAMESPACE
kubectl get ingress -n $NAMESPACE
```

**Benefits of AKS Deployment:**
- **High Availability**: Multi-node cluster with automatic failover
- **Auto-scaling**: Horizontal and vertical pod autoscaling
- **Load Balancing**: Built-in load balancing and ingress controllers
- **Security**: Pod security policies and network policies
- **Monitoring**: Integration with Azure Monitor and Application Insights

### **Q10: How would you implement CI/CD for Kubernetes deployments?**

**Answer:**
CI/CD for Kubernetes can be implemented using Azure DevOps and Helm:

**Helm Chart Structure:**
```
azuredociq/
├── Chart.yaml
├── values.yaml
├── values-dev.yaml
├── values-prod.yaml
├── templates/
│   ├── _helpers.tpl
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secret.yaml
│   ├── deployment.yaml
│   ├── service.yaml
│   ├── ingress.yaml
│   ├── hpa.yaml
│   └── NOTES.txt
```

**Chart.yaml:**
```yaml
apiVersion: v2
name: azuredociq
description: AzureDocIQ Document Intelligence Platform
type: application
version: 1.0.0
appVersion: "1.0.0"
```

**values.yaml:**
```yaml
# Default values for azuredociq
replicaCount: 3

image:
  repository: azuredociqacr.azurecr.io/azuredociq
  tag: "latest"
  pullPolicy: IfNotPresent

imagePullSecrets:
  - name: acr-secret

serviceAccount:
  create: true
  annotations: {}
  name: ""

podAnnotations: {}

podSecurityContext: {}

securityContext: {}

service:
  type: ClusterIP
  port: 80
  targetPort: 8080

ingress:
  enabled: true
  className: "nginx"
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
  hosts:
    - host: azuredociq.yourdomain.com
      paths:
        - path: /
          pathType: Prefix
  tls:
    - secretName: azuredociq-tls
      hosts:
        - azuredociq.yourdomain.com

resources:
  limits:
    cpu: 500m
    memory: 512Mi
  requests:
    cpu: 250m
    memory: 256Mi

autoscaling:
  enabled: true
  minReplicas: 3
  maxReplicas: 10
  targetCPUUtilizationPercentage: 70
  targetMemoryUtilizationPercentage: 80

nodeSelector: {}

tolerations: []

affinity: {}

env:
  ASPNETCORE_ENVIRONMENT: "Production"
  ASPNETCORE_URLS: "http://+:8080"
```

**Azure DevOps Pipeline for Kubernetes:**
```yaml
# azure-pipelines-k8s.yml
trigger:
- main

variables:
  imageRepository: 'azuredociq'
  containerRegistry: 'azuredociqacr.azurecr.io'
  dockerfilePath: '**/Dockerfile'
  tag: '$(Build.BuildId)'
  helmChartPath: '**/azuredociq'

stages:
- stage: Build
  displayName: 'Build and Push Image'
  jobs:
  - job: Build
    pool:
      vmImage: 'ubuntu-latest'
    steps:
    - task: Docker@2
      displayName: 'Build and push image to ACR'
      inputs:
        command: 'buildAndPush'
        repository: '$(imageRepository)'
        dockerfile: '$(dockerfilePath)'
        containerRegistry: 'Azure Container Registry'
        tags: |
          $(tag)
          latest

- stage: DeployToDev
  displayName: 'Deploy to Development'
  dependsOn: Build
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/develop'))
  jobs:
  - deployment: Deploy
    environment: 'development'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: HelmInstaller@0
            displayName: 'Install Helm'
            inputs:
              helmVersion: '3.7.0'
              
          - task: HelmDeploy@0
            displayName: 'Deploy to AKS'
            inputs:
              connectionType: 'Azure Resource Manager'
              azureSubscription: 'Azure Subscription'
              azureResourceGroup: 'azure-doc-iq-rg'
              kubernetesCluster: 'azure-doc-iq-aks'
              namespace: 'azuredociq-dev'
              command: 'upgrade'
              chartType: 'FilePath'
              chartPath: '$(helmChartPath)'
              releaseName: 'azuredociq'
              valueFile: '$(helmChartPath)/values-dev.yaml'
              install: true
              waitForExecution: true

- stage: DeployToProd
  displayName: 'Deploy to Production'
  dependsOn: Build
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
  - deployment: Deploy
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: HelmInstaller@0
            displayName: 'Install Helm'
            inputs:
              helmVersion: '3.7.0'
              
          - task: HelmDeploy@0
            displayName: 'Deploy to AKS'
            inputs:
              connectionType: 'Azure Resource Manager'
              azureSubscription: 'Azure Subscription'
              azureResourceGroup: 'azure-doc-iq-rg'
              kubernetesCluster: 'azure-doc-iq-aks'
              namespace: 'azuredociq-prod'
              command: 'upgrade'
              chartType: 'FilePath'
              chartPath: '$(helmChartPath)'
              releaseName: 'azuredociq'
              valueFile: '$(helmChartPath)/values-prod.yaml'
              install: true
              waitForExecution: true
              
          - task: KubernetesManifest@0
            displayName: 'Verify deployment'
            inputs:
              action: 'deploy'
              kubernetesServiceConnection: 'azure-doc-iq-aks'
              namespace: 'azuredociq-prod'
              manifests: |
                $(Pipeline.Workspace)/manifests/deployment.yaml
              containers: |
                $(containerRegistry)/$(imageRepository):$(tag)
```

**Benefits of Kubernetes CI/CD:**
- **GitOps**: Infrastructure and application changes tracked in Git
- **Rollback**: Easy rollback to previous versions
- **Blue-Green Deployment**: Zero-downtime deployments
- **Canary Deployments**: Gradual rollout with monitoring
- **Environment Consistency**: Same deployment process across environments

---

**Next**: [Technical Questions](technical-questions.md) | [Project-Specific Questions](project-specific-questions.md) 