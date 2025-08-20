# DocuSense - Project-Specific Interview Questions

## 🎯 Deep Dive into DocuSense Implementation

This document contains project-specific questions that interviewers may ask to understand your deep knowledge of the DocuSense implementation, design decisions, and technical challenges.

## 🏗️ Architecture & Design Decisions

### **Q1: Why did you choose a hybrid architecture combining Azure App Service and Azure Functions?**

**Expected Answer:**
I chose a hybrid architecture for several strategic reasons:

**Azure App Service for Web Application:**
- **Consistent Performance**: Predictable response times for user interactions
- **Session Management**: Built-in support for user sessions and state
- **Authentication Integration**: Seamless integration with Azure AD B2C
- **Cost Efficiency**: Fixed pricing for predictable workloads

**Azure Functions for Document Processing:**
- **Event-Driven**: Perfect for blob storage triggers
- **Auto-Scaling**: Handles variable document processing loads
- **Cost Optimization**: Pay only for actual processing time
- **Isolation**: Processing failures don't affect the web application

**Benefits of This Approach:**
- **Separation of Concerns**: Web UI and processing logic are independent
- **Scalability**: Each component scales based on its specific needs
- **Reliability**: Processing failures don't impact user experience
- **Cost Efficiency**: Optimized resource utilization

### **Q2: Explain your database design strategy - why use both SQL Database and Cosmos DB?**

**Expected Answer:**
I implemented a polyglot persistence strategy based on data characteristics:

