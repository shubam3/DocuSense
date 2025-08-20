# DocuSense - Complete Project File Structure Guide

## 📁 Project Overview

This document provides a comprehensive explanation of every file and folder in the DocuSense project, their purposes, and how they work together to create a complete AI-powered document intelligence platform.

## 🏗️ Root Level Files

### **`DocuSense.csproj`**
**Purpose**: Main project file for the ASP.NET Core application
**Content**: 
- Defines the project as an ASP.NET Core 8.0 web application
- Contains all NuGet package references
- Specifies target framework and build configurations
- Defines project structure and dependencies

**Key Packages**:
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT authentication
- `Azure.Storage.Blobs` - Azure Blob Storage integration
- `Azure.AI.FormRecognizer` - AI document processing
- `Microsoft.EntityFrameworkCore.SqlServer` - Database ORM
- `Serilog.AspNetCore` - Structured logging

### **`Program.cs`**
**Purpose**: Application entry point and service configuration
**Key Responsibilities**:
- Configures dependency injection container
- Sets up authentication and authorization
- Configures database context and Entity Framework
- Registers Azure services (Blob Storage, Cognitive Services, Key Vault)
- Sets up logging with Serilog
- Configures CORS and session management
- Ensures database creation on startup

**Critical Services Registered**:
```csharp
services.AddScoped<IDocumentService, DocumentService>();
services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();
services.AddScoped<IAzureCognitiveServices, AzureCognitiveServices>();
services.AddScoped<IAuditService, AuditService>();
```

### **`appsettings.json`**
**Purpose**: Main configuration file for production settings
**Contains**:
- Azure service connection strings
- Cognitive Services endpoints and keys
- Azure AD B2C configuration
- Database connection strings
- Logging configuration
- Security settings
- Document processing limits

### **`appsettings.Development.json`**
**Purpose**: Development-specific configuration overrides
**Contains**:
- Local development settings
- Azure Storage Emulator connection
- Local SQL Server connection
- Development-specific logging levels
- Debug configurations

### **`README.md`**
**Purpose**: Project overview and getting started guide
**Contains**:
- Project description and features
- Technology stack overview
- Quick start instructions
- Architecture overview
- Deployment guide links

### **`instructions.txt`**
**Purpose**: Complete setup and deployment instructions
**Contains**:
- Prerequisites installation
- Local development setup
- Azure infrastructure deployment
- Configuration management
- Testing procedures
- Troubleshooting guide
- Cost optimization tips

## 📂 Core Application Folders

### **`Controllers/`**
**Purpose**: MVC controllers handling HTTP requests and responses

#### **`HomeController.cs`**
- **Purpose**: Main landing page and dashboard controller
- **Actions**:
  - `Index()` - Landing page with project overview
  - `Dashboard()` - User dashboard with document statistics
  - `About()`, `Contact()`, `Privacy()` - Static pages

#### **`DocumentsController.cs`**
- **Purpose**: Document management and processing controller
- **Actions**:
  - `Index()` - List user's documents with search/filter
  - `Upload()` - Handle document uploads
  - `Details()` - Show document details and extracted fields
  - `Delete()` - Soft delete documents
  - `RetryProcessing()` - Retry failed document processing
  - `Download()` - Download original documents

### **`Models/`**
**Purpose**: Entity models and data structures

#### **`ApplicationUser.cs`**
- **Purpose**: Extended user model for ASP.NET Core Identity
- **Properties**:
  - `FirstName`, `LastName` - User personal information
  - `Company`, `Department` - Organization details
  - `Role` - User role (Admin, Manager, User)
  - `CreatedAt`, `LastLoginAt` - Timestamps
  - `IsActive`, `LoginAttempts` - Account status
- **Navigation Properties**: `Documents`, `AuditLogs`

