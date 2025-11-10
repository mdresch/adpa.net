using ADPA.Services.Intelligence;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace ADPA.Services.Intelligence;

/// <summary>
/// Document classification service using ML.NET
/// Categorizes documents based on their text content
/// </summary>
public class MlNetDocumentClassificationService : IDocumentClassificationService
{
    private readonly ILogger<MlNetDocumentClassificationService> _logger;
    private readonly MLContext _mlContext;
    private ITransformer? _trainedModel;
    private readonly string _modelPath;

    // Default document categories for classification
    private readonly string[] _defaultCategories = {
        "Business", "Legal", "Technical", "Financial", "Medical", 
        "Educational", "Personal", "Government", "Marketing", "Other"
    };

    public MlNetDocumentClassificationService(
        ILogger<MlNetDocumentClassificationService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _mlContext = new MLContext(seed: 0);
        _modelPath = configuration["Classification:ModelPath"] ?? "./models/document_classifier.zip";
        
        // Try to load existing model
        LoadModelIfExists();
        
        // If no model exists, create a default one
        if (_trainedModel == null)
        {
            _logger.LogInformation("🚀 No existing classification model found. Creating default model...");
            Task.Run(async () => await CreateDefaultModelAsync());
        }
    }

    /// <summary>
    /// Classify document text into categories
    /// </summary>
    public async Task<DocumentClassification> ClassifyAsync(string documentText, ClassificationOptions? options = null)
    {
        try
        {
            _logger.LogInformation("🔍 Starting document classification for text length: {TextLength}", documentText.Length);

            options ??= new ClassificationOptions();

            // Validate input
            if (string.IsNullOrWhiteSpace(documentText))
            {
                return new DocumentClassification
                {
                    Success = false,
                    ErrorMessage = "Document text cannot be empty"
                };
            }

            // Ensure model is available
            if (_trainedModel == null)
            {
                await CreateDefaultModelAsync();
                if (_trainedModel == null)
                {
                    return new DocumentClassification
                    {
                        Success = false,
                        ErrorMessage = "Classification model is not available"
                    };
                }
            }

            // Prepare text for classification
            var processedText = PreprocessText(documentText);
            
            // Create prediction engine
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<DocumentData, DocumentPrediction>(_trainedModel);

            // Make prediction
            var input = new DocumentData { Text = processedText };
            var prediction = predictionEngine.Predict(input);

            // Process results
            var result = ProcessPredictionResult(prediction, options);

            _logger.LogInformation("✅ Document classified as '{Category}' with confidence {Confidence:F2}", 
                result.PrimaryCategory, result.PrimaryConfidence);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Document classification failed");
            return new DocumentClassification
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Get available document categories
    /// </summary>
    public async Task<string[]> GetAvailableCategoriesAsync()
    {
        await Task.CompletedTask; // Make async for future database integration
        return _defaultCategories;
    }

    /// <summary>
    /// Train classification model with provided training data
    /// </summary>
    public async Task TrainModelAsync(IEnumerable<DocumentTrainingData> trainingData)
    {
        try
        {
            _logger.LogInformation("🚀 Starting model training with {Count} samples", trainingData.Count());

            // Convert training data to ML.NET format
            var mlTrainingData = trainingData.Select(d => new DocumentData
            {
                Text = PreprocessText(d.Text),
                Category = d.Category
            }).ToList();

            // Create data view
            var dataView = _mlContext.Data.LoadFromEnumerable(mlTrainingData);

            // Define training pipeline
            var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("Label", "Category")
                .Append(_mlContext.Transforms.Text.FeaturizeText("Features", "Text"))
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "Features"))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            // Train model
            _logger.LogInformation("🔄 Training classification model...");
            _trainedModel = pipeline.Fit(dataView);

            // Save model
            await SaveModelAsync();

            _logger.LogInformation("✅ Model training completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Model training failed");
            throw;
        }
    }

    /// <summary>
    /// Create a default classification model with basic training data
    /// </summary>
    private async Task CreateDefaultModelAsync()
    {
        try
        {
            _logger.LogInformation("🔧 Creating default classification model...");

            var defaultTrainingData = GenerateDefaultTrainingData();
            await TrainModelAsync(defaultTrainingData);

            _logger.LogInformation("✅ Default classification model created");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create default model");
        }
    }