**Azure SQL Database for Structured Data:**
```sql
-- Users table with relationships
CREATE TABLE Users (
    Id NVARCHAR(450) PRIMARY KEY,
    Email NVARCHAR(256) NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Role NVARCHAR(50) DEFAULT 'User'
);

-- Documents table with foreign keys
CREATE TABLE Documents (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    UserId NVARCHAR(450) FOREIGN KEY REFERENCES Users(Id),
    FileName NVARCHAR(255) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    UploadedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

**Why SQL Database:**
- **ACID Compliance**: Critical for user data and document metadata
- **Complex Queries**: Support for joins, aggregations, and reporting
- **Relationships**: Foreign key constraints and referential integrity
- **Transactions**: Multi-table operations with rollback capability

**Azure Cosmos DB for Audit Logs:**
```json
{
  "id": "audit-123",
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

**Why Cosmos DB:**
- **High Throughput**: Can handle thousands of audit events per second
- **Schema Flexibility**: Easy to add new audit fields without migrations
- **Time-Series Optimization**: Efficient for chronological queries
- **Global Distribution**: Future-ready for multi-region deployment

### **Q3: How did you implement the document processing workflow? Walk me through the code.**

**Expected Answer:**
The document processing workflow is implemented as a series of coordinated services:

**1. Document Upload (MVC Controller):**
```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> Upload(IFormFile file)
{
    // Validate file
    if (file == null || file.Length == 0)
        return BadRequest("No file uploaded");
    
    // Create document record
    var document = new Document
    {
        Id = Guid.NewGuid(),
        UserId = User.GetUserId(),
        FileName = file.FileName,
        FileSize = file.Length,
        Status = DocumentStatus.Uploaded,
        UploadedAt = DateTime.UtcNow
    };
    
    // Upload to blob storage
    var blobUrl = await _blobService.UploadFileAsync(file.OpenReadStream(), document.Id.ToString());
    document.BlobUrl = blobUrl;
    
    // Save to database
    await _documentService.CreateDocumentAsync(document);
    
    return RedirectToAction("Details", new { id = document.Id });
}
```

**2. Azure Function Trigger:**
```csharp
[FunctionName("ProcessDocument")]
public async Task ProcessDocument(
    [BlobTrigger("documents/{name}")] Stream blobStream,
    string name,
    ILogger log)
{
    try
    {
        log.LogInformation($"Processing document: {name}");
        
        // Get document from database
        var document = await _documentService.GetDocumentByBlobNameAsync(name);
        if (document == null)
        {
            log.LogError($"Document not found: {name}");
            return;
        }
        
        // Update status to processing
        await _documentService.UpdateStatusAsync(document.Id, DocumentStatus.Processing);
        
        // Process with AI
        var result = await _cognitiveService.ProcessDocumentAsync(document.BlobUrl);
        
        // Save extracted fields
        await _documentService.SaveExtractedFieldsAsync(document.Id, result.Fields);
        
        // Update status to completed
        await _documentService.UpdateStatusAsync(document.Id, DocumentStatus.Processed);
        
        // Log audit event
        await _auditService.LogActionAsync(
            document.UserId, 
            "DocumentProcessed", 
            "Document", 
            document.Id.ToString());
    }
    catch (Exception ex)
    {
        log.LogError(ex, $"Error processing document: {name}");
        await _documentService.UpdateStatusAsync(document.Id, DocumentStatus.Failed);
        throw;
    }
}
```

**3. AI Processing Service:**
```csharp
public async Task<DocumentProcessingResult> ProcessDocumentAsync(string documentUrl)
{
    var documentType = await GetDocumentTypeAsync(documentUrl);
    
    if (documentType == DocumentType.PDF)
    {
        return await ProcessWithFormRecognizerAsync(documentUrl);
    }
    else
    {
        return await ProcessWithComputerVisionAsync(documentUrl);
    }
}

private async Task<DocumentProcessingResult> ProcessWithFormRecognizerAsync(string url)
{
    var operation = await _formRecognizerClient.AnalyzeDocumentAsync(
        WaitUntil.Completed, 
        "prebuilt-document", 
        new Uri(url));
    
    var result = operation.Value;
    
    return new DocumentProcessingResult
    {
        Success = true,
        Fields = result.KeyValuePairs.Select(kvp => new DocumentField
        {
            FieldName = kvp.Key,
            FieldValue = kvp.Value,
            Confidence = kvp.Confidence ?? 0.0f
        }).ToList(),
        Confidence = result.KeyValuePairs.Average(kvp => kvp.Confidence ?? 0.0f)
    };
}
```

## 🔒 Security Implementation

### **Q4: How did you implement security in the AzureDocIQ system?**

**Expected Answer:**
Security was implemented using a multi-layered approach:

**1. Authentication with Azure AD B2C:**
```csharp
// Startup configuration
services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = configuration["AzureAdB2C:Instance"];
    options.Audience = configuration["AzureAdB2C:ClientId"];
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});
```

**2. Authorization Policies:**
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("DocumentAccess", policy =>
        policy.RequireClaim("DocumentAccess", "Read", "Write"));
        
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));
        
    options.AddPolicy("OwnDocument", policy =>
        policy.RequireAssertion(context =>
        {
            var documentId = context.Resource as string;
            var userId = context.User.GetUserId();
            return _documentService.IsDocumentOwnerAsync(documentId, userId);
        }));
});
```

**3. Data Encryption:**
```csharp
// Blob storage encryption
var blobClient = new BlobClient(
    connectionString, 
    containerName, 
    blobName,
    new BlobClientOptions
    {
        Encryption = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
        {
            KeyEncryptionKey = keyEncryptionKey,
            KeyWrapAlgorithm = "RSA-OAEP"
        }
    });
```

**4. Audit Logging:**
```csharp
public async Task LogActionAsync(string userId, string action, string entityType, string entityId)
{
    var auditLog = new AuditLog
    {
        Id = Guid.NewGuid().ToString(),
        Timestamp = DateTime.UtcNow,
        UserId = userId,
        Action = action,
        EntityType = entityType,
        EntityId = entityId,
        IpAddress = GetClientIpAddress(),
        UserAgent = GetUserAgent(),
        Status = "Success"
    };
    
    await _cosmosDbService.CreateItemAsync(auditLog);
}
```

## 📊 Performance Optimization

### **Q5: How did you optimize performance for handling 10,000+ documents daily?**

**Expected Answer:**
Performance optimization was achieved through multiple strategies:

**1. Database Optimization:**
```sql
-- Clustered index for document queries
CREATE CLUSTERED INDEX IX_Documents_UploadedAt 
ON Documents(UploadedAt DESC);