#### **`Document.cs`**
- **Purpose**: Core document entity
- **Properties**:
  - `FileName`, `FileType`, `FileSize` - File metadata
  - `BlobUrl` - Azure Blob Storage URL
  - `Status` - Processing status enum
  - `UploadedAt`, `ProcessedAt` - Timestamps
  - `UserId`, `ProjectName` - Ownership and organization
- **Navigation Properties**: `ApplicationUser`, `DocumentFields`, `AuditLogs`

#### **`DocumentField.cs`**
- **Purpose**: Extracted data from documents
- **Properties**:
  - `FieldName`, `FieldValue` - Extracted key-value pairs
  - `FieldType`, `Confidence` - AI processing metadata
  - `BoundingBox`, `PageNumber` - Spatial information
  - `IsVerified`, `VerifiedBy` - Quality control
- **Navigation Properties**: `Document`

#### **`AuditLog.cs`**
- **Purpose**: System audit trail for compliance
- **Properties**:
  - `Timestamp`, `Action`, `EntityType` - Event details
  - `UserId`, `UserEmail`, `UserRole` - User context
  - `IpAddress`, `UserAgent` - Request context
  - `Status`, `Details` - Operation results
  - `Severity`, `IsAnomaly` - Security monitoring
- **Navigation Properties**: `ApplicationUser`, `Document`

#### **`ViewModels/`**
**Purpose**: Data transfer objects for views

##### **`DocumentViewModel.cs`**
- **Purpose**: View models for document display and upload
- **Classes**:
  - `DocumentViewModel` - Document list display
  - `DocumentFieldViewModel` - Extracted field display
  - `DocumentUploadViewModel` - Upload form data
  - `DocumentListViewModel` - Paginated document list

### **`Data/`**
**Purpose**: Database context and data access layer

#### **`ApplicationDbContext.cs`**
- **Purpose**: Entity Framework Core database context
- **Features**:
  - Extends `IdentityDbContext<ApplicationUser>`
  - Configures entity relationships and constraints
  - Sets up database indexes for performance
  - Configures audit logging
  - Handles soft delete functionality

**Key Configurations**:
```csharp
modelBuilder.Entity<Document>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.UserId);
    entity.HasIndex(e => e.Status);
    entity.HasIndex(e => e.UploadedAt);
});
```

### **`DTOs/`**
**Purpose**: Data Transfer Objects for API communication

#### **`DocumentDto.cs`**
- **Purpose**: Clean data transfer between layers
- **Classes**:
  - `DocumentDto` - Document data transfer
  - `DocumentFieldDto` - Field data transfer
  - `CreateDocumentDto` - Document creation request
  - `UpdateDocumentDto` - Document update request
  - `DocumentProcessingResultDto` - AI processing results
  - `DocumentSearchDto` - Search parameters

### **`Services/`**
**Purpose**: Business logic and external service integration

#### **`Interfaces/`**
**Purpose**: Service contracts and abstractions

##### **`IDocumentService.cs`**
- **Purpose**: Document business logic interface
- **Methods**:
  - `GetDocumentsAsync()` - Retrieve user documents
  - `CreateDocumentAsync()` - Upload and process documents
  - `ProcessDocumentAsync()` - AI document processing
  - `UpdateDocumentAsync()` - Update document metadata
  - `DeleteDocumentAsync()` - Soft delete documents

##### **`IAzureBlobStorageService.cs`**
- **Purpose**: Azure Blob Storage operations interface
- **Methods**:
  - `UploadFileAsync()` - Upload documents to blob storage
  - `DownloadFileAsync()` - Download documents
  - `DeleteFileAsync()` - Remove documents
  - `GetFileUrlAsync()` - Generate access URLs
  - `ListFilesAsync()` - List user documents

##### **`IAzureCognitiveServices.cs`**
- **Purpose**: AI processing interface
- **Methods**:
  - `ProcessDocumentWithFormRecognizerAsync()` - PDF processing
  - `ProcessDocumentWithComputerVisionAsync()` - Image processing
  - `ExtractTextAsync()` - Text extraction
  - `GetDocumentConfidenceAsync()` - Confidence scoring
  - `IsDocumentValidAsync()` - Document validation