    /// <summary>
    /// Generate basic training data for default model
    /// </summary>
    private List<DocumentTrainingData> GenerateDefaultTrainingData()
    {
        return new List<DocumentTrainingData>
        {
            // Business documents
            new() { Text = "quarterly report financial results revenue profit business performance", Category = "Business" },
            new() { Text = "meeting agenda business strategy marketing plan project management", Category = "Business" },
            new() { Text = "company policy employee handbook business operations procedures", Category = "Business" },
            
            // Legal documents  
            new() { Text = "contract agreement terms conditions legal obligations parties", Category = "Legal" },
            new() { Text = "court case lawsuit legal proceedings attorney lawyer evidence", Category = "Legal" },
            new() { Text = "legal notice compliance regulation law statute requirements", Category = "Legal" },
            
            // Technical documents
            new() { Text = "software development programming code technical specifications API", Category = "Technical" },
            new() { Text = "system architecture database design technical documentation", Category = "Technical" },
            new() { Text = "technical manual installation guide configuration settings", Category = "Technical" },
            
            // Financial documents
            new() { Text = "financial statement balance sheet income statement cash flow", Category = "Financial" },
            new() { Text = "budget forecast investment portfolio financial planning", Category = "Financial" },
            new() { Text = "tax return accounting records financial audit expenses", Category = "Financial" },
            
            // Medical documents
            new() { Text = "patient medical record diagnosis treatment medication health", Category = "Medical" },
            new() { Text = "medical research clinical study healthcare patient care", Category = "Medical" },
            new() { Text = "prescription medication dosage medical history symptoms", Category = "Medical" },
            
            // Educational documents
            new() { Text = "course curriculum educational program academic research study", Category = "Educational" },
            new() { Text = "student assignment homework exam test educational material", Category = "Educational" },
            new() { Text = "research paper academic publication educational methodology", Category = "Educational" },
            
            // Personal documents
            new() { Text = "personal letter diary journal family photos memories", Category = "Personal" },
            new() { Text = "resume cv personal profile career experience education", Category = "Personal" },
            new() { Text = "personal notes thoughts ideas personal development", Category = "Personal" },
            
            // Government documents
            new() { Text = "government policy public administration official document regulation", Category = "Government" },
            new() { Text = "tax document official form government application permit", Category = "Government" },
            new() { Text = "public record government report official correspondence", Category = "Government" },
            
            // Marketing documents
            new() { Text = "marketing campaign advertisement promotional material brand", Category = "Marketing" },
            new() { Text = "marketing strategy social media promotion customer engagement", Category = "Marketing" },
            new() { Text = "product brochure marketing content sales material", Category = "Marketing" },
            
            // Other/General
            new() { Text = "miscellaneous document general information various topics", Category = "Other" },
            new() { Text = "random text content mixed topics general purpose", Category = "Other" }
        };
    }

    /// <summary>
    /// Preprocess text for better classification accuracy
    /// </summary>
    private static string PreprocessText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Convert to lowercase
        var processed = text.ToLowerInvariant();

        // Remove extra whitespace
        processed = System.Text.RegularExpressions.Regex.Replace(processed, @"\s+", " ");

        // Remove special characters but keep spaces
        processed = System.Text.RegularExpressions.Regex.Replace(processed, @"[^\w\s]", " ");

        // Trim
        processed = processed.Trim();

        // Limit length for performance
        if (processed.Length > 5000)
        {
            processed = processed.Substring(0, 5000);
        }

        return processed;
    }

    /// <summary>
    /// Process ML.NET prediction result
    /// </summary>
    private DocumentClassification ProcessPredictionResult(DocumentPrediction prediction, ClassificationOptions options)
    {
        var result = new DocumentClassification
        {
            Success = true,
            PrimaryCategory = prediction.PredictedCategory,
            PrimaryConfidence = prediction.Score.Max(),
            ClassificationModel = "ML.NET SdcaMaximumEntropy",
            ClassifiedAt = DateTime.UtcNow
        };

        // Add alternative categories if confidence meets minimum threshold
        if (result.PrimaryConfidence >= options.MinimumConfidence)
        {
            var alternatives = new List<CategoryPrediction>();
            
            for (int i = 0; i < prediction.Score.Length && alternatives.Count < options.MaxAlternatives; i++)
            {
                var category = _defaultCategories[i];
                var confidence = prediction.Score[i];
                
                if (category != result.PrimaryCategory && confidence >= options.MinimumConfidence * 0.5)
                {
                    alternatives.Add(new CategoryPrediction
                    {
                        Category = category,
                        Confidence = confidence
                    });
                }
            }

            result.AlternativeCategories = alternatives.OrderByDescending(c => c.Confidence).ToArray();
        }

        // Add classification features for analysis
        result.Features["TextLength"] = prediction.Score.Length;
        result.Features["MaxConfidence"] = prediction.Score.Max();
        result.Features["MinConfidence"] = prediction.Score.Min();
        result.Features["ConfidenceVariance"] = CalculateVariance(prediction.Score);

        return result;
    }

    /// <summary>
    /// Calculate variance of confidence scores
    /// </summary>
    private static double CalculateVariance(float[] scores)
    {
        var mean = scores.Average();
        var variance = scores.Select(s => Math.Pow(s - mean, 2)).Average();
        return variance;
    }

    /// <summary>
    /// Load existing model if available
    /// </summary>
    private void LoadModelIfExists()
    {
        try
        {
            if (File.Exists(_modelPath))
            {
                _logger.LogInformation("📁 Loading existing classification model from: {ModelPath}", _modelPath);
                _trainedModel = _mlContext.Model.Load(_modelPath, out _);
                _logger.LogInformation("✅ Classification model loaded successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to load existing model, will create new one");
        }
    }

    /// <summary>
    /// Save trained model to disk
    /// </summary>
    private async Task SaveModelAsync()
    {
        try
        {
            if (_trainedModel == null) return;

            // Ensure model directory exists
            var modelDir = Path.GetDirectoryName(_modelPath);
            if (!string.IsNullOrEmpty(modelDir) && !Directory.Exists(modelDir))
            {
                Directory.CreateDirectory(modelDir);
            }

            await Task.Run(() =>
            {
                _mlContext.Model.Save(_trainedModel, null, _modelPath);
            });

            _logger.LogInformation("💾 Classification model saved to: {ModelPath}", _modelPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to save classification model");
        }
    }
}

/// <summary>
/// ML.NET data model for document classification
/// </summary>
public class DocumentData
{
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// ML.NET prediction result model
/// </summary>
public class DocumentPrediction
{
    [ColumnName("PredictedLabel")]
    public string PredictedCategory { get; set; } = string.Empty;

    [ColumnName("Score")]
    public float[] Score { get; set; } = Array.Empty<float>();
}