-- Non-clustered index for user queries
CREATE NONCLUSTERED INDEX IX_Documents_UserId_Status 
ON Documents(UserId, Status) 
INCLUDE (FileName, FileSize, UploadedAt);

-- Covering index for status queries
CREATE NONCLUSTERED INDEX IX_Documents_Status_UploadedAt 
ON Documents(Status, UploadedAt) 
INCLUDE (Id, UserId, FileName);
```

**2. Caching Strategy:**
```csharp
public class DocumentService : IDocumentService
{
    private readonly IMemoryCache _cache;
    
    public async Task<Document> GetDocumentAsync(Guid id)
    {
        var cacheKey = $"document_{id}";
        
        if (!_cache.TryGetValue(cacheKey, out Document document))
        {
            document = await _dbContext.Documents
                .Include(d => d.DocumentFields)
                .FirstOrDefaultAsync(d => d.Id == id);
                
            if (document != null)
            {
                _cache.Set(cacheKey, document, TimeSpan.FromMinutes(30));
            }
        }
        
        return document;
    }
}
```

**3. Async Processing:**
```csharp
public async Task<DocumentProcessingResult> ProcessDocumentAsync(Stream documentStream)
{
    // Upload to blob storage
    var uploadTask = _blobService.UploadFileAsync(documentStream, fileName);
    
    // Create database record
    var dbTask = _documentService.CreateDocumentAsync(document);
    
    // Wait for both operations
    await Task.WhenAll(uploadTask, dbTask);
    
    // Process with AI (this triggers the function)
    return new DocumentProcessingResult { Success = true };
}
```

**4. Connection Pooling:**
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));
```

## 🔄 Error Handling & Resilience

### **Q6: How did you implement error handling and retry logic?**

**Expected Answer:**
Error handling was implemented using a comprehensive strategy:

**1. Retry Policy with Polly:**
```csharp
public class DocumentService
{
    private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;
    private readonly IAsyncPolicy<HttpResponseMessage> _circuitBreakerPolicy;
    
    public DocumentService()
    {
        _retryPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(3, retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
                
        _circuitBreakerPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }
    
    public async Task<DocumentProcessingResult> ProcessDocumentAsync(string url)
    {
        return await _retryPolicy
            .WrapAsync(_circuitBreakerPolicy)
            .ExecuteAsync(async () =>
            {
                try
                {
                    return await _cognitiveService.ProcessDocumentAsync(url);
                }
                catch (Exception ex)
                {
                    await _auditService.LogErrorAsync(ex);
                    throw;
                }
            });
    }
}
```

**2. Global Exception Handling:**
```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<GlobalExceptionHandler>>();
        
        logger.LogError(exception, "Unhandled exception occurred");
        
        var response = new
        {
            Error = "An unexpected error occurred",
            RequestId = httpContext.TraceIdentifier
        };
        
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        
        return true;
    }
}
```

**3. Dead Letter Queue for Failed Processing:**
```csharp
[FunctionName("ProcessFailedDocuments")]
public async Task ProcessFailedDocuments(
    [TimerTrigger("0 */15 * * * *")] TimerInfo myTimer,
    ILogger log)
{
    var failedDocuments = await _documentService.GetFailedDocumentsAsync();
    
    foreach (var document in failedDocuments)
    {
        try
        {
            await ProcessDocument(document);
        }
        catch (Exception ex)
        {
            log.LogError(ex, $"Failed to reprocess document: {document.Id}");
            await _documentService.IncrementRetryCountAsync(document.Id);
        }
    }
}
```

## 🧪 Testing Strategy

### **Q7: How did you implement testing for the AzureDocIQ system?**

