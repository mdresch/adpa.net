using ADPA.Services;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace ADPA.Services.Processors;

/// <summary>
/// Word Document (.docx) processor using OpenXML SDK
/// Extracts text content, formatting, and document metadata
/// </summary>
public class WordDocumentProcessor : IDocumentFormatProcessor
{
    private readonly ILogger _logger;

    public WordDocumentProcessor(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extract all text content from Word document
    /// </summary>
    public async Task<string> ExtractTextAsync(string filePath)
    {
        await Task.CompletedTask; // OpenXML operations are synchronous
        
        try
        {
            using var document = WordprocessingDocument.Open(filePath, false);
            var body = document.MainDocumentPart?.Document?.Body;
            
            if (body == null)
            {
                _logger.LogWarning("📄 Word document {FilePath} has no body content", filePath);
                return string.Empty;
            }

            var textBuilder = new StringBuilder();
            
            // Extract text from all text-containing elements
            ExtractTextFromElement(body, textBuilder);
            
            var extractedText = textBuilder.ToString();
            
            _logger.LogDebug("📄 Extracted {CharacterCount} characters from Word document {FilePath}", 
                extractedText.Length, filePath);
            
            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to extract text from Word document {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to extract text from Word document: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Extract metadata from Word document
    /// </summary>
    public async Task<DocumentMetadata> ExtractMetadataAsync(string filePath)
    {
        await Task.CompletedTask; // OpenXML operations are synchronous
        
        try
        {
            using var document = WordprocessingDocument.Open(filePath, false);
            var metadata = new DocumentMetadata();

            // Extract core properties
            var coreProps = document.PackageProperties;
            if (coreProps != null)
            {
                metadata.Title = coreProps.Title;
                metadata.Author = coreProps.Creator;
                metadata.Subject = coreProps.Subject;
                metadata.CreatedDate = coreProps.Created;
                metadata.ModifiedDate = coreProps.Modified;
            }

            // Extract extended properties
            var extProps = document.ExtendedFilePropertiesPart?.Properties;
            if (extProps != null)
            {
                if (extProps.Application?.Text != null)
                    metadata.Creator = extProps.Application.Text;
                    
                if (extProps.Pages?.Text != null && int.TryParse(extProps.Pages.Text, out int pageCount))
                    metadata.PageCount = pageCount;
            }

            // Extract custom properties
            var customProps = document.CustomFilePropertiesPart?.Properties;
            if (customProps != null)
            {
                foreach (var prop in customProps.Elements())
                {
                    var name = prop.GetAttribute("name", "").Value;
                    var value = prop.InnerText;
                    
                    if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                    {
                        metadata.CustomProperties[name] = value;
                    }
                }
            }

            // Count pages if not available in properties
            if (metadata.PageCount == 0)
            {
                metadata.PageCount = CountPagesFromContent(document);
            }

            _logger.LogDebug("📄 Extracted metadata from Word document {FilePath}: Title='{Title}', Pages={PageCount}", 
                filePath, metadata.Title, metadata.PageCount);

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to extract metadata from Word document {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to extract metadata from Word document: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Recursively extract text from OpenXML elements
    /// </summary>
    private static void ExtractTextFromElement(OpenXmlElement element, StringBuilder textBuilder)
    {
        foreach (var child in element.Elements())
        {
            switch (child)
            {
                case Paragraph paragraph:
                    ExtractTextFromParagraph(paragraph, textBuilder);
                    textBuilder.AppendLine(); // Add line break after paragraph
                    break;
                    
                case Table table:
                    ExtractTextFromTable(table, textBuilder);
                    break;
                    
                case Run run:
                    ExtractTextFromRun(run, textBuilder);
                    break;
                    
                case Text text:
                    textBuilder.Append(text.Text);
                    break;
                    
                default:
                    // Recursively process other elements
                    ExtractTextFromElement(child, textBuilder);
                    break;
            }
        }
    }

    /// <summary>
    /// Extract text from paragraph with formatting awareness
    /// </summary>
    private static void ExtractTextFromParagraph(Paragraph paragraph, StringBuilder textBuilder)
    {
        foreach (var run in paragraph.Elements<Run>())
        {
            ExtractTextFromRun(run, textBuilder);
        }
    }

    /// <summary>
    /// Extract text from run (text with consistent formatting)
    /// </summary>
    private static void ExtractTextFromRun(Run run, StringBuilder textBuilder)
    {
        foreach (var element in run.Elements())
        {
            switch (element)
            {
                case Text text:
                    textBuilder.Append(text.Text);
                    break;
                    
                case TabChar:
                    textBuilder.Append('\t');
                    break;
                    
                case Break breakElement:
                    if (breakElement.Type?.Value == BreakValues.Page)
                        textBuilder.AppendLine("\n[PAGE BREAK]\n");
                    else
                        textBuilder.AppendLine();
                    break;
            }
        }
    }

    /// <summary>
    /// Extract text from table with structure preservation
    /// </summary>
    private static void ExtractTextFromTable(Table table, StringBuilder textBuilder)
    {
        textBuilder.AppendLine("\n[TABLE START]");
        
        foreach (var row in table.Elements<TableRow>())
        {
            var cellTexts = new List<string>();
            
            foreach (var cell in row.Elements<TableCell>())
            {
                var cellBuilder = new StringBuilder();
                ExtractTextFromElement(cell, cellBuilder);
                cellTexts.Add(cellBuilder.ToString().Trim());
            }
            
            textBuilder.AppendLine(string.Join(" | ", cellTexts));
        }
        
        textBuilder.AppendLine("[TABLE END]\n");
    }

    /// <summary>
    /// Count pages from document content (approximate)
    /// </summary>
    private int CountPagesFromContent(WordprocessingDocument document)
    {
        try
        {
            var body = document.MainDocumentPart?.Document?.Body;
            if (body == null) return 1;

            // Count page breaks
            var pageBreaks = body.Descendants<Break>()
                .Where(b => b.Type?.Value == BreakValues.Page)
                .Count();

            // Count section breaks (new sections typically start new pages)
            var sectionBreaks = body.Descendants<SectionProperties>().Count();

            // Estimate based on content if no explicit breaks
            if (pageBreaks == 0 && sectionBreaks <= 1)
            {
                var textLength = body.InnerText.Length;
                // Rough estimate: 2500 characters per page
                return Math.Max(1, textLength / 2500);
            }

            return Math.Max(1, pageBreaks + sectionBreaks);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to count pages, defaulting to 1");
            return 1;
        }
    }
}