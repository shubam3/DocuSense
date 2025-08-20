# DocuSense - Technology Stack Deep Dive

## 🛠️ Complete Technology Stack Overview

DocuSense leverages a modern, cloud-native technology stack designed for scalability, security, and performance. This document provides a comprehensive breakdown of all technologies used in the project.

## 🏗️ Backend Technologies

### Core Framework
#### **ASP.NET Core 8.0**
- **Version**: 8.0 (Latest LTS)
- **Purpose**: Main web application framework
- **Key Features**:
  - Cross-platform compatibility (Windows, Linux, macOS)
  - High-performance HTTP pipeline
  - Built-in dependency injection
  - Middleware pipeline for request processing
  - Model binding and validation
  - Built-in security features

**Why ASP.NET Core 8.0?**
- **Performance**: 2-3x faster than previous versions
- **Memory Efficiency**: Reduced memory footprint
- **Modern C# Features**: Latest language features and patterns
- **Cloud-Native**: Designed for containerization and microservices
- **Long-term Support**: Extended support until 2026

#### **C# 12.0**
- **Version**: Latest C# language features
- **Key Features**:
  - Records and record structs
  - Pattern matching enhancements
  - Nullable reference types
  - Async/await patterns
  - LINQ and functional programming
  - Source generators

### Database Technologies

#### **Entity Framework Core 8.0**
- **Purpose**: Object-Relational Mapping (ORM)
- **Key Features**:
  - Code-first and database-first approaches
  - LINQ query support
  - Change tracking and lazy loading
  - Migration management
  - Connection pooling
  - Query optimization

**Configuration**:
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging()
           .EnableDetailedErrors());
```

#### **Azure SQL Database**
- **Service Tier**: Standard (S1) for production
- **Purpose**: Primary relational database
- **Key Features**:
  - Built-in high availability
  - Automatic backups and point-in-time restore
  - Advanced threat protection
  - Elastic pools for cost optimization
  - Geo-replication for disaster recovery

**Performance Optimizations**:
- Clustered and non-clustered indexes
- Query store for performance monitoring
- Automatic tuning recommendations
- Connection pooling (max 100 connections)

#### **Azure Cosmos DB**
- **API**: SQL (Core) API
- **Purpose**: NoSQL database for audit logs and analytics
- **Key Features**:
  - Global distribution
  - Multi-model support
  - Automatic scaling
  - Sub-millisecond response times
  - Built-in security

**Data Models**:
```json
{
  "id": "audit-log-123",
  "timestamp": "2024-01-15T10:30:00Z",
  "userId": "user-456",
  "action": "DocumentUpload",
  "entityType": "Document",
  "entityId": "doc-789",
  "details": {
    "fileName": "invoice.pdf",
    "fileSize": 1024000,
    "ipAddress": "192.168.1.100"
  }
}
```

### Cloud Services

#### **Azure App Service**
- **Plan**: Standard (S1) for production
- **Runtime**: .NET 8.0
- **Purpose**: Web application hosting
- **Key Features**:
  - Auto-scaling (1-10 instances)
  - Built-in SSL/TLS
  - Custom domains
  - Deployment slots
  - Application insights integration

**Configuration**:
```json
{
  "WEBSITE_RUN_FROM_PACKAGE": "1",
  "WEBSITE_NODE_DEFAULT_VERSION": "18.17.0",
  "DOTNET_ENVIRONMENT": "Production",
  "ASPNETCORE_ENVIRONMENT": "Production"
}
```

#### **Azure Functions v4**
- **Runtime**: .NET 8.0 (Isolated)
- **Purpose**: Serverless document processing
- **Triggers**:
  - Blob Storage triggers
  - HTTP triggers
  - Timer triggers
  - Event Grid triggers

**Function Types**:
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
```

#### **Azure Blob Storage**
- **Account Type**: General Purpose v2
- **Redundancy**: Geo-redundant storage (GRS)
- **Purpose**: Document file storage
- **Containers**:
  - `documents`: Original uploaded files
  - `processed`: AI processing results
  - `backups`: System backups
  - `temp`: Temporary processing files

**Security Features**:
- Shared Access Signatures (SAS)
- Azure AD authentication
- Encryption at rest and in transit
- Soft delete for data protection

#### **Azure Cognitive Services**
- **Form Recognizer**: Document analysis and form field extraction
- **Computer Vision**: OCR and image analysis
- **Language Understanding**: Text analysis and classification