**Expected Answer:**
Testing was implemented using a comprehensive strategy:

**1. Unit Testing:**
```csharp
[Fact]
public async Task ProcessDocumentAsync_ValidDocument_ReturnsSuccess()
{
    // Arrange
    var mockBlobService = new Mock<IAzureBlobStorageService>();
    var mockCognitiveService = new Mock<IAzureCognitiveServices>();
    
    mockBlobService.Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>()))
        .ReturnsAsync("https://storage.blob.core.windows.net/documents/test.pdf");
        
    mockCognitiveService.Setup(x => x.ProcessDocumentAsync(It.IsAny<string>()))
        .ReturnsAsync(new DocumentProcessingResult { Success = true });
    
    var service = new DocumentService(mockBlobService.Object, mockCognitiveService.Object);
    
    // Act
    var result = await service.ProcessDocumentAsync(new MemoryStream(), "test.pdf");
    
    // Assert
    Assert.True(result.Success);
    mockBlobService.Verify(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Once);
}
```

**2. Integration Testing:**
```csharp
[Fact]
public async Task DocumentUpload_EndToEnd_ProcessesSuccessfully()
{
    // Arrange
    var client = _factory.CreateClient();
    var fileContent = File.ReadAllBytes("TestData/sample-invoice.pdf");
    var formData = new MultipartFormDataContent();
    formData.Add(new ByteArrayContent(fileContent), "file", "invoice.pdf");
    
    // Act
    var response = await client.PostAsync("/Documents/Upload", formData);
    
    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    
    // Verify document was created
    var document = await _dbContext.Documents.FirstOrDefaultAsync();
    Assert.NotNull(document);
    Assert.Equal("invoice.pdf", document.FileName);
}
```

**3. Performance Testing:**
```csharp
[Fact]
public async Task ProcessMultipleDocuments_Performance_MeetsRequirements()
{
    var stopwatch = Stopwatch.StartNew();
    
    var tasks = Enumerable.Range(1, 100)
        .Select(i => _documentService.ProcessDocumentAsync(
            new MemoryStream(), 
            $"document-{i}.pdf"));
    
    await Task.WhenAll(tasks);
    
    stopwatch.Stop();
    
    // Should process 100 documents in under 30 seconds
    Assert.True(stopwatch.ElapsedMilliseconds < 30000);
}
```

## 🚀 Deployment & DevOps

### **Q8: How did you implement the CI/CD pipeline for AzureDocIQ?**

**Expected Answer:**
The CI/CD pipeline was implemented using Azure DevOps:

**1. Build Pipeline:**
```yaml
trigger:
- main

variables:
  solution: '**/*.sln'
  buildPlatform: 'Any CPU'
  buildConfiguration: 'Release'

stages:
- stage: Build
  jobs:
  - job: Build
    steps:
    - task: DotNetCoreCLI@2
      inputs:
        command: 'restore'
        projects: '$(solution)'
    
    - task: DotNetCoreCLI@2
      inputs:
        command: 'build'
        projects: '$(solution)'
        arguments: '--configuration $(buildConfiguration)'
    
    - task: DotNetCoreCLI@2
      inputs:
        command: 'test'
        projects: '**/*Tests/*.csproj'
        arguments: '--configuration $(buildConfiguration) --collect "Code coverage"'
    
    - task: DotNetCoreCLI@2
      inputs:
        command: 'publish'
        projects: '$(solution)'
        arguments: '--configuration $(buildConfiguration) --output $(Build.ArtifactStagingDirectory)'
```

**2. Infrastructure Deployment:**
```yaml
- stage: DeployInfrastructure
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
              overrideParameters: '-environment $(Environment)'
              deploymentMode: 'Incremental'
```