##### **`IAuditService.cs`**
- **Purpose**: Audit logging interface
- **Methods**:
  - `LogActionAsync()` - Log user actions
  - `GetAuditLogsAsync()` - Retrieve audit logs
  - `GetAnomaliesAsync()` - Security anomaly detection
  - `CheckAnomalousActivityAsync()` - Real-time monitoring

##### **`IUserService.cs`**
- **Purpose**: User management interface
- **Methods**:
  - `GetUsersAsync()` - Retrieve users
  - `CreateUserAsync()` - Create new users
  - `UpdateUserAsync()` - Update user information
  - `ManageUserRolesAsync()` - Role management

##### **`ICosmosDbService.cs`**
- **Purpose**: Cosmos DB operations interface
- **Methods**:
  - `InitializeAsync()` - Database initialization
  - `CreateItemAsync()` - Create audit logs
  - `GetItemsAsync()` - Retrieve audit data
  - `QueryItemsAsync()` - Complex queries

##### **`IKeyVaultService.cs`**
- **Purpose**: Azure Key Vault operations interface
- **Methods**:
  - `GetSecretAsync()` - Retrieve secrets
  - `SetSecretAsync()` - Store secrets
  - `DeleteSecretAsync()` - Remove secrets
  - `ListSecretsAsync()` - List available secrets

##### **`IReportService.cs`**
- **Purpose**: Reporting and analytics interface
- **Methods**:
  - `GenerateDocumentReportAsync()` - Document analytics
  - `GenerateAuditReportAsync()` - Audit analytics
  - `GetDashboardStatisticsAsync()` - Dashboard metrics
  - `GetRecentActivityAsync()` - Recent activity feed

#### **`Implementations/`**
**Purpose**: Concrete service implementations

##### **`DocumentService.cs`**
- **Purpose**: Core document processing business logic
- **Key Features**:
  - Orchestrates document upload and processing workflow
  - Integrates with Azure Blob Storage and Cognitive Services
  - Manages document lifecycle and status updates
  - Implements retry logic and error handling
  - Provides access control and security

##### **`AzureBlobStorageService.cs`**
- **Purpose**: Azure Blob Storage integration
- **Key Features**:
  - Secure file upload with SAS tokens
  - Automatic container management
  - File metadata tracking
  - CDN integration for performance
  - Lifecycle management for cost optimization

##### **`AzureCognitiveServices.cs`**
- **Purpose**: AI document processing implementation
- **Key Features**:
  - Form Recognizer for PDF processing
  - Computer Vision for image processing
  - Confidence scoring and validation
  - Multi-language support
  - Custom model training capabilities

##### **`AuditService.cs`**
- **Purpose**: Comprehensive audit logging
- **Key Features**:
  - Real-time action logging
  - Anomaly detection algorithms
  - Compliance reporting
  - Security monitoring
  - Performance tracking

##### **`UserService.cs`**
- **Purpose**: User management implementation
- **Key Features**:
  - User CRUD operations
  - Role-based access control
  - Account security management
  - User activity tracking
  - Profile management

##### **`CosmosDbService.cs`**
- **Purpose**: Cosmos DB integration
- **Key Features**:
  - High-throughput audit logging
  - Global distribution support
  - Automatic scaling
  - Schema flexibility
  - Performance optimization

##### **`KeyVaultService.cs`**
- **Purpose**: Secure secret management
- **Key Features**:
  - Centralized secret storage
  - Access control with RBAC
  - Automatic key rotation
  - Audit logging
  - Compliance support

##### **`ReportService.cs`**
- **Purpose**: Analytics and reporting
- **Key Features**:
  - Real-time dashboard metrics
  - Custom report generation
  - Performance analytics
  - User activity insights
  - Business intelligence

### **`Views/`**
**Purpose**: MVC Razor views for user interface