**Form Recognizer Configuration**:
```csharp
var client = new DocumentAnalysisClient(
    new Uri(endpoint), 
    new AzureKeyCredential(key));

var operation = await client.AnalyzeDocumentAsync(
    WaitUntil.Completed, 
    "prebuilt-document", 
    documentStream);
```

#### **Azure Active Directory B2C**
- **Purpose**: User authentication and authorization
- **Features**:
  - Social identity providers (Google, Facebook, Microsoft)
  - Custom user flows
  - Multi-factor authentication
  - Password reset and account recovery
  - User profile management

**Configuration**:
```json
{
  "AzureAdB2C": {
    "Instance": "https://your-tenant.b2clogin.com/",
    "Domain": "your-tenant.onmicrosoft.com",
    "ClientId": "your-client-id",
    "SignUpSignInPolicyId": "B2C_1_signupsignin"
  }
}
```

#### **Azure Key Vault**
- **Purpose**: Secure storage of secrets and certificates
- **Features**:
  - Centralized secret management
  - Access control with RBAC
  - Audit logging
  - Soft delete and purge protection
  - Hardware security modules (HSM)

**Secret Management**:
```csharp
var client = new SecretClient(
    new Uri(keyVaultUrl), 
    new DefaultAzureCredential());

var secret = await client.GetSecretAsync("CognitiveServicesKey");
```

#### **Azure Application Insights**
- **Purpose**: Application monitoring and telemetry
- **Features**:
  - Performance monitoring
  - Error tracking and diagnostics
  - User behavior analytics
  - Custom metrics and events
  - Live metrics streaming

**Telemetry Configuration**:
```csharp
services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
});
```

## 🎨 Frontend Technologies

### **ASP.NET Core MVC Views**
- **Template Engine**: Razor
- **Purpose**: Server-side rendering
- **Features**:
  - Strongly-typed views
  - Partial views and layouts
  - View components
  - Tag helpers
  - Model binding and validation

### **Bootstrap 5.3**
- **Version**: Latest stable
- **Purpose**: Responsive UI framework
- **Features**:
  - Mobile-first responsive design
  - CSS Grid and Flexbox
  - Customizable components
  - Accessibility features
  - Dark mode support

### **Font Awesome 6.0**
- **Purpose**: Icon library
- **Features**:
  - 1,600+ free icons
  - Scalable vector graphics
  - Consistent styling
  - Accessibility support

### **JavaScript Libraries**
- **jQuery**: DOM manipulation and AJAX
- **Chart.js**: Data visualization
- **DataTables**: Interactive tables
- **SweetAlert2**: Enhanced alerts and modals

## 🔧 Development Tools

### **Visual Studio 2022**
- **Version**: 17.8 or later
- **Features**:
  - IntelliSense and code completion
  - Integrated debugging
  - Git integration
  - Azure development tools
  - Performance profiling

### **Azure CLI**
- **Version**: 2.50.0 or later
- **Purpose**: Azure resource management
- **Key Commands**:
  ```bash
  az login
  az group create --name myResourceGroup --location eastus
  az webapp create --name myApp --resource-group myResourceGroup
  ```

### **Azure Functions Core Tools**
- **Version**: 4.x
- **Purpose**: Local Function development
- **Features**:
  - Local debugging
  - Function creation templates
  - Deployment to Azure

### **Entity Framework Tools**
- **Package**: Microsoft.EntityFrameworkCore.Tools
- **Commands**:
  ```bash
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  dotnet ef dbcontext scaffold
  ```

## 📦 NuGet Packages

### **Core Packages**
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
```

### **Azure Packages**
```xml
<PackageReference Include="Azure.Storage.Blobs" Version="12.19.1" />
<PackageReference Include="Azure.AI.FormRecognizer" Version="4.1.0" />
<PackageReference Include="Azure.AI.ComputerVision" Version="4.0.0" />
<PackageReference Include="Azure.Security.KeyVault.Secrets" Version="4.5.0" />
<PackageReference Include="Azure.Identity" Version="1.10.4" />
<PackageReference Include="Microsoft.Azure.Cosmos" Version="3.38.1" />
<PackageReference Include="Microsoft.Azure.Functions.Worker" Version="1.19.0" />
```

### **Monitoring & Logging**
```xml
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.22.0" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.ApplicationInsights" Version="4.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
```

### **Utilities**
```xml
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="Microsoft.Extensions.Configuration.AzureKeyVault" Version="8.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