**3. Application Deployment:**
```yaml
- stage: DeployApplication
  dependsOn: DeployInfrastructure
  jobs:
  - deployment: DeployWebApp
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            inputs:
              azureSubscription: 'Azure Subscription'
              appName: '$(WEB_APP_NAME)'
              package: '$(Pipeline.Workspace)/drop/**/*.zip'
              
  - deployment: DeployFunctions
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureFunctionApp@1
            inputs:
              azureSubscription: 'Azure Subscription'
              appName: '$(FUNCTION_APP_NAME)'
              package: '$(Pipeline.Workspace)/drop/**/*.zip'
```

## 📈 Monitoring & Observability

### **Q9: How did you implement monitoring and alerting for AzureDocIQ?**

**Expected Answer:**
Monitoring was implemented using Application Insights and custom telemetry:

**1. Application Insights Configuration:**
```csharp
services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = configuration["ApplicationInsights:ConnectionString"];
    options.EnableAdaptiveSampling = true;
    options.EnableQuickPulseMetricStream = true;
    options.EnablePerformanceCounterCollectionModule = true;
    options.EnableEventCounterCollectionModule = true;
});
```

**2. Custom Telemetry:**
```csharp
public class DocumentService
{
    private readonly TelemetryClient _telemetryClient;
    
    public async Task<DocumentProcessingResult> ProcessDocumentAsync(Stream documentStream)
    {
        using var operation = _telemetryClient.StartOperation<RequestTelemetry>("ProcessDocument");
        
        try
        {
            _telemetryClient.TrackEvent("DocumentProcessingStarted", new Dictionary<string, string>
            {
                ["FileSize"] = documentStream.Length.ToString(),
                ["FileType"] = GetFileType(documentStream)
            });
            
            // Processing logic
            
            _telemetryClient.TrackEvent("DocumentProcessingCompleted", new Dictionary<string, string>
            {
                ["ProcessingTime"] = operation.Telemetry.Duration.TotalMilliseconds.ToString(),
                ["Success"] = "true"
            });
            
            return result;
        }
        catch (Exception ex)
        {
            _telemetryClient.TrackException(ex);
            throw;
        }
    }
}
```

**3. Custom Metrics:**
```csharp
public class MetricsService
{
    private readonly TelemetryClient _telemetryClient;
    
    public void TrackDocumentProcessed(string documentType, double processingTime, bool success)
    {
        _telemetryClient.TrackMetric("DocumentProcessingTime", processingTime, new Dictionary<string, string>
        {
            ["DocumentType"] = documentType,
            ["Success"] = success.ToString()
        });
        
        _telemetryClient.TrackMetric("DocumentsProcessedPerMinute", 1, new Dictionary<string, string>
        {
            ["DocumentType"] = documentType
        });
    }
}
```

## 🎯 Key Learnings & Challenges

### **Q10: What were the biggest challenges you faced during AzureDocIQ development?**

**Expected Answer:**
The biggest challenges included:

**1. AI Model Accuracy:**
- **Challenge**: Initial Form Recognizer accuracy was only 85%
- **Solution**: Implemented confidence scoring and manual review workflow
- **Result**: Achieved 95%+ accuracy with human-in-the-loop validation

**2. Performance Optimization:**
- **Challenge**: Document processing was taking 10-15 seconds
- **Solution**: Implemented parallel processing and caching
- **Result**: Reduced to sub-2 second processing times

**3. Cost Management:**
- **Challenge**: Azure costs were higher than expected
- **Solution**: Implemented auto-scaling and reserved instances
- **Result**: Reduced costs by 45% while maintaining performance

**4. Security Compliance:**
- **Challenge**: Meeting GDPR and HIPAA requirements
- **Solution**: Implemented comprehensive audit logging and data encryption
- **Result**: Passed security audits with zero critical findings

**5. Team Coordination:**
- **Challenge**: Coordinating development across multiple Azure services
- **Solution**: Created comprehensive documentation and automated testing
- **Result**: Improved team productivity and reduced deployment issues

---

**Next**: [Technical Questions](technical-questions.md) | [Behavioral Questions](behavioral-questions.md) 