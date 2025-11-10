using ADPA.Services;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace ADPA.Services.Processors;

/// <summary>
/// PDF document processor using iText7 library
/// Extracts text content and metadata from PDF files
/// </summary>
public class PdfDocumentProcessor : IDocumentFormatProcessor
{
    private readonly ILogger _logger;

    public PdfDocumentProcessor(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extract all text content from PDF document
    /// </summary>
    public async Task<string> ExtractTextAsync(string filePath)
    {
        await Task.CompletedTask; // iText operations are synchronous
        
        try
        {
            var textBuilder = new StringBuilder();
            
            using var pdfReader = new PdfReader(filePath);
            using var pdfDocument = new PdfDocument(pdfReader);
            
            int pageCount = pdfDocument.GetNumberOfPages();
            _logger.LogDebug("📄 Processing PDF with {PageCount} pages: {FilePath}", pageCount, filePath);

            for (int pageNum = 1; pageNum <= pageCount; pageNum++)
            {
                try
                {
                    var page = pdfDocument.GetPage(pageNum);
                    var strategy = new SimpleTextExtractionStrategy();
                    var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                    
                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        textBuilder.AppendLine($"[PAGE {pageNum}]");
                        textBuilder.AppendLine(pageText);
                        textBuilder.AppendLine();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Failed to extract text from page {PageNum} of PDF {FilePath}", pageNum, filePath);
                    textBuilder.AppendLine($"[PAGE {pageNum} - ERROR: {ex.Message}]");
                }
            }

            var extractedText = textBuilder.ToString();
            _logger.LogDebug("📄 Extracted {CharacterCount} characters from {PageCount} pages of PDF {FilePath}", 
                extractedText.Length, pageCount, filePath);

            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to extract text from PDF {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to extract text from PDF: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Extract metadata from PDF document
    /// </summary>
    public async Task<DocumentMetadata> ExtractMetadataAsync(string filePath)
    {
        await Task.CompletedTask; // iText operations are synchronous
        
        try
        {
            using var pdfReader = new PdfReader(filePath);
            using var pdfDocument = new PdfDocument(pdfReader);
            
            var metadata = new DocumentMetadata
            {
                PageCount = pdfDocument.GetNumberOfPages()
            };

            // Extract document info
            var docInfo = pdfDocument.GetDocumentInfo();
            if (docInfo != null)
            {
                metadata.Title = docInfo.GetTitle();
                metadata.Author = docInfo.GetAuthor();
                metadata.Subject = docInfo.GetSubject();
                metadata.Creator = docInfo.GetCreator();
                metadata.Producer = docInfo.GetProducer();

                // Skip date extraction for now - API compatibility issue
                _logger.LogInformation("Date extraction skipped due to iText7 API changes");

                // Add basic custom properties using available methods
                try
                {
                    // Add basic metadata to custom properties
                    if (!string.IsNullOrEmpty(metadata.Title))
                        metadata.CustomProperties["Title"] = metadata.Title;
                    if (!string.IsNullOrEmpty(metadata.Author))
                        metadata.CustomProperties["Author"] = metadata.Author;
                    if (!string.IsNullOrEmpty(metadata.Subject))
                        metadata.CustomProperties["Subject"] = metadata.Subject;
                    if (!string.IsNullOrEmpty(metadata.Creator))
                        metadata.CustomProperties["Creator"] = metadata.Creator;
                    if (!string.IsNullOrEmpty(metadata.Producer))
                        metadata.CustomProperties["Producer"] = metadata.Producer;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to extract PDF custom properties");
                }
            }

            // Extract additional PDF-specific metadata
            try
            {
                var catalog = pdfDocument.GetCatalog();
                if (catalog != null)
                {
                    // Extract PDF version
                    var version = pdfDocument.GetPdfVersion();
                    metadata.CustomProperties["PDFVersion"] = version.ToString();

                    // Check if PDF is encrypted
                    metadata.CustomProperties["IsEncrypted"] = pdfReader.IsEncrypted();

                    // Extract form information
                    var acroForm = catalog.GetPdfObject().GetAsDictionary(iText.Kernel.Pdf.PdfName.AcroForm);
                    if (acroForm != null)
                    {
                        metadata.CustomProperties["HasForms"] = true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to extract additional PDF metadata");
            }

            _logger.LogDebug("📄 Extracted metadata from PDF {FilePath}: Title='{Title}', Pages={PageCount}, Author='{Author}'", 
                filePath, metadata.Title, metadata.PageCount, metadata.Author);

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to extract metadata from PDF {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to extract metadata from PDF: {ex.Message}", ex);
        }
    }



    /// <summary>
    /// Check if property name is a standard PDF metadata property
    /// </summary>
    private bool IsStandardProperty(string propertyName)
    {
        var standardProperties = new[]
        {
            "Title", "Author", "Subject", "Creator", "Producer",
            "CreationDate", "ModDate", "Trapped", "Keywords"
        };

        return standardProperties.Contains(propertyName, StringComparer.OrdinalIgnoreCase);
    }
}