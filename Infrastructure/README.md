# DocuSense Infrastructure Deployment Guide

This guide provides step-by-step instructions for deploying the DocuSense infrastructure to Azure using Bicep templates and Azure DevOps pipelines.

## Prerequisites

- Azure Subscription with appropriate permissions
- Azure CLI installed and configured
- Azure DevOps organization and project
- .NET 8 SDK installed
- Visual Studio 2022 or VS Code

## Azure Services Required

The following Azure services will be deployed:

1. **Azure App Service** - Hosts the web application and API
2. **Azure Functions** - Processes documents using Azure Cognitive Services
3. **Azure SQL Database** - Stores structured data and user information
4. **Azure Cosmos DB** - Stores audit logs and unstructured data
5. **Azure Blob Storage** - Stores uploaded documents
6. **Azure Cognitive Services** - Provides OCR and form recognition capabilities
7. **Azure Key Vault** - Stores secrets and configuration
8. **Azure Application Insights** - Provides monitoring and telemetry
9. **Azure AD B2C** - Handles user authentication and authorization

## Step 1: Azure AD B2C Setup

1. Create an Azure AD B2C tenant:
   ```bash
   az ad b2c tenant create --name your-tenant-name --resource-group your-rg --location eastus
   ```

2. Register your application in Azure AD B2C:
   - Go to Azure Portal > Azure AD B2C
   - Navigate to "App registrations" > "New registration"
   - Enter application name: "DocuSense"
   - Set redirect URI: `https://your-app-name.azurewebsites.net/signin-oidc`
   - Note the Application (client) ID

3. Create user flows for sign-up/sign-in:
   - Go to "User flows" > "New user flow"
   - Select "Sign up and sign in"
   - Configure attributes and claims
   - Note the user flow name (e.g., "B2C_1_signupsignin")

