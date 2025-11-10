using ADPA.Services.Intelligence;
using NTextCat;

namespace ADPA.Services.Intelligence;

/// <summary>
/// Advanced text analysis service providing language detection, entity extraction, and sentiment analysis
/// </summary>
public class AdvancedTextAnalysisService : ITextAnalysisService
{
    private readonly ILogger<AdvancedTextAnalysisService> _logger;
    private readonly RankedLanguageIdentifier _languageIdentifier;

    // Common entity patterns for basic entity extraction
    private readonly Dictionary<string, string[]> _entityPatterns = new()
    {
        ["EMAIL"] = new[] { @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b" },
        ["PHONE"] = new[] { @"\b\d{3}[-.]?\d{3}[-.]?\d{4}\b", @"\b\(\d{3}\)\s?\d{3}[-.]?\d{4}\b" },
        ["URL"] = new[] { @"https?://[^\s]+", @"www\.[^\s]+" },
        ["DATE"] = new[] { @"\b\d{1,2}[/-]\d{1,2}[/-]\d{2,4}\b", @"\b\d{4}[/-]\d{1,2}[/-]\d{1,2}\b" },
        ["MONEY"] = new[] { @"\$\d+(?:,\d{3})*(?:\.\d{2})?", @"\b\d+(?:,\d{3})*(?:\.\d{2})?\s*(?:dollars?|USD|euros?|EUR)\b" },
        ["PERSON"] = new[] { @"\b[A-Z][a-z]+\s+[A-Z][a-z]+\b" }, // Basic pattern for names
        ["ORGANIZATION"] = new[] { @"\b[A-Z][a-z]+\s+(?:Inc|Corp|LLC|Ltd|Company|Corporation)\b" }
    };

    // Sentiment analysis keywords
    private readonly Dictionary<string, string[]> _sentimentKeywords = new()
    {
        ["POSITIVE"] = new[] { "good", "great", "excellent", "amazing", "wonderful", "fantastic", "love", "like", "happy", "satisfied", "pleased", "success", "win", "achievement", "perfect" },
        ["NEGATIVE"] = new[] { "bad", "terrible", "awful", "horrible", "hate", "dislike", "sad", "angry", "frustrated", "disappointed", "fail", "failure", "problem", "issue", "wrong" },
        ["NEUTRAL"] = new[] { "okay", "fine", "average", "normal", "standard", "typical", "usual", "regular", "moderate" }
    };

    public AdvancedTextAnalysisService(ILogger<AdvancedTextAnalysisService> logger)
    {
        _logger = logger;
        
        // Initialize language identifier
        var factory = new RankedLanguageIdentifierFactory();
        _languageIdentifier = factory.Load("Core14.profile.xml");
        
        _logger.LogInformation("✅ Advanced Text Analysis Service initialized");
    }

    /// <summary>
    /// Perform comprehensive text analysis
    /// </summary>
    public async Task<TextAnalysisResult> AnalyzeAsync(string text, TextAnalysisOptions? options = null)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("🔍 Starting comprehensive text analysis for text length: {TextLength}", text.Length);

            options ??= new TextAnalysisOptions();

            var result = new TextAnalysisResult { Success = true };

            // Perform language detection
            if (options.DetectLanguage)
            {
                result.LanguageDetection = await DetectLanguageAsync(text);
            }

            // Perform entity extraction
            if (options.ExtractEntities)
            {
                result.EntityExtraction = await ExtractEntitiesAsync(text);
            }

            // Perform sentiment analysis
            if (options.AnalyzeSentiment)
            {
                result.SentimentAnalysis = await AnalyzeSentimentAsync(text);
            }

            // Calculate text statistics
            if (options.CalculateStatistics)
            {
                result.Statistics = CalculateTextStatistics(text);
            }

            stopwatch.Stop();
            result.ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("✅ Text analysis completed in {ProcessingTime}ms", result.ProcessingTimeMs);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Text analysis failed");
            
            return new TextAnalysisResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <summary>
    /// Detect language of the text
    /// </summary>
    public async Task<LanguageDetectionResult> DetectLanguageAsync(string text)
    {
        try
        {
            await Task.CompletedTask; // Make async for consistency

            if (string.IsNullOrWhiteSpace(text))
            {
                return new LanguageDetectionResult
                {
                    DetectedLanguage = "Unknown",
                    LanguageCode = "unk",
                    Confidence = 0.0
                };
            }

            // Use NTextCat for language detection
            var languages = _languageIdentifier.Identify(text);
            var topLanguage = languages.FirstOrDefault();

            if (topLanguage != null)
            {
                // Get alternative predictions
                var alternatives = languages.Skip(1).Take(3).Select(lang => new LanguagePrediction
                {
                    Language = GetLanguageName(lang.Item1.Iso639_3),
                    LanguageCode = lang.Item1.Iso639_3,
                    Confidence = lang.Item2
                }).ToArray();

                return new LanguageDetectionResult
                {
                    DetectedLanguage = GetLanguageName(topLanguage.Item1.Iso639_3),
                    LanguageCode = topLanguage.Item1.Iso639_3,
                    Confidence = topLanguage.Item2,
                    AlternativeLanguages = alternatives
                };
            }

            return new LanguageDetectionResult
            {
                DetectedLanguage = "Unknown",
                LanguageCode = "unk",
                Confidence = 0.0
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Language detection failed, defaulting to English");
            return new LanguageDetectionResult
            {
                DetectedLanguage = "English",
                LanguageCode = "eng",
                Confidence = 0.5
            };
        }
    }

    /// <summary>
    /// Extract entities from text using pattern matching
    /// </summary>
    public async Task<EntityExtractionResult> ExtractEntitiesAsync(string text)
    {
        try
        {
            await Task.CompletedTask; // Make async for consistency

            var entities = new List<Entity>();
            var entityTypeCount = new Dictionary<string, int>();

            foreach (var entityType in _entityPatterns.Keys)
            {
                var patterns = _entityPatterns[entityType];
                var typeCount = 0;

                foreach (var pattern in patterns)
                {
                    var regex = new System.Text.RegularExpressions.Regex(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    var matches = regex.Matches(text);

                    foreach (System.Text.RegularExpressions.Match match in matches)
                    {
                        entities.Add(new Entity
                        {
                            Text = match.Value,
                            Type = entityType,
                            StartPosition = match.Index,
                            EndPosition = match.Index + match.Length,
                            Confidence = 0.8, // Pattern matching confidence
                            Properties = new Dictionary<string, object>
                            {
                                ["Pattern"] = pattern,
                                ["MatchLength"] = match.Length
                            }
                        });

                        typeCount++;
                    }
                }

                if (typeCount > 0)
                {
                    entityTypeCount[entityType] = typeCount;
                }
            }

            return new EntityExtractionResult
            {
                Entities = entities.OrderBy(e => e.StartPosition).ToArray(),
                EntityCount = entities.Count,
                EntityTypeCount = entityTypeCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Entity extraction failed");
            return new EntityExtractionResult
            {
                Entities = Array.Empty<Entity>(),
                EntityCount = 0,
                EntityTypeCount = new Dictionary<string, int>()
            };
        }
    }

    /// <summary>
    /// Analyze sentiment of text using keyword-based approach
    /// </summary>
    public async Task<SentimentAnalysisResult> AnalyzeSentimentAsync(string text)
    {
        try
        {
            await Task.CompletedTask; // Make async for consistency

            if (string.IsNullOrWhiteSpace(text))
            {
                return new SentimentAnalysisResult
                {
                    OverallSentiment = "Neutral",
                    ConfidenceScore = 0.0,
                    Scores = new SentimentScore { Neutral = 1.0, Positive = 0.0, Negative = 0.0 }
                };
            }

            var words = text.ToLowerInvariant()
                .Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);

            // Count sentiment words
            var positiveCount = words.Count(w => _sentimentKeywords["POSITIVE"].Contains(w));
            var negativeCount = words.Count(w => _sentimentKeywords["NEGATIVE"].Contains(w));
            var neutralCount = words.Count(w => _sentimentKeywords["NEUTRAL"].Contains(w));

            // Calculate scores
            var totalSentimentWords = positiveCount + negativeCount + neutralCount;
            
            if (totalSentimentWords == 0)
            {
                return new SentimentAnalysisResult
                {
                    OverallSentiment = "Neutral",
                    ConfidenceScore = 0.5,
                    Scores = new SentimentScore { Neutral = 0.5, Positive = 0.25, Negative = 0.25 }
                };
            }

            var positiveScore = (double)positiveCount / totalSentimentWords;
            var negativeScore = (double)negativeCount / totalSentimentWords;
            var neutralScore = (double)neutralCount / totalSentimentWords;

            // Normalize scores
            if (positiveScore + negativeScore + neutralScore == 0)
            {
                neutralScore = 1.0;
            }

            // Determine overall sentiment
            string overallSentiment;
            double confidence;

            if (positiveScore > negativeScore && positiveScore > neutralScore)
            {
                overallSentiment = "Positive";
                confidence = positiveScore;
            }
            else if (negativeScore > positiveScore && negativeScore > neutralScore)
            {
                overallSentiment = "Negative";
                confidence = negativeScore;
            }
            else
            {
                overallSentiment = "Neutral";
                confidence = neutralScore;
            }

            return new SentimentAnalysisResult
            {
                OverallSentiment = overallSentiment,
                ConfidenceScore = confidence,
                Scores = new SentimentScore
                {
                    Positive = positiveScore,
                    Negative = negativeScore,
                    Neutral = neutralScore
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Sentiment analysis failed");
            return new SentimentAnalysisResult
            {
                OverallSentiment = "Neutral",
                ConfidenceScore = 0.0,
                Scores = new SentimentScore { Neutral = 1.0, Positive = 0.0, Negative = 0.0 }
            };
        }
    }

    /// <summary>
    /// Calculate comprehensive text statistics
    /// </summary>
    private TextStatistics CalculateTextStatistics(string text)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new TextStatistics();
            }

            // Basic counts
            var characterCount = text.Length;
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var wordCount = words.Length;
            
            // Count sentences (basic approach)
            var sentences = text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var sentenceCount = sentences.Length;
            
            // Count paragraphs
            var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var paragraphCount = paragraphs.Length;

            // Calculate readability score (simplified Flesch Reading Ease)
            var avgWordsPerSentence = sentenceCount > 0 ? (double)wordCount / sentenceCount : 0;
            var avgSyllablesPerWord = CalculateAverageSyllables(words);
            var readabilityScore = 206.835 - (1.015 * avgWordsPerSentence) - (84.6 * avgSyllablesPerWord);
            readabilityScore = Math.Max(0, Math.Min(100, readabilityScore)); // Clamp between 0-100

            // Extract top keywords (simple frequency-based)
            var topKeywords = ExtractTopKeywords(words, 10);

            return new TextStatistics
            {
                CharacterCount = characterCount,
                WordCount = wordCount,
                SentenceCount = sentenceCount,
                ParagraphCount = paragraphCount,
                ReadabilityScore = readabilityScore,
                TopKeywords = topKeywords
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Text statistics calculation failed");
            return new TextStatistics();
        }
    }

    /// <summary>
    /// Calculate average syllables per word (simplified)
    /// </summary>
    private static double CalculateAverageSyllables(string[] words)
    {
        if (words.Length == 0) return 0;

        var totalSyllables = 0;
        foreach (var word in words)
        {
            totalSyllables += CountSyllables(word);
        }

        return (double)totalSyllables / words.Length;
    }

    /// <summary>
    /// Count syllables in a word (simplified algorithm)
    /// </summary>
    private static int CountSyllables(string word)
    {
        if (string.IsNullOrWhiteSpace(word)) return 0;

        word = word.ToLowerInvariant();
        var vowels = "aeiouy";
        var syllableCount = 0;
        var previousWasVowel = false;

        foreach (var c in word)
        {
            var isVowel = vowels.Contains(c);
            if (isVowel && !previousWasVowel)
            {
                syllableCount++;
            }
            previousWasVowel = isVowel;
        }

        // Handle silent 'e'
        if (word.EndsWith("e") && syllableCount > 1)
        {
            syllableCount--;
        }

        return Math.Max(1, syllableCount);
    }

    /// <summary>
    /// Extract top keywords by frequency
    /// </summary>
    private static string[] ExtractTopKeywords(string[] words, int topCount)
    {
        // Common stop words to exclude
        var stopWords = new HashSet<string>
        {
            "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by", "is", "are", "was", "were", "be", "been", "have", "has", "had", "do", "does", "did", "will", "would", "could", "should", "may", "might", "can", "this", "that", "these", "those", "i", "you", "he", "she", "it", "we", "they", "me", "him", "her", "us", "them"
        };

        return words
            .Where(w => w.Length > 2 && !stopWords.Contains(w.ToLowerInvariant()))
            .GroupBy(w => w.ToLowerInvariant())
            .OrderByDescending(g => g.Count())
            .Take(topCount)
            .Select(g => g.Key)
            .ToArray();
    }

    /// <summary>
    /// Get friendly language name from ISO code
    /// </summary>
    private static string GetLanguageName(string isoCode)
    {
        var languageMap = new Dictionary<string, string>
        {
            ["eng"] = "English",
            ["spa"] = "Spanish", 
            ["fra"] = "French",
            ["deu"] = "German",
            ["ita"] = "Italian",
            ["por"] = "Portuguese",
            ["rus"] = "Russian",
            ["chi"] = "Chinese",
            ["jpn"] = "Japanese",
            ["kor"] = "Korean",
            ["ara"] = "Arabic",
            ["hin"] = "Hindi",
            ["nld"] = "Dutch",
            ["swe"] = "Swedish",
            ["dan"] = "Danish",
            ["nor"] = "Norwegian"
        };

        return languageMap.TryGetValue(isoCode, out var name) ? name : isoCode.ToUpperInvariant();
    }
}