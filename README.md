# DocuSense – AI-Powered Document Intelligence Platform

##  Overview

DocuSense is an enterprise-grade, cloud-native platform built using C# with ASP.NET Core MVC and Microsoft Azure services. It enables users to upload, analyze, extract, and audit data from documents (PDFs, images, reports) using Azure Cognitive Services, with full support for security, auditability, and DevOps automation.

## 🔧 Tech Stack

### Frontend
- ASP.NET Core MVC (Razor Views + Controllers)

### Backend
- C# .NET 8 Web API, Services, DTOs, Repositories

### Database
- Azure SQL (structured data), Cosmos DB (logs)

### File Storage
- Azure Blob Storage (document uploads)

### AI Services
- Azure Cognitive Services (OCR), Azure ML (optional classification/anomaly scoring)

### Identity & Access
- Azure Active Directory B2C (RBAC)

### Deployment
- Azure App Services (API + MVC), Azure Functions

### DevOps
- Azure DevOps (CI/CD YAML pipelines)

### Monitoring
- Azure Monitor, Application Insights

### Visualization
- Optional Power BI integration for audit/reporting

## 📂 Project Structure

```
/DocuSense
├── /Controllers             # ASP.NET Core MVC Controllers
├── /Models                  # Domain Models + ViewModels
├── /Views                   # Razor Views (.cshtml pages)
├── /Services                # Business Logic (Interfaces + Implementations)
├── /Repositories            # Data Access Layer using EF Core
├── /DTOs                    # Clean object transfer between layers
├── /AzureFunctions          # OCR Trigger + Document Analyzer Functions
├── /Data                    # DbContext, Migrations, Seeders
├── /KeyVaultClient          # Secure Azure Key Vault Integration
├── /Infrastructure          # Bicep templates + YAML pipeline configs
├── /Pipelines               # Azure DevOps build & release pipelines
├── /PowerBI                 # PBIX templates or embed scripts (optional)
├── /wwwroot                 # Static assets: JS, CSS, Images
├── /Logs                    # Custom logs or output logs
└── README.md                # This file
```

##  Key Modules

### 1. Document Upload & Storage
- Users upload documents through Razor UI
- Files stored in Azure Blob Storage under user/project-based containers
- Metadata saved to Azure SQL (file ID, user ID, timestamps, status)

### 2. AI Processing Pipeline
- Azure Function triggers OCR via Azure Cognitive Services
- Parsed data saved in structured format to SQL or NoSQL

### 3. User Authentication & Roles
- Managed by Azure AD B2C
- Roles: Admin, Reviewer, User
- Razor Views rendered dynamically based on role permissions

### 4. Admin Dashboard & Audit Logs
- Visualize uploads, processing status, anomalies
- Downloadable reports (CSV/PDF)
- Data stored in Cosmos DB and optionally visualized using Power BI

### 5. Monitoring & Logging
- Integrate App Insights in API, MVC, and Azure Functions
- Custom telemetry for failed OCRs, API latency, and abnormal activity
- Alerts via Azure Monitor

##  Azure Services Used

- **Azure App Service** (host MVC and API)
- **Azure Blob Storage** (store documents securely)
- **Azure Cognitive Services** (OCR extraction)
- **Azure SQL** (processed structured data)
- **Azure Cosmos DB** (logs, audit data)
- **Azure Functions** (trigger document processing)
- **Azure DevOps** (CI/CD pipeline)
- **Azure Key Vault** (secret/config storage)
- **Azure AD B2C** (secure login & RBAC)
- **Azure Monitor + App Insights** (full observability)

##  User Flow

1. User logs in via Azure AD B2C
2. Uploads document (PDF/image)
3. Document is stored in Blob and triggers Azure Function
4. Function processes file using OCR and saves result in SQL/Cosmos
5. User sees extracted fields and status on dashboard
6. Admin can view logs, download reports, monitor anomalies

## Getting Started

### Prerequisites
- .NET 8 SDK
- Azure Subscription
- Azure CLI
- Visual Studio 2022 or VS Code

### Local Development Setup
1. Clone the repository
2. Configure Azure services (see Infrastructure/README.md)
3. Update appsettings.json with your Azure connection strings
4. Run `dotnet restore` and `dotnet run`

### Azure Deployment
1. Deploy infrastructure using Bicep templates
2. Configure CI/CD pipelines
3. Deploy to Azure App Service

##  License

This project is licensed under the MIT License - see the LICENSE file for details. 
