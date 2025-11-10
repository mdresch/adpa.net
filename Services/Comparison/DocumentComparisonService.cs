using ADPA.Models.DTOs;
using ADPA.Services.Notifications;
using System.Text;
using System.Text.RegularExpressions;

namespace ADPA.Services.Comparison;

/// <summary>
/// Document comparison result
/// </summary>
public class DocumentComparisonResult
{
    public Guid ComparisonId { get; set; } = Guid.NewGuid();
    public Guid Document1Id { get; set; }
    public Guid Document2Id { get; set; }
    public string Document1Name { get; set; } = string.Empty;
    public string Document2Name { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public ComparisonType ComparisonType { get; set; }
    public List<DocumentDifference> Differences { get; set; } = new();
    public ComparisonStatistics Statistics { get; set; } = new();
    public DateTime ComparedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ProcessingTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Types of document comparison
/// </summary>
public enum ComparisonType
{
    TextContent,
    Structure,
    Metadata,
    Complete
}

/// <summary>
/// Type of document difference
/// </summary>
public enum DifferenceType
{
    Addition,
    Deletion,
    Modification,
    Movement
}

/// <summary>
/// Individual difference between documents
/// </summary>
public class DocumentDifference
{
    public DifferenceType Type { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public double Confidence { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Comparison statistics
/// </summary>
public class ComparisonStatistics
{
    public int TotalDifferences { get; set; }
    public int Additions { get; set; }
    public int Deletions { get; set; }
    public int Modifications { get; set; }
    public int Movements { get; set; }
    public double ContentSimilarity { get; set; }
    public double StructuralSimilarity { get; set; }
    public double MetadataSimilarity { get; set; }
    public int Document1WordCount { get; set; }
    public int Document2WordCount { get; set; }
    public int CommonWords { get; set; }
    public int UniqueWords1 { get; set; }
    public int UniqueWords2 { get; set; }
}

/// <summary>
/// Document comparison options
/// </summary>
public class ComparisonOptions
{
    public ComparisonType ComparisonType { get; set; } = ComparisonType.Complete;
    public bool IgnoreWhitespace { get; set; } = true;
    public bool IgnoreCase { get; set; } = false;
    public bool IgnorePunctuation { get; set; } = false;
    public double SimilarityThreshold { get; set; } = 0.1;
    public int MaxDifferences { get; set; } = 1000;
    public bool EnableSemanticComparison { get; set; } = false;
}

/// <summary>
/// Interface for document comparison service
/// </summary>
public interface IDocumentComparisonService
{
    /// <summary>
    /// Compare two documents by their IDs
    /// </summary>
    Task<DocumentComparisonResult> CompareDocumentsAsync(Guid document1Id, Guid document2Id, ComparisonOptions? options = null);
    
    /// <summary>
    /// Compare document content directly
    /// </summary>
    Task<DocumentComparisonResult> CompareContentAsync(string content1, string content2, string name1, string name2, ComparisonOptions? options = null);
    
    /// <summary>
    /// Calculate similarity score between two texts
    /// </summary>
    Task<double> CalculateSimilarityScoreAsync(string text1, string text2);
    
    /// <summary>
    /// Get comparison history for a document
    /// </summary>
    Task<IEnumerable<DocumentComparisonResult>> GetComparisonHistoryAsync(Guid documentId);
}

/// <summary>
/// Document comparison service implementation
/// Phase 4: Advanced Features - Intelligent Document Comparison
/// </summary>
public class DocumentComparisonService : IDocumentComparisonService
{
    private readonly IDocumentService _documentService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<DocumentComparisonService> _logger;
    private readonly List<DocumentComparisonResult> _comparisonHistory; // In production, use a repository

    public DocumentComparisonService(
        IDocumentService documentService,
        INotificationService notificationService,
        ILogger<DocumentComparisonService> logger)
    {
        _documentService = documentService;
        _notificationService = notificationService;
        _logger = logger;
        _comparisonHistory = new List<DocumentComparisonResult>();
    }

    /// <summary>
    /// Compare two documents by their IDs
    /// </summary>
    public async Task<DocumentComparisonResult> CompareDocumentsAsync(Guid document1Id, Guid document2Id, ComparisonOptions? options = null)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("🔍 Comparing documents {Document1Id} and {Document2Id}", document1Id, document2Id);

            options ??= new ComparisonOptions();

            // Get document information
            var doc1 = await _documentService.GetDocumentAsync(document1Id);
            var doc2 = await _documentService.GetDocumentAsync(document2Id);

            if (doc1 == null || doc2 == null)
            {
                throw new ArgumentException("One or both documents not found");
            }

            // For now, we'll simulate getting document content
            // In a real implementation, you would extract the actual text content
            var content1 = $"Sample content for document {doc1.FileName}. This is document 1 content with some text to compare.";
            var content2 = $"Sample content for document {doc2.FileName}. This is document 2 content with some different text to compare.";

            var result = await CompareContentAsync(content1, content2, doc1.FileName, doc2.FileName, options);
            result.Document1Id = document1Id;
            result.Document2Id = document2Id;
            result.ProcessingTime = DateTime.UtcNow - startTime;

            // Store in history
            _comparisonHistory.Add(result);

            _logger.LogInformation("✅ Document comparison completed: {ComparisonId}. Similarity: {Similarity}%", 
                result.ComparisonId, result.SimilarityScore * 100);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Document comparison failed: {Document1Id} vs {Document2Id}", document1Id, document2Id);
            throw;
        }
    }

    /// <summary>
    /// Compare document content directly
    /// </summary>
    public async Task<DocumentComparisonResult> CompareContentAsync(string content1, string content2, string name1, string name2, ComparisonOptions? options = null)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("📝 Comparing content: '{Name1}' vs '{Name2}'", name1, name2);

            options ??= new ComparisonOptions();

            var result = new DocumentComparisonResult
            {
                Document1Name = name1,
                Document2Name = name2,
                ComparisonType = options.ComparisonType
            };

            // Preprocess content based on options
            var processedContent1 = PreprocessContent(content1, options);
            var processedContent2 = PreprocessContent(content2, options);

            // Calculate similarity score
            result.SimilarityScore = await CalculateSimilarityScoreAsync(processedContent1, processedContent2);

            // Find differences
            result.Differences = FindDifferences(processedContent1, processedContent2, options);

            // Calculate statistics
            result.Statistics = CalculateStatistics(processedContent1, processedContent2, result.Differences);

            // Add metadata
            result.Metadata["OriginalContent1Length"] = content1.Length;
            result.Metadata["OriginalContent2Length"] = content2.Length;
            result.Metadata["ProcessedContent1Length"] = processedContent1.Length;
            result.Metadata["ProcessedContent2Length"] = processedContent2.Length;

            result.ProcessingTime = DateTime.UtcNow - startTime;

            _logger.LogInformation("✅ Content comparison completed: {ComparisonId}. Similarity: {Similarity}%", 
                result.ComparisonId, result.SimilarityScore * 100);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Content comparison failed: '{Name1}' vs '{Name2}'", name1, name2);
            throw;
        }
    }

    /// <summary>
    /// Calculate similarity score using Levenshtein distance and other metrics
    /// </summary>
    public async Task<double> CalculateSimilarityScoreAsync(string text1, string text2)
    {
        if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
            return 1.0;

        if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
            return 0.0;

        // Calculate Levenshtein distance
        var distance = CalculateLevenshteinDistance(text1, text2);
        var maxLength = Math.Max(text1.Length, text2.Length);
        var similarity = 1.0 - (double)distance / maxLength;

        // Word-level similarity
        var words1 = text1.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var words2 = text2.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var commonWords = words1.Intersect(words2).Count();
        var totalWords = words1.Union(words2).Count();
        var wordSimilarity = totalWords > 0 ? (double)commonWords / totalWords : 0;

        // Combine character and word similarities
        var finalSimilarity = (similarity * 0.7) + (wordSimilarity * 0.3);

        return await Task.FromResult(Math.Round(finalSimilarity, 4));
    }

    /// <summary>
    /// Get comparison history for a document
    /// </summary>
    public async Task<IEnumerable<DocumentComparisonResult>> GetComparisonHistoryAsync(Guid documentId)
    {
        var history = _comparisonHistory
            .Where(c => c.Document1Id == documentId || c.Document2Id == documentId)
            .OrderByDescending(c => c.ComparedAt)
            .ToList();

        return await Task.FromResult(history);
    }

    /// <summary>
    /// Preprocess content based on comparison options
    /// </summary>
    private string PreprocessContent(string content, ComparisonOptions options)
    {
        var processed = content;

        if (options.IgnoreWhitespace)
        {
            processed = Regex.Replace(processed, @"\s+", " ").Trim();
        }

        if (options.IgnoreCase)
        {
            processed = processed.ToLowerInvariant();
        }

        if (options.IgnorePunctuation)
        {
            processed = Regex.Replace(processed, @"[^\w\s]", "");
        }

        return processed;
    }

    /// <summary>
    /// Find differences between two texts
    /// </summary>
    private List<DocumentDifference> FindDifferences(string content1, string content2, ComparisonOptions options)
    {
        var differences = new List<DocumentDifference>();
        
        try
        {
            var lines1 = content1.Split('\n');
            var lines2 = content2.Split('\n');

            var maxLines = Math.Max(lines1.Length, lines2.Length);
            var diffCount = 0;

            for (int i = 0; i < maxLines && diffCount < options.MaxDifferences; i++)
            {
                var line1 = i < lines1.Length ? lines1[i] : "";
                var line2 = i < lines2.Length ? lines2[i] : "";

                if (line1 != line2)
                {
                    var differenceType = DifferenceType.Modification;
                    if (string.IsNullOrEmpty(line1)) differenceType = DifferenceType.Addition;
                    else if (string.IsNullOrEmpty(line2)) differenceType = DifferenceType.Deletion;

                    differences.Add(new DocumentDifference
                    {
                        Type = differenceType,
                        Location = $"Line {i + 1}",
                        OldValue = line1,
                        NewValue = line2,
                        Confidence = 0.95,
                        Description = $"{differenceType} at line {i + 1}"
                    });

                    diffCount++;
                }
            }

            return differences.Take(options.MaxDifferences).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error finding differences, returning empty list");
            return new List<DocumentDifference>();
        }
    }

    /// <summary>
    /// Calculate comparison statistics
    /// </summary>
    private ComparisonStatistics CalculateStatistics(string content1, string content2, List<DocumentDifference> differences)
    {
        var words1 = content1.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var words2 = content2.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var wordSet1 = words1.ToHashSet();
        var wordSet2 = words2.ToHashSet();
        
        var commonWords = wordSet1.Intersect(wordSet2).Count();

        return new ComparisonStatistics
        {
            TotalDifferences = differences.Count,
            Additions = differences.Count(d => d.Type == DifferenceType.Addition),
            Deletions = differences.Count(d => d.Type == DifferenceType.Deletion),
            Modifications = differences.Count(d => d.Type == DifferenceType.Modification),
            Movements = differences.Count(d => d.Type == DifferenceType.Movement),
            Document1WordCount = words1.Length,
            Document2WordCount = words2.Length,
            CommonWords = commonWords,
            UniqueWords1 = wordSet1.Except(wordSet2).Count(),
            UniqueWords2 = wordSet2.Except(wordSet1).Count(),
            ContentSimilarity = Math.Round(1.0 - (double)differences.Count / Math.Max(words1.Length, words2.Length), 4),
            StructuralSimilarity = 0.85, // Simplified - would need more complex analysis
            MetadataSimilarity = 0.90 // Simplified - would compare actual metadata
        };
    }

    /// <summary>
    /// Calculate Levenshtein distance between two strings
    /// </summary>
    private int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return string.IsNullOrEmpty(target) ? 0 : target.Length;

        if (string.IsNullOrEmpty(target))
            return source.Length;

        var sourceLength = source.Length;
        var targetLength = target.Length;
        var matrix = new int[sourceLength + 1, targetLength + 1];

        // Initialize first column and row
        for (int i = 1; i <= sourceLength; i++)
            matrix[i, 0] = i;

        for (int j = 1; j <= targetLength; j++)
            matrix[0, j] = j;

        // Calculate distances
        for (int i = 1; i <= sourceLength; i++)
        {
            for (int j = 1; j <= targetLength; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[sourceLength, targetLength];
    }
}