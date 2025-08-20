using Azure.AI.FormRecognizer;
using Azure.AI.FormRecognizer.Models;
using Azure.AI.ComputerVision;
using Azure.AI.ComputerVision.Models;
using DocuSense.DTOs;
using DocuSense.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DocuSense.Services
{
    public class AzureCognitiveServices : IAzureCognitiveServices
    {
        private readonly FormRecognizerClient _formRecognizerClient;
        private readonly ComputerVisionClient _computerVisionClient;
        private readonly ILogger _logger;

        public AzureCognitiveServices(IConfiguration configuration, ILogger logger)
        {
            _logger = logger;

            var formRecognizerEndpoint = configuration["Azure:CognitiveServices:FormRecognizer:Endpoint"];
            var formRecognizerKey = configuration["Azure:CognitiveServices:FormRecognizer:Key"];
            var computerVisionEndpoint = configuration["Azure:CognitiveServices:ComputerVision:Endpoint"];
            var computerVisionKey = configuration["Azure:CognitiveServices:ComputerVision:Key"];

            if (!string.IsNullOrEmpty(formRecognizerEndpoint) && !string.IsNullOrEmpty(formRecognizerKey))
            {
                _formRecognizerClient = new FormRecognizerClient(new Uri(formRecognizerEndpoint), 
                    new Azure.AzureKeyCredential(formRecognizerKey));
            }

            if (!string.IsNullOrEmpty(computerVisionEndpoint) && !string.IsNullOrEmpty(computerVisionKey))
            {
                _computerVisionClient = new ComputerVisionClient(new Uri(computerVisionEndpoint), 
                    new Azure.AzureKeyCredential(computerVisionKey));
            }
        }

        public async Task<DocumentProcessingResultDto> ProcessDocumentWithFormRecognizerAsync(Stream documentStream, string fileName)
        {
            try
            {
                if (_formRecognizerClient == null)
                {
                    throw new InvalidOperationException("Form Recognizer client not configured");
                }

                _logger.Information("Processing document with Form Recognizer: {FileName}", fileName);

                // Analyze document layout
                var operation = await _formRecognizerClient.StartAnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", documentStream);
                var result = await operation.WaitForCompletionAsync();

                var extractedFields = new List<DocumentFieldDto>();

                if (result.Value.AnalyzeResult != null)
                {
                    foreach (var page in result.Value.AnalyzeResult.Pages)
                    {
                        foreach (var line in page.Lines)
                        {
                            extractedFields.Add(new DocumentFieldDto
                            {
                                Id = Guid.NewGuid(),
                                DocumentId = Guid.Empty, // Will be set by caller
                                FieldName = "Text",
                                FieldValue = line.Content,
                                FieldType = "Text",
                                Confidence = line.Confidence,
                                PageNumber = page.PageNumber,
                                ExtractedAt = DateTime.UtcNow,
                                ExtractedBy = "Azure Form Recognizer"
                            });
                        }
                    }

                    // Extract key-value pairs
                    if (result.Value.AnalyzeResult.KeyValuePairs != null)
                    {
                        foreach (var kvp in result.Value.AnalyzeResult.KeyValuePairs)
                        {
                            if (kvp.Key != null && kvp.Value != null)
                            {
                                extractedFields.Add(new DocumentFieldDto
                                {
                                    Id = Guid.NewGuid(),
                                    DocumentId = Guid.Empty,
                                    FieldName = kvp.Key.Content,
                                    FieldValue = kvp.Value.Content,
                                    FieldType = "KeyValuePair",
                                    Confidence = kvp.Confidence,
                                    PageNumber = kvp.Key.BoundingRegions?.FirstOrDefault()?.PageNumber,
                                    ExtractedAt = DateTime.UtcNow,
                                    ExtractedBy = "Azure Form Recognizer"
                                });
                            }
                        }
                    }
                }

                return new DocumentProcessingResultDto
                {
                    DocumentId = Guid.Empty,
                    Status = "Success",
                    ProcessedAt = DateTime.UtcNow,
                    ProcessingResult = $"Successfully processed {extractedFields.Count} fields",
                    ExtractedFields = extractedFields
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing document with Form Recognizer: {FileName}", fileName);
                return new DocumentProcessingResultDto
                {
                    DocumentId = Guid.Empty,
                    Status = "Failed",
                    ProcessedAt = DateTime.UtcNow,
                    ErrorMessage = ex.Message,
                    ExtractedFields = new List<DocumentFieldDto>()
                };
            }
        }

        public async Task<DocumentProcessingResultDto> ProcessDocumentWithComputerVisionAsync(Stream documentStream, string fileName)
        {
            try
            {
                if (_computerVisionClient == null)
                {
                    throw new InvalidOperationException("Computer Vision client not configured");
                }

                _logger.Information("Processing document with Computer Vision: {FileName}", fileName);

                var extractedFields = new List<DocumentFieldDto>();

                // Extract text using OCR
                var ocrResult = await _computerVisionClient.ReadInStreamAsync(documentStream);
                var operationLocation = ocrResult.Headers.OperationLocation;
                var operationId = operationLocation.Substring(operationLocation.LastIndexOf('/') + 1);

                // Wait for the operation to complete
                var readResult = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
                while (readResult.Value.Status == OperationStatusCodes.Running || readResult.Value.Status == OperationStatusCodes.NotStarted)
                {
                    await Task.Delay(1000);
                    readResult = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
                }

                if (readResult.Value.Status == OperationStatusCodes.Succeeded)
                {
                    var textResults = readResult.Value.AnalyzeResult.ReadResults;
                    foreach (var textResult in textResults)
                    {
                        foreach (var line in textResult.Lines)
                        {
                            extractedFields.Add(new DocumentFieldDto
                            {
                                Id = Guid.NewGuid(),
                                DocumentId = Guid.Empty,
                                FieldName = "Text",
                                FieldValue = line.Text,
                                FieldType = "Text",
                                Confidence = line.Appearance?.Confidence ?? 0.0,
                                PageNumber = textResult.Page,
                                ExtractedAt = DateTime.UtcNow,
                                ExtractedBy = "Azure Computer Vision"
                            });
                        }
                    }
                }

                return new DocumentProcessingResultDto
                {
                    DocumentId = Guid.Empty,
                    Status = "Success",
                    ProcessedAt = DateTime.UtcNow,
                    ProcessingResult = $"Successfully extracted {extractedFields.Count} text lines",
                    ExtractedFields = extractedFields
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error processing document with Computer Vision: {FileName}", fileName);
                return new DocumentProcessingResultDto
                {
                    DocumentId = Guid.Empty,
                    Status = "Failed",
                    ProcessedAt = DateTime.UtcNow,
                    ErrorMessage = ex.Message,
                    ExtractedFields = new List<DocumentFieldDto>()
                };
            }
        }

        public async Task<List<DocumentFieldDto>> ExtractTextFromImageAsync(Stream imageStream)
        {
            try
            {
                if (_computerVisionClient == null)
                {
                    throw new InvalidOperationException("Computer Vision client not configured");
                }

                var extractedFields = new List<DocumentFieldDto>();

                // Extract text using OCR
                var ocrResult = await _computerVisionClient.ReadInStreamAsync(imageStream);
                var operationLocation = ocrResult.Headers.OperationLocation;
                var operationId = operationLocation.Substring(operationLocation.LastIndexOf('/') + 1);

                // Wait for the operation to complete
                var readResult = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
                while (readResult.Value.Status == OperationStatusCodes.Running || readResult.Value.Status == OperationStatusCodes.NotStarted)
                {
                    await Task.Delay(1000);
                    readResult = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
                }

                if (readResult.Value.Status == OperationStatusCodes.Succeeded)
                {
                    var textResults = readResult.Value.AnalyzeResult.ReadResults;
                    foreach (var textResult in textResults)
                    {
                        foreach (var line in textResult.Lines)
                        {
                            extractedFields.Add(new DocumentFieldDto
                            {
                                Id = Guid.NewGuid(),
                                DocumentId = Guid.Empty,
                                FieldName = "Text",
                                FieldValue = line.Text,
                                FieldType = "Text",
                                Confidence = line.Appearance?.Confidence ?? 0.0,
                                PageNumber = textResult.Page,
                                ExtractedAt = DateTime.UtcNow,
                                ExtractedBy = "Azure Computer Vision"
                            });
                        }
                    }
                }

                return extractedFields;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error extracting text from image");
                throw;
            }
        }

        public async Task<List<DocumentFieldDto>> ExtractFormFieldsAsync(Stream documentStream)
        {
            try
            {
                if (_formRecognizerClient == null)
                {
                    throw new InvalidOperationException("Form Recognizer client not configured");
                }

                var extractedFields = new List<DocumentFieldDto>();

                // Analyze document layout
                var operation = await _formRecognizerClient.StartAnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-document", documentStream);
                var result = await operation.WaitForCompletionAsync();

                if (result.Value.AnalyzeResult?.KeyValuePairs != null)
                {
                    foreach (var kvp in result.Value.AnalyzeResult.KeyValuePairs)
                    {
                        if (kvp.Key != null && kvp.Value != null)
                        {
                            extractedFields.Add(new DocumentFieldDto
                            {
                                Id = Guid.NewGuid(),
                                DocumentId = Guid.Empty,
                                FieldName = kvp.Key.Content,
                                FieldValue = kvp.Value.Content,
                                FieldType = "FormField",
                                Confidence = kvp.Confidence,
                                PageNumber = kvp.Key.BoundingRegions?.FirstOrDefault()?.PageNumber,
                                ExtractedAt = DateTime.UtcNow,
                                ExtractedBy = "Azure Form Recognizer"
                            });
                        }
                    }
                }

                return extractedFields;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error extracting form fields");
                throw;
            }
        }

        public async Task<string> ExtractTextAsync(Stream documentStream)
        {
            try
            {
                if (_computerVisionClient == null)
                {
                    throw new InvalidOperationException("Computer Vision client not configured");
                }

                var extractedText = new List<string>();

                // Extract text using OCR
                var ocrResult = await _computerVisionClient.ReadInStreamAsync(documentStream);
                var operationLocation = ocrResult.Headers.OperationLocation;
                var operationId = operationLocation.Substring(operationLocation.LastIndexOf('/') + 1);

                // Wait for the operation to complete
                var readResult = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
                while (readResult.Value.Status == OperationStatusCodes.Running || readResult.Value.Status == OperationStatusCodes.NotStarted)
                {
                    await Task.Delay(1000);
                    readResult = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
                }

                if (readResult.Value.Status == OperationStatusCodes.Succeeded)
                {
                    var textResults = readResult.Value.AnalyzeResult.ReadResults;
                    foreach (var textResult in textResults)
                    {
                        foreach (var line in textResult.Lines)
                        {
                            extractedText.Add(line.Text);
                        }
                    }
                }

                return string.Join(Environment.NewLine, extractedText);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error extracting text");
                throw;
            }
        }

        public async Task<double> GetDocumentConfidenceAsync(Stream documentStream)
        {
            try
            {
                if (_computerVisionClient == null)
                {
                    return 0.0;
                }

                var ocrResult = await _computerVisionClient.ReadInStreamAsync(documentStream);
                var operationLocation = ocrResult.Headers.OperationLocation;
                var operationId = operationLocation.Substring(operationLocation.LastIndexOf('/') + 1);

                // Wait for the operation to complete
                var readResult = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
                while (readResult.Value.Status == OperationStatusCodes.Running || readResult.Value.Status == OperationStatusCodes.NotStarted)
                {
                    await Task.Delay(1000);
                    readResult = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
                }

                if (readResult.Value.Status == OperationStatusCodes.Succeeded)
                {
                    var textResults = readResult.Value.AnalyzeResult.ReadResults;
                    var totalConfidence = 0.0;
                    var lineCount = 0;

                    foreach (var textResult in textResults)
                    {
                        foreach (var line in textResult.Lines)
                        {
                            totalConfidence += line.Appearance?.Confidence ?? 0.0;
                            lineCount++;
                        }
                    }

                    return lineCount > 0 ? totalConfidence / lineCount : 0.0;
                }

                return 0.0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting document confidence");
                return 0.0;
            }
        }

        public async Task<bool> IsDocumentValidAsync(Stream documentStream, string fileName)
        {
            try
            {
                // Check file extension
                var extension = Path.GetExtension(fileName).ToLower();
                var validExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".tiff", ".bmp" };
                
                if (!validExtensions.Contains(extension))
                {
                    return false;
                }

                // Check file size (max 10MB)
                if (documentStream.Length > 10 * 1024 * 1024)
                {
                    return false;
                }

                // Try to process a small portion to validate
                var confidence = await GetDocumentConfidenceAsync(documentStream);
                return confidence > 0.1; // Minimum confidence threshold
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error validating document: {FileName}", fileName);
                return false;
            }
        }

        public async Task<string> GetDocumentTypeAsync(Stream documentStream)
        {
            try
            {
                if (_computerVisionClient == null)
                {
                    return "Unknown";
                }

                // Analyze image to determine document type
                var features = new List<VisualFeatureTypes?> { VisualFeatureTypes.Description };
                var result = await _computerVisionClient.AnalyzeImageInStreamAsync(documentStream, features);

                if (result.Value.Description?.Captions != null && result.Value.Description.Captions.Any())
                {
                    var description = result.Value.Description.Captions.First().Text.ToLower();
                    
                    if (description.Contains("form") || description.Contains("document"))
                        return "Form";
                    else if (description.Contains("receipt") || description.Contains("invoice"))
                        return "Receipt";
                    else if (description.Contains("id") || description.Contains("card"))
                        return "ID";
                    else
                        return "Document";
                }

                return "Document";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error determining document type");
                return "Unknown";
            }
        }

        public async Task<List<string>> GetSupportedLanguagesAsync()
        {
            return new List<string>
            {
                "en", "es", "fr", "de", "it", "pt", "nl", "pl", "ru", "ja", "ko", "zh"
            };
        }

        public async Task<DocumentProcessingResultDto> AnalyzeDocumentLayoutAsync(Stream documentStream)
        {
            try
            {
                if (_formRecognizerClient == null)
                {
                    throw new InvalidOperationException("Form Recognizer client not configured");
                }

                var extractedFields = new List<DocumentFieldDto>();

                // Analyze document layout
                var operation = await _formRecognizerClient.StartAnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-layout", documentStream);
                var result = await operation.WaitForCompletionAsync();

                if (result.Value.AnalyzeResult != null)
                {
                    foreach (var page in result.Value.AnalyzeResult.Pages)
                    {
                        foreach (var table in page.Tables)
                        {
                            for (int row = 0; row < table.RowCount; row++)
                            {
                                for (int col = 0; col < table.ColumnCount; col++)
                                {
                                    var cell = table.Cells.FirstOrDefault(c => c.RowIndex == row && c.ColumnIndex == col);
                                    if (cell != null)
                                    {
                                        extractedFields.Add(new DocumentFieldDto
                                        {
                                            Id = Guid.NewGuid(),
                                            DocumentId = Guid.Empty,
                                            FieldName = $"Table_{table.RowIndex}_{row}_{col}",
                                            FieldValue = cell.Content,
                                            FieldType = "TableCell",
                                            Confidence = cell.Confidence,
                                            PageNumber = page.PageNumber,
                                            ExtractedAt = DateTime.UtcNow,
                                            ExtractedBy = "Azure Form Recognizer"
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                return new DocumentProcessingResultDto
                {
                    DocumentId = Guid.Empty,
                    Status = "Success",
                    ProcessedAt = DateTime.UtcNow,
                    ProcessingResult = $"Successfully analyzed layout with {extractedFields.Count} elements",
                    ExtractedFields = extractedFields
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error analyzing document layout");
                return new DocumentProcessingResultDto
                {
                    DocumentId = Guid.Empty,
                    Status = "Failed",
                    ProcessedAt = DateTime.UtcNow,
                    ErrorMessage = ex.Message,
                    ExtractedFields = new List<DocumentFieldDto>()
                };
            }
        }
    }
} 