## 🔒 Security Technologies

### **Authentication & Authorization**
- **ASP.NET Core Identity**: User management
- **JWT Bearer Tokens**: API authentication
- **Role-based Access Control (RBAC)**: Authorization
- **Claims-based Authorization**: Fine-grained permissions

### **Data Protection**
- **AES-256 Encryption**: Data at rest
- **TLS 1.3**: Data in transit
- **Azure Key Vault**: Key management
- **Managed Identities**: Service-to-service authentication

### **Input Validation**
- **FluentValidation**: Model validation
- **ASP.NET Core Model Validation**: Built-in validation
- **Anti-forgery Tokens**: CSRF protection
- **SQL Injection Prevention**: Parameterized queries

## 📊 Performance Technologies

### **Caching**
- **In-Memory Caching**: Application-level caching
- **Distributed Caching**: Redis (optional)
- **Response Caching**: HTTP response caching
- **Output Caching**: View output caching

### **Optimization**
- **Connection Pooling**: Database connections
- **Async/Await**: Non-blocking operations
- **Compression**: Gzip/Brotli compression
- **CDN**: Static content delivery

## 🧪 Testing Technologies

### **Unit Testing**
- **xUnit**: Testing framework
- **Moq**: Mocking framework
- **FluentAssertions**: Assertion library
- **Microsoft.NET.Test.Sdk**: Test runner

### **Integration Testing**
- **Microsoft.AspNetCore.Mvc.Testing**: Web application testing
- **Microsoft.EntityFrameworkCore.InMemory**: In-memory database
- **TestServer**: HTTP testing

### **Performance Testing**
- **NBomber**: Load testing
- **Application Insights**: Performance monitoring
- **Azure Load Testing**: Cloud-based load testing

## 🚀 Deployment Technologies

### **Infrastructure as Code**
- **Azure Bicep**: Infrastructure templates
- **Azure Resource Manager**: Resource deployment
- **Azure CLI**: Command-line deployment

### **CI/CD Pipeline**
- **Azure DevOps**: Build and release pipelines
- **YAML Pipelines**: Pipeline as code
- **Azure Artifacts**: Package management
- **Azure Repos**: Source control

### **Containerization**
- **Docker**: Containerization (optional)
- **Azure Container Registry**: Container registry
- **Azure Container Instances**: Container hosting

## 📈 Monitoring & Observability

### **Application Monitoring**
- **Application Insights**: APM and telemetry
- **Azure Monitor**: Infrastructure monitoring
- **Log Analytics**: Centralized logging
- **Azure Dashboards**: Custom monitoring dashboards

### **Alerting**
- **Azure Monitor Alerts**: Metric and log alerts
- **Action Groups**: Notification channels
- **Webhooks**: Custom integrations
- **Email/SMS**: Direct notifications

## 🔄 Version Compatibility Matrix

| Component | Version | Compatibility | Notes |
|-----------|---------|---------------|-------|
| .NET | 8.0 | LTS | Supported until 2026 |
| ASP.NET Core | 8.0 | LTS | Latest stable |
| Entity Framework | 8.0 | LTS | Matches .NET version |
| Azure Functions | 4.x | Latest | .NET 8 isolated |
| Azure SDK | Latest | Rolling | Monthly updates |
| Bootstrap | 5.3 | Latest | Stable release |
| jQuery | 3.7 | Latest | Legacy support |

## 🎯 Technology Selection Rationale

### **Why .NET 8?**
- **Performance**: Significant performance improvements
- **Cloud-Native**: Built for modern cloud applications
- **Ecosystem**: Rich ecosystem of libraries and tools
- **Enterprise**: Proven in enterprise environments
- **Support**: Long-term support and regular updates

### **Why Azure?**
- **Integration**: Seamless integration between services
- **Scalability**: Auto-scaling and serverless capabilities
- **Security**: Enterprise-grade security features
- **Compliance**: Built-in compliance certifications
- **Cost**: Pay-as-you-go pricing model

### **Why Serverless?**
- **Cost**: Pay only for actual usage
- **Scalability**: Automatic scaling to zero
- **Maintenance**: No server management required
- **Performance**: Fast cold start times
- **Reliability**: Built-in fault tolerance

---

**Next**: [Azure Services Deep Dive](azure-services.md) | [Security Implementation](security-implementation.md) 