#### **`Shared/`**
**Purpose**: Shared layout and partial views

##### **`_Layout.cshtml`**
- **Purpose**: Main application layout
- **Features**:
  - Bootstrap 5.3 responsive design
  - Navigation menu with role-based access
  - User authentication status
  - Flash message display
  - CSS and JavaScript includes

##### **`_LoginPartial.cshtml`**
- **Purpose**: Authentication UI component
- **Features**:
  - Dynamic login/logout links
  - User profile information
  - Role-based menu items
  - Responsive design

#### **`Home/`**
**Purpose**: Home page and dashboard views

##### **`Index.cshtml`**
- **Purpose**: Landing page
- **Features**:
  - Hero section with call-to-action
  - Feature highlights
  - Technology stack showcase
  - User testimonials
  - Getting started guide

#### **`Documents/`**
**Purpose**: Document management views

##### **`Index.cshtml`**
- **Purpose**: Document list page
- **Features**:
  - Search and filter functionality
  - Pagination
  - Status indicators
  - Action buttons (View, Download, Delete)
  - Responsive table design

### **`wwwroot/`**
**Purpose**: Static web assets

#### **`css/`**
**Purpose**: Stylesheets

##### **`site.css`**
- **Purpose**: Custom application styles
- **Features**:
  - Bootstrap overrides
  - Custom component styles
  - Dark mode support
  - Responsive design
  - Animation effects

#### **`js/`**
**Purpose**: JavaScript files
- **Features**:
  - Document upload handling
  - AJAX requests
  - Form validation
  - Interactive components
  - Real-time updates

#### **`lib/`**
**Purpose**: Third-party libraries
- **Contents**:
  - Bootstrap CSS and JS
  - jQuery
  - Font Awesome
  - Chart.js
  - DataTables

## 🏗️ Infrastructure & Deployment

### **`Infrastructure/`**
**Purpose**: Infrastructure as Code and deployment templates

#### **`main.bicep`**
- **Purpose**: Azure infrastructure template
- **Resources**:
  - Resource Group
  - Storage Account with blob containers
  - SQL Server and Database
  - App Service Plan and Web App
  - Function App
  - Cognitive Services
  - Key Vault
  - Application Insights
  - Cosmos DB
- **Features**:
  - Environment-specific parameters
  - Security configurations
  - Monitoring setup
  - Cost optimization

#### **`README.md`**
- **Purpose**: Infrastructure deployment guide
- **Contains**:
  - Prerequisites
  - Step-by-step deployment instructions
  - Configuration management
  - Troubleshooting guide
  - Cost optimization tips

### **`Pipelines/`**
**Purpose**: CI/CD pipeline definitions

#### **`azure-pipelines.yml`**
- **Purpose**: Azure DevOps CI/CD pipeline
- **Stages**:
  - Build: Compile and test application
  - DeployInfrastructure: Deploy Azure resources
  - DeployApplication: Deploy application code
  - PostDeployment: Run smoke tests
- **Features**:
  - Multi-stage deployment
  - Environment protection
  - Automated testing
  - Infrastructure as Code
  - Monitoring integration

## 📚 Documentation

### **`docs/`**
**Purpose**: Comprehensive project documentation

#### **`README.md`**
- **Purpose**: Documentation index and navigation
- **Contains**:
  - Documentation structure overview
  - Quick navigation links
  - Usage guidelines
  - Project status

#### **`system-design/`**
**Purpose**: System architecture documentation

##### **`high-level-design.md`**
- **Purpose**: System architecture overview
- **Contains**:
  - Architecture diagrams
  - Component interactions
  - Scalability design
  - Security architecture
  - Deployment strategy

##### **`low-level-design.md`**
- **Purpose**: Detailed component design
- **Contains**:
  - Service implementations
  - Database schemas
  - API specifications
  - Integration patterns
  - Performance considerations

#### **`technical-specs/`**
**Purpose**: Technical specifications

