using ADPA.Services;
using System.Text;

namespace ADPA.Services.Processors;

/// <summary>
/// Plain text document processor
/// Handles .txt files with encoding detection and basic analysis
/// </summary>
public class TextDocumentProcessor : IDocumentFormatProcessor
{
    private readonly ILogger _logger;

    public TextDocumentProcessor(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extract text content from plain text file with encoding detection
    /// </summary>
    public async Task<string> ExtractTextAsync(string filePath)
    {
        try
        {
            // Detect encoding and read file
            var encoding = DetectEncoding(filePath);
            var content = await File.ReadAllTextAsync(filePath, encoding);
            
            _logger.LogDebug("📄 Extracted {CharacterCount} characters from text file {FilePath} using {Encoding} encoding", 
                content.Length, filePath, encoding.EncodingName);

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to extract text from file {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to extract text from text file: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Extract metadata from text file
    /// </summary>
    public async Task<DocumentMetadata> ExtractMetadataAsync(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            var content = await ExtractTextAsync(filePath);

            var metadata = new DocumentMetadata
            {
                CreatedDate = fileInfo.CreationTime,
                ModifiedDate = fileInfo.LastWriteTime,
                PageCount = EstimatePageCount(content)
            };

            // Analyze content for additional metadata
            AnalyzeTextContent(content, metadata);

            _logger.LogDebug("📄 Extracted metadata from text file {FilePath}: Pages={PageCount}, Lines={LineCount}", 
                filePath, metadata.PageCount, metadata.CustomProperties.GetValueOrDefault("LineCount", 0));

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to extract metadata from text file {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to extract metadata from text file: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Detect file encoding using BOM and heuristics
    /// </summary>
    private Encoding DetectEncoding(string filePath)
    {
        try
        {
            var buffer = new byte[4];
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            fileStream.Read(buffer, 0, 4);

            // Check for BOM (Byte Order Mark)
            if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
                return Encoding.UTF8;
            if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                return Encoding.Unicode; // UTF-16 LE
            if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                return Encoding.BigEndianUnicode; // UTF-16 BE
            if (buffer[0] == 0xFF && buffer[1] == 0xFE && buffer[2] == 0x00 && buffer[3] == 0x00)
                return Encoding.UTF32; // UTF-32 LE

            // Default to UTF-8 for modern files, fallback to system default
            return Encoding.UTF8;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to detect encoding for {FilePath}, using UTF-8", filePath);
            return Encoding.UTF8;
        }
    }

    /// <summary>
    /// Analyze text content for additional metadata
    /// </summary>
    private void AnalyzeTextContent(string content, DocumentMetadata metadata)
    {
        if (string.IsNullOrEmpty(content))
            return;

        var lines = content.Split('\n');
        metadata.CustomProperties["LineCount"] = lines.Length;
        metadata.CustomProperties["CharacterCount"] = content.Length;
        metadata.CustomProperties["WordCount"] = CountWords(content);

        // Try to extract title from first non-empty line
        var firstLine = lines.FirstOrDefault(line => !string.IsNullOrWhiteSpace(line))?.Trim();
        if (!string.IsNullOrEmpty(firstLine) && firstLine.Length < 100)
        {
            metadata.Title = firstLine;
        }

        // Analyze content type
        var contentAnalysis = AnalyzeContentStructure(content);
        foreach (var kvp in contentAnalysis)
        {
            metadata.CustomProperties[kvp.Key] = kvp.Value;
        }
    }

    /// <summary>
    /// Analyze content structure to determine document type
    /// </summary>
    private Dictionary<string, object> AnalyzeContentStructure(string content)
    {
        var analysis = new Dictionary<string, object>();

        // Check for structured content patterns
        var lines = content.Split('\n');
        
        // Count empty lines (might indicate paragraphs)
        var emptyLines = lines.Count(line => string.IsNullOrWhiteSpace(line));
        analysis["EmptyLineCount"] = emptyLines;

        // Check for list-like content
        var bulletPoints = lines.Count(line => 
            line.TrimStart().StartsWith("- ") || 
            line.TrimStart().StartsWith("* ") ||
            line.TrimStart().StartsWith("• "));
        analysis["BulletPointCount"] = bulletPoints;

        // Check for numbered lists
        var numberedItems = lines.Count(line => 
            System.Text.RegularExpressions.Regex.IsMatch(line.TrimStart(), @"^\d+\.\s"));
        analysis["NumberedItemCount"] = numberedItems;

        // Check for code-like content
        var indentedLines = lines.Count(line => line.StartsWith("    ") || line.StartsWith("\t"));
        analysis["IndentedLineCount"] = indentedLines;

        // Determine likely document type
        if (bulletPoints > lines.Length * 0.1 || numberedItems > lines.Length * 0.1)
            analysis["LikelyType"] = "List/Outline";
        else if (indentedLines > lines.Length * 0.2)
            analysis["LikelyType"] = "Code/Technical";
        else if (emptyLines > lines.Length * 0.1)
            analysis["LikelyType"] = "Prose/Article";
        else
            analysis["LikelyType"] = "Plain Text";

        return analysis;
    }

    /// <summary>
    /// Count words in text content
    /// </summary>
    private int CountWords(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return 0;

        return content.Split(new char[] { ' ', '\t', '\n', '\r', '.', ',', ';', '!', '?' }, 
                           StringSplitOptions.RemoveEmptyEntries).Length;
    }

    /// <summary>
    /// Estimate page count based on character count
    /// </summary>
    private int EstimatePageCount(string content)
    {
        if (string.IsNullOrEmpty(content))
            return 1;

        // Estimate ~2000 characters per page for plain text
        return Math.Max(1, content.Length / 2000);
    }
}

/// <summary>
/// CSV document processor with structure analysis
/// Handles comma-separated value files with column detection
/// </summary>
public class CsvDocumentProcessor : IDocumentFormatProcessor
{
    private readonly ILogger _logger;

    public CsvDocumentProcessor(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extract text content from CSV file with structure preservation
    /// </summary>
    public async Task<string> ExtractTextAsync(string filePath)
    {
        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var textBuilder = new StringBuilder();

            textBuilder.AppendLine("[CSV DATA]");
            
            for (int i = 0; i < lines.Length && i < 1000; i++) // Limit to first 1000 rows
            {
                var columns = ParseCsvLine(lines[i]);
                textBuilder.AppendLine($"Row {i + 1}: {string.Join(" | ", columns)}");
            }

            if (lines.Length > 1000)
            {
                textBuilder.AppendLine($"... and {lines.Length - 1000} more rows");
            }

            textBuilder.AppendLine("[END CSV DATA]");

            var extractedText = textBuilder.ToString();
            _logger.LogDebug("📄 Extracted CSV structure from {FilePath}: {RowCount} rows", 
                filePath, lines.Length);

            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to extract text from CSV {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to extract text from CSV file: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Extract metadata from CSV file including column analysis
    /// </summary>
    public async Task<DocumentMetadata> ExtractMetadataAsync(string filePath)
    {
        try
        {
            var fileInfo = new FileInfo(filePath);
            var lines = await File.ReadAllLinesAsync(filePath);

            var metadata = new DocumentMetadata
            {
                CreatedDate = fileInfo.CreationTime,
                ModifiedDate = fileInfo.LastWriteTime,
                PageCount = Math.Max(1, lines.Length / 50) // Estimate pages based on rows
            };

            if (lines.Length > 0)
            {
                // Analyze CSV structure
                var firstRow = ParseCsvLine(lines[0]);
                metadata.CustomProperties["ColumnCount"] = firstRow.Count;
                metadata.CustomProperties["RowCount"] = lines.Length;
                
                // Try to detect if first row is header
                var hasHeader = DetectHeader(lines);
                metadata.CustomProperties["HasHeader"] = hasHeader;

                if (hasHeader && firstRow.Count > 0)
                {
                    metadata.CustomProperties["ColumnNames"] = firstRow;
                    metadata.Title = $"CSV with columns: {string.Join(", ", firstRow.Take(5))}";
                }
                else
                {
                    metadata.Title = $"CSV Data ({lines.Length} rows, {firstRow.Count} columns)";
                }

                // Sample data analysis
                if (lines.Length > 1)
                {
                    var sampleRow = hasHeader && lines.Length > 1 ? lines[1] : lines[0];
                    var sampleColumns = ParseCsvLine(sampleRow);
                    metadata.CustomProperties["SampleData"] = sampleColumns.Take(3).ToArray();
                }
            }

            _logger.LogDebug("📄 Extracted CSV metadata from {FilePath}: {RowCount} rows, {ColumnCount} columns", 
                filePath, metadata.CustomProperties.GetValueOrDefault("RowCount", 0), 
                metadata.CustomProperties.GetValueOrDefault("ColumnCount", 0));

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to extract metadata from CSV {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to extract metadata from CSV file: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parse a CSV line handling quoted fields and commas
    /// </summary>
    private List<string> ParseCsvLine(string line)
    {
        var columns = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                columns.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        columns.Add(current.ToString().Trim());
        return columns;
    }

    /// <summary>
    /// Detect if first row is likely a header row
    /// </summary>
    private bool DetectHeader(string[] lines)
    {
        if (lines.Length < 2)
            return false;

        var firstRow = ParseCsvLine(lines[0]);
        var secondRow = ParseCsvLine(lines[1]);

        if (firstRow.Count != secondRow.Count)
            return false;

        // Check if first row contains more text and second row contains more numbers
        int firstRowNumbers = firstRow.Count(col => double.TryParse(col, out _));
        int secondRowNumbers = secondRow.Count(col => double.TryParse(col, out _));

        // If first row has fewer numbers than second row, it's likely a header
        return firstRowNumbers < secondRowNumbers;
    }
}