## Step 2: Local Development Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/your-org/DocuSense.git
   cd DocuSense
   ```

2. Configure local settings:
   - Copy `appsettings.Development.json` and update with your local settings
   - For local development, you can use Azure Storage Emulator or Azurite

3. Set up user secrets:
   ```bash
   dotnet user-secrets set "Azure:BlobStorage:ConnectionString" "UseDevelopmentStorage=true"
   dotnet user-secrets set "Azure:CognitiveServices:FormRecognizer:Endpoint" "your-endpoint"
   dotnet user-secrets set "Azure:CognitiveServices:FormRecognizer:Key" "your-key"
   ```

4. Run the application locally:
   ```bash
   dotnet restore
   dotnet run
   ```

## Step 3: Azure Infrastructure Deployment

### Option A: Using Azure CLI

1. Create a resource group:
   ```bash
   az group create --name azure-doc-iq-rg --location eastus
   ```

2. Deploy the Bicep template:
   ```bash
   az deployment group create \
     --resource-group azure-doc-iq-rg \
     --template-file Infrastructure/main.bicep \
     --parameters \
       resourceGroupName=azure-doc-iq-rg \
       location=eastus \
       environment=dev \
       sqlAdminUsername=your-admin-username \
       sqlAdminPassword=your-secure-password \
       b2cTenantName=your-tenant-name \
       b2cClientId=your-client-id
   ```

### Option B: Using Azure DevOps Pipeline

1. Set up Azure DevOps service connection:
   - Go to Project Settings > Service connections
   - Create new service connection for Azure Resource Manager
   - Grant access to all pipelines

2. Configure pipeline variables:
   - Go to Pipelines > Edit pipeline
   - Add the following variables:
     - `AZURE_SUBSCRIPTION_ID`: Your Azure subscription ID
     - `RESOURCE_GROUP_NAME`: azure-doc-iq-rg
     - `AZURE_LOCATION`: eastus
     - `WEB_APP_NAME`: azure-doc-iq-dev
     - `FUNCTION_APP_NAME`: azure-doc-iq-func-dev
     - `APP_INSIGHTS_NAME`: azure-doc-iq-ai-dev

3. Run the pipeline:
   - Commit and push your changes
   - The pipeline will automatically trigger and deploy the infrastructure

## Step 4: Application Configuration

1. Update application settings:
   - Go to Azure Portal > App Service > Configuration
   - Add the following application settings:
     ```
     ConnectionStrings__DefaultConnection: [SQL Connection String]
     Azure__BlobStorage__ConnectionString: [Blob Storage Connection String]
     Azure__CognitiveServices__FormRecognizer__Endpoint: [Cognitive Services Endpoint]
     Azure__CognitiveServices__FormRecognizer__Key: [Cognitive Services Key]
     Azure__KeyVault__Url: [Key Vault URL]
     AzureAdB2C__Instance: https://your-tenant.b2clogin.com/
     AzureAdB2C__Domain: your-tenant.onmicrosoft.com
     AzureAdB2C__ClientId: [B2C Client ID]
     AzureAdB2C__SignUpSignInPolicyId: B2C_1_signupsignin
     ```

2. Configure Key Vault access:
   - Go to Key Vault > Access policies
   - Add access policy for the App Service managed identity
   - Grant permissions: Get, List for secrets

3. Configure CORS for Blob Storage:
   - Go to Storage Account > CORS
   - Add rule for your web app domain

## Step 5: Database Setup

1. Run Entity Framework migrations:
   ```bash
   dotnet ef database update --connection "your-connection-string"
   ```

2. Seed initial data (optional):
   ```bash
   dotnet run --project Data/SeedData.csproj
   ```

## Step 6: Azure Functions Deployment

1. Deploy the Azure Functions:
   ```bash
   func azure functionapp publish azure-doc-iq-func-dev
   ```

2. Configure function app settings:
   - Add the same connection strings and settings as the web app
   - Configure the blob trigger for document processing

## Step 7: Monitoring Setup

1. Configure Application Insights:
   - Go to Application Insights > Live Metrics
   - Verify telemetry is being received

2. Set up alerts:
   - Create alert rules for:
     - High error rates
     - Slow response times
     - Failed document processing
     - Unusual user activity

3. Configure logging:
   - Set up log analytics workspace
   - Configure diagnostic settings for all resources

## Step 8: Security Configuration

1. Enable managed identities:
   - Go to App Service > Identity
   - Enable system-assigned managed identity
   - Grant necessary permissions to Key Vault and Storage

2. Configure network security:
   - Set up VNet integration if required
   - Configure firewall rules for SQL Database
   - Enable private endpoints for sensitive resources

3. Set up SSL/TLS:
   - Configure custom domain with SSL certificate
   - Enable HTTPS-only access

## Step 9: Testing

1. Test the application:
   - Navigate to your deployed web app
   - Test user registration and login
   - Upload and process a test document
   - Verify extracted data is saved correctly

2. Test monitoring:
   - Check Application Insights for telemetry
   - Verify audit logs are being created
   - Test alert notifications

## Troubleshooting

### Common Issues

1. **Authentication Errors**:
   - Verify B2C tenant configuration
   - Check redirect URIs in app registration
   - Ensure user flows are properly configured

2. **Database Connection Issues**:
   - Verify connection string format
   - Check firewall rules for SQL Database
   - Ensure managed identity has proper permissions

3. **Blob Storage Access**:
   - Verify CORS configuration
   - Check managed identity permissions
   - Ensure container exists and is accessible

4. **Cognitive Services Errors**:
   - Verify endpoint and key configuration
   - Check service quota and limits
   - Ensure proper region configuration

### Logs and Diagnostics

- **Application Logs**: Check App Service logs in Azure Portal
- **Function Logs**: Use Azure Functions monitoring
- **Database Logs**: Check SQL Database diagnostic settings
- **Network Issues**: Use Network Watcher for connectivity problems

## Cost Optimization

1. **Development Environment**:
   - Use Basic tier for App Service
   - Use Basic tier for SQL Database
   - Use Standard tier for Cognitive Services

2. **Production Environment**:
   - Use Standard or Premium tiers based on load
   - Enable auto-scaling for App Service
   - Use reserved instances for cost savings

3. **Monitoring**:
   - Set up budget alerts
   - Monitor resource usage regularly
   - Use Azure Advisor for optimization recommendations

## Next Steps

1. Set up staging environment
2. Configure backup and disaster recovery
3. Implement CI/CD for automated deployments
4. Set up performance monitoring and alerting
5. Plan for scaling and high availability

## Support

For issues and questions:
- Check Azure documentation
- Review application logs
- Contact Azure support if needed
- Refer to the project README for additional information 