##### **`technology-stack.md`**
- **Purpose**: Complete technology stack breakdown
- **Contains**:
  - Backend technologies
  - Frontend technologies
  - Azure services
  - Development tools
  - Version compatibility matrix

#### **`business/`**
**Purpose**: Business requirements and analysis

##### **`business-requirements.md`**
- **Purpose**: Business requirements document
- **Contains**:
  - Business objectives
  - Functional requirements
  - Non-functional requirements
  - Market analysis
  - Success metrics

#### **`interview-prep/`**
**Purpose**: Interview preparation materials

##### **`technical-questions.md`**
- **Purpose**: Technical interview questions
- **Contains**:
  - 21 detailed technical Q&A
  - Code examples
  - Architecture explanations
  - Best practices

##### **`behavioral-questions.md`**
- **Purpose**: Behavioral interview questions
- **Contains**:
  - 16 behavioral Q&A with STAR method
  - Leadership examples
  - Problem-solving scenarios
  - Team collaboration stories

##### **`project-specific-questions.md`**
- **Purpose**: Project-specific technical questions
- **Contains**:
  - 10 deep-dive implementation questions
  - Design decision explanations
  - Code walkthroughs
  - Challenge solutions

##### **`azure-devops-kubernetes-docker-questions.md`**
- **Purpose**: Cloud and DevOps questions
- **Contains**:
  - 10 Azure services questions
  - DevOps pipeline explanations
  - Kubernetes deployment guides
  - Docker containerization details

## 🔄 How Files Work Together

### **Application Flow**
1. **User Request** → `Controllers/` → `Services/` → `Data/`
2. **Document Upload** → `DocumentsController` → `DocumentService` → `AzureBlobStorageService`
3. **AI Processing** → `AzureFunctions` → `AzureCognitiveServices` → `DocumentService`
4. **Data Storage** → `ApplicationDbContext` → `Azure SQL Database`
5. **Audit Logging** → `AuditService` → `CosmosDbService` → `Azure Cosmos DB`

### **Configuration Flow**
1. **Startup** → `Program.cs` → `appsettings.json` → `Azure Key Vault`
2. **Authentication** → `Azure AD B2C` → `ApplicationUser` → `Controllers`
3. **Authorization** → `Role-based Access` → `Views` → `User Interface`

### **Deployment Flow**
1. **Development** → `Local Development` → `Azure Storage Emulator`
2. **Testing** → `Azure DevOps Pipeline` → `Staging Environment`
3. **Production** → `Infrastructure as Code` → `Azure Resources`

## 🎯 Project Architecture Summary

### **Layered Architecture**
- **Presentation Layer**: `Controllers/`, `Views/`, `wwwroot/`
- **Business Layer**: `Services/`, `DTOs/`
- **Data Layer**: `Data/`, `Models/`
- **Infrastructure Layer**: `Infrastructure/`, `Pipelines/`

### **Key Design Patterns**
- **Dependency Injection**: Service registration in `Program.cs`
- **Repository Pattern**: Data access through `ApplicationDbContext`
- **Service Layer Pattern**: Business logic in `Services/`
- **DTO Pattern**: Data transfer through `DTOs/`
- **MVC Pattern**: Separation of concerns in `Controllers/`, `Views/`, `Models/`

### **Security Implementation**
- **Authentication**: Azure AD B2C integration
- **Authorization**: Role-based access control
- **Data Protection**: Azure Key Vault for secrets
- **Audit Logging**: Comprehensive audit trail
- **Encryption**: Data at rest and in transit

### **Scalability Features**
- **Auto-scaling**: Azure App Service and Functions
- **Load Balancing**: Azure Load Balancer
- **Caching**: In-memory and distributed caching
- **Database Optimization**: Indexing and query optimization
- **CDN**: Azure CDN for static content

---

**This comprehensive file structure creates a complete, enterprise-grade AI-powered document intelligence platform with full documentation, deployment automation, and interview preparation materials.** 