//using Azure;
//using Azure.AI.OpenAI;
//using Microsoft.EntityFrameworkCore;
//using OpenAI.Chat;
//using System.Text;
//using YesterdayNews.Data;
//using YesterdayNews.Models.Db;
//using YesterdayNews.Models.ViewModels;
//using YesterdayNews.Services.IServices;

//namespace YesterdayNews.Services
//{
//    public class NewsChatService : INewsChatService
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IConfiguration _configuration;
//        private readonly ILogger<NewsChatService> _logger;
//        private readonly AzureOpenAIClient _azureClient;
//        private readonly ChatClient _chatClient;

//        public NewsChatService(
//            ApplicationDbContext context,
//            IConfiguration configuration,
//            ILogger<NewsChatService> logger)
//        {
//            _context = context;
//            _configuration = configuration;
//            _logger = logger;

//            // Initialize Azure OpenAI client
//            var endpoint = _configuration["AzureOpenAI:Endpoint"] ?? "https://foundryservicegr12.openai.azure.com/";
//            var apiKey = _configuration["AzureOpenAI:Key"];

//            if (string.IsNullOrEmpty(apiKey))
//            {
//                throw new InvalidOperationException("Azure OpenAI API key is not configured. Please set AzureOpenAI:Key in your configuration.");
//            }

//            var credential = new AzureKeyCredential(apiKey);
//            _azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);

//            var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o-2-gr12";
//            _chatClient = _azureClient.GetChatClient(deploymentName);
//        }

//        public async Task<string> GetNewsResponseAsync(string userMessage, List<ChatMessageViewModel> conversationHistory)
//        {
//            try
//            {
//                // Search for relevant articles
//                var relevantArticles = await SearchRelevantArticlesAsync(userMessage);

//                // Build context from articles
//                var articlesContext = BuildArticlesContext(relevantArticles);

//                // Build conversation history for OpenAI
//                var messages = BuildChatMessages(userMessage, conversationHistory, articlesContext);

//                // Create chat completion options
//                var options = new ChatCompletionOptions
//                {
//                    Temperature = 0.7f,
//                    MaxOutputTokenCount = 1000,
//                    TopP = 0.95f,
//                    FrequencyPenalty = 0f,
//                    PresencePenalty = 0f
//                };

//                // Get response from Azure OpenAI
//                var completionResult = await _chatClient.CompleteChatAsync(messages, options);

//                if ( completionResult.Value != null)
//                {
//                    var completion = completionResult.Value;

//                    if (completion.Content != null && completion.Content.Count > 0)
//                    {
//                        var response = completion.Content[0].Text;

//                        // Add article references if relevant articles were found
//                        if (relevantArticles.Any())
//                        {
//                            response = AddArticleReferences(response, relevantArticles);
//                        }

//                        return response;
//                    }
//                }

//                return "I'm sorry, I couldn't generate a response at this time. Please try again.";
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting news response for message: {Message}", userMessage);
//                return "I apologize, but I encountered an error while searching our news database. Please try rephrasing your question.";
//            }
//        }

//        private async Task<List<Article>> SearchRelevantArticlesAsync(string userMessage)
//        {
//            try
//            {
//                var searchTerms = ExtractSearchTerms(userMessage.ToLower());

//                var query = _context.Articles
//                    .Include(a => a.Author)
//                    .Include(a => a.Category)
//                    .Where(a => a.ArticleStatus == ArticleStatus.Published);

//                // Search in multiple fields
//                var articles = await query
//                    .Where(a =>
//                        searchTerms.Any(term =>
//                            a.Headline.ToLower().Contains(term) ||
//                            a.ContentSummary.ToLower().Contains(term) ||
//                            a.Content.ToLower().Contains(term) ||
//                            a.Author.FirstName.ToLower().Contains(term) ||
//                            a.Author.LastName.ToLower().Contains(term) ||
//                            a.Category.Name.ToLower().Contains(term)
//                        )
//                    )
//                    .OrderByDescending(a => a.DateStamp)
//                    .Take(5) // Limit to top 5 most relevant articles
//                    .ToListAsync();

//                return articles;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error searching articles for: {Message}", userMessage);
//                return new List<Article>();
//            }
//        }

//        private List<string> ExtractSearchTerms(string message)
//        {
//            // Simple keyword extraction - you can make this more sophisticated
//            var words = message.Split(new char[] { ' ', ',', '.', '?', '!', ';', ':', '\n', '\r' },
//                StringSplitOptions.RemoveEmptyEntries);

//            // Filter out common words and short terms
//            var stopWords = new HashSet<string>
//            {
//                "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
//                "of", "with", "by", "about", "what", "when", "where", "who", "why",
//                "how", "is", "are", "was", "were", "be", "been", "have", "has", "had",
//                "do", "does", "did", "will", "would", "could", "should", "may", "might",
//                "can", "tell", "me", "you", "i", "we", "they", "them", "this", "that"
//            };

//            return words
//                .Where(w => w.Length > 2 && !stopWords.Contains(w))
//                .Distinct()
//                .ToList();
//        }

//        private string BuildArticlesContext(List<Article> articles)
//        {
//            if (!articles.Any())
//                return "No relevant articles found in the database.";

//            var context = new StringBuilder();
//            context.AppendLine("Here are the relevant articles from YesterdayNews database:");
//            context.AppendLine();

//            foreach (var article in articles)
//            {
//                context.AppendLine($"Title: {article.Headline}");
//                context.AppendLine($"Author: {article.Author?.FirstName} {article.Author?.LastName}");
//                context.AppendLine($"Category: {article.Category?.Name}");
//                context.AppendLine($"Published: {article.DateStamp:yyyy-MM-dd}");
//                context.AppendLine($"Summary: {article.ContentSummary}");

//                // Include first 300 characters of content for context
//                var contentPreview = article.Content.Length > 300
//                    ? article.Content.Substring(0, 300) + "..."
//                    : article.Content;
//                context.AppendLine($"Content Preview: {contentPreview}");
//                context.AppendLine($"Views: {article.Views}, Likes: {article.Likes}");
//                context.AppendLine("---");
//            }

//            return context.ToString();
//        }

//        private List<ChatMessage> BuildChatMessages(string userMessage, List<ChatMessageViewModel> conversationHistory, string articlesContext)
//        {
//            var messages = new List<ChatMessage>();

//            // System message with context
//            var systemPrompt = $@"You are a helpful news assistant for YesterdayNews website. Your role is to help users find and understand news articles from our database.

//IMPORTANT GUIDELINES:
//1. Only provide information based on the articles provided in the context below
//2. If the user asks about something not covered in the provided articles, politely explain that you can only discuss articles available in the YesterdayNews database
//3. Be conversational, helpful, and informative
//4. When referencing articles, mention the title, author, and key details
//5. If no relevant articles are found, suggest the user try different search terms
//6. Keep responses concise but informative
//7. Always stay focused on news and articles from YesterdayNews

//CONTEXT FROM DATABASE:
//{articlesContext}

//Answer the user's questions based only on the information provided above.";

//            messages.Add(new SystemChatMessage(systemPrompt));

//            // Add conversation history (last 6 messages to keep context reasonable)
//            foreach (var historyMessage in conversationHistory.TakeLast(6))
//            {
//                if (historyMessage.Type.ToLower() == "user")
//                {
//                    messages.Add(new UserChatMessage(historyMessage.Content));
//                }
//                else if (historyMessage.Type.ToLower() == "bot")
//                {
//                    messages.Add(new AssistantChatMessage(historyMessage.Content));
//                }
//            }

//            // Add current user message
//            messages.Add(new UserChatMessage(userMessage));

//            return messages;
//        }

//        private string AddArticleReferences(string response, List<Article> relevantArticles)
//        {
//            if (!relevantArticles.Any())
//                return response;

//            var references = new StringBuilder();
//            references.AppendLine();
//            references.AppendLine("📰 **Related Articles:**");

//            foreach (var article in relevantArticles.Take(3)) // Show max 3 references
//            {
//                references.AppendLine($"• **{article.Headline}** by {article.Author?.FirstName} {article.Author?.LastName} ({article.DateStamp:MMM dd, yyyy})");
//            }

//            return response + references.ToString();
//        }
//    }
//}
using Azure;
using Azure.AI.OpenAI;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using System.Text;
using YesterdayNews.Data;
using YesterdayNews.Models.Db;
using YesterdayNews.Models.ViewModels;
using YesterdayNews.Services.IServices;

namespace YesterdayNews.Services
{
    public class NewsChatService : INewsChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NewsChatService> _logger;
        private readonly AzureOpenAIClient _azureClient;
        private readonly ChatClient _chatClient;

        public NewsChatService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<NewsChatService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;

            // Initialize Azure OpenAI client
            var endpoint = _configuration["AzureOpenAI:Endpoint"] ?? "https://foundryservicegr12.openai.azure.com/";
            var apiKey = _configuration["AzureOpenAI:Key"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Azure OpenAI API key is not configured. Please set AzureOpenAI:Key in your configuration.");
            }

            var credential = new AzureKeyCredential(apiKey);
            _azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);

            var deploymentName = _configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4o-2-gr12";
            _chatClient = _azureClient.GetChatClient(deploymentName);
        }

        public async Task<string> GetNewsResponseAsync(string userMessage, List<ChatMessageViewModel> conversationHistory)
        {
            try
            {
                // Check if user is asking for statistics or general information
                var isStatisticsQuery = IsStatisticsQuery(userMessage);

                List<Article> relevantArticles;
                string articlesContext;

                if (isStatisticsQuery)
                {
                    // Get database statistics
                    var stats = await GetDatabaseStatisticsAsync();
                    articlesContext = stats;
                    relevantArticles = new List<Article>(); // No specific articles for stats queries
                }
                else
                {
                    // Search for relevant articles
                    relevantArticles = await SearchRelevantArticlesAsync(userMessage);
                    articlesContext = BuildArticlesContext(relevantArticles);
                }

                // Get general database info for context
                var generalStats = await GetGeneralDatabaseInfoAsync();

                // Build conversation history for OpenAI
                var messages = BuildChatMessages(userMessage, conversationHistory, articlesContext, generalStats);

                // Create chat completion options
                var options = new ChatCompletionOptions
                {
                    Temperature = 0.7f,
                    MaxOutputTokenCount = 1000,
                    TopP = 0.95f,
                    FrequencyPenalty = 0f,
                    PresencePenalty = 0f
                };

                // Get response from Azure OpenAI
                var completionResult = await _chatClient.CompleteChatAsync(messages, options);

                if (completionResult.Value != null)
                {
                    var completion = completionResult.Value;

                    if (completion.Content != null && completion.Content.Count > 0)
                    {
                        var response = completion.Content[0].Text;

                        // Add article references if relevant articles were found (not for stats queries)
                        if (relevantArticles.Any() && !isStatisticsQuery)
                        {
                            response = AddArticleReferences(response, relevantArticles);
                        }

                        return response;
                    }
                }

                return "I'm sorry, I couldn't generate a response at this time. Please try again.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting news response for message: {Message}", userMessage);
                return "I apologize, but I encountered an error while searching our news database. Please try rephrasing your question.";
            }
        }

        private bool IsStatisticsQuery(string userMessage)
        {
            var statsKeywords = new[]
            {
                "how many", "total", "count", "number of", "statistics", "stats",
                "overview", "summary", "authors", "categories", "published", "articles",
                "most popular", "trending", "recent", "latest", "top", "best"
            };

            return statsKeywords.Any(keyword => userMessage.ToLower().Contains(keyword));
        }

        private async Task<string> GetDatabaseStatisticsAsync()
        {
            try
            {
                var stats = new StringBuilder();
                stats.AppendLine("=== YESTERDAYNEWS DATABASE STATISTICS ===");
                stats.AppendLine();

                // Article counts by status
                var publishedCount = await _context.Articles.CountAsync(a => a.ArticleStatus == ArticleStatus.Published);
                var draftCount = await _context.Articles.CountAsync(a => a.ArticleStatus == ArticleStatus.Draft);
                var pendingCount = await _context.Articles.CountAsync(a => a.ArticleStatus == ArticleStatus.PendingReview);
                var rejectedCount = await _context.Articles.CountAsync(a => a.ArticleStatus == ArticleStatus.Rejected);
                var archivedCount = await _context.Articles.CountAsync(a => a.ArticleStatus == ArticleStatus.Archived);

                stats.AppendLine("📊 ARTICLE COUNTS BY STATUS:");
                stats.AppendLine($"• Published: {publishedCount}");
                stats.AppendLine($"• Draft: {draftCount}");
                //stats.AppendLine($"• Pending Review: {pendingCount}");
                //stats.AppendLine($"• Rejected: {rejectedCount}");
                stats.AppendLine($"• Archived: {archivedCount}");
                stats.AppendLine($"• Total Articles: {publishedCount + draftCount + pendingCount + rejectedCount + archivedCount}");
                stats.AppendLine();

                // Categories
                var categoriesWithCounts = await _context.Articles
                    .Where(a => a.ArticleStatus == ArticleStatus.Published)
                    .Include(a => a.Category)
                    .GroupBy(a => a.Category.Name)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                stats.AppendLine("📂 ARTICLES BY CATEGORY:");
                foreach (var cat in categoriesWithCounts)
                {
                    stats.AppendLine($"• {cat.Category}: {cat.Count} articles");
                }
                stats.AppendLine();

                // Top authors
                var topAuthors = await _context.Articles
                    .Where(a => a.ArticleStatus == ArticleStatus.Published)
                    .Include(a => a.Author)
                    .GroupBy(a => new { a.Author.FirstName, a.Author.LastName })
                    .Select(g => new {
                        AuthorName = g.Key.FirstName + " " + g.Key.LastName,
                        ArticleCount = g.Count(),
                        TotalViews = g.Sum(x => x.Views),
                        TotalLikes = g.Sum(x => x.Likes)
                    })
                    .OrderByDescending(x => x.ArticleCount)
                    .Take(10)
                    .ToListAsync();

                stats.AppendLine("👥 TOP AUTHORS:");
                foreach (var author in topAuthors)
                {
                    stats.AppendLine($"• {author.AuthorName}: {author.ArticleCount} articles, {author.TotalViews} views, {author.TotalLikes} likes");
                }
                stats.AppendLine();

                // Most popular articles
                var popularArticles = await _context.Articles
                    .Where(a => a.ArticleStatus == ArticleStatus.Published)
                    .Include(a => a.Author)
                    .Include(a => a.Category)
                    .OrderByDescending(a => a.Views)
                    .Take(5)
                    .Select(a => new {
                        a.Headline,
                        AuthorName = a.Author.FirstName + " " + a.Author.LastName,
                        a.Category.Name,
                        a.Views,
                        a.Likes,
                        a.DateStamp
                    })
                    .ToListAsync();

                stats.AppendLine("🔥 MOST POPULAR ARTICLES:");
                foreach (var article in popularArticles)
                {
                    stats.AppendLine($"• '{article.Headline}' by {article.AuthorName} - {article.Views} views, {article.Likes} likes ({article.DateStamp:MMM dd, yyyy})");
                }
                stats.AppendLine();

                // Recent activity
                var recentArticles = await _context.Articles
                    .Where(a => a.ArticleStatus == ArticleStatus.Published)
                    .Include(a => a.Author)
                    .OrderByDescending(a => a.DateStamp)
                    .Take(5)
                    .Select(a => new {
                        a.Headline,
                        AuthorName = a.Author.FirstName + " " + a.Author.LastName,
                        a.DateStamp
                    })
                    .ToListAsync();

                stats.AppendLine("📅 RECENTLY PUBLISHED:");
                foreach (var article in recentArticles)
                {
                    stats.AppendLine($"• '{article.Headline}' by {article.AuthorName} ({article.DateStamp:MMM dd, yyyy})");
                }
                stats.AppendLine();

                // Engagement stats
                var totalViews = await _context.Articles
                    .Where(a => a.ArticleStatus == ArticleStatus.Published)
                    .SumAsync(a => a.Views);

                var totalLikes = await _context.Articles
                    .Where(a => a.ArticleStatus == ArticleStatus.Published)
                    .SumAsync(a => a.Likes);

                var averageViews = publishedCount > 0 ? totalViews / publishedCount : 0;
                var averageLikes = publishedCount > 0 ? totalLikes / publishedCount : 0;

                stats.AppendLine("💡 ENGAGEMENT STATISTICS:");
                stats.AppendLine($"• Total Views: {totalViews:N0}");
                stats.AppendLine($"• Total Likes: {totalLikes:N0}");
                stats.AppendLine($"• Average Views per Article: {averageViews:N0}");
                stats.AppendLine($"• Average Likes per Article: {averageLikes:N0}");

                return stats.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database statistics");
                return "Error retrieving database statistics.";
            }
        }

        private async Task<string> GetGeneralDatabaseInfoAsync()
        {
            try
            {
                var publishedCount = await _context.Articles.CountAsync(a => a.ArticleStatus == ArticleStatus.Published);
                var authorCount = await _context.Users.CountAsync();
                var categoryCount = await _context.Categories.CountAsync();

                return $"YesterdayNews has {publishedCount} published articles, {authorCount} authors, and {categoryCount} categories.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting general database info");
                return "Database information unavailable.";
            }
        }

        private async Task<List<Article>> SearchRelevantArticlesAsync(string userMessage)
        {
            try
            {
                var searchTerms = ExtractSearchTerms(userMessage.ToLower());

                var query = _context.Articles
                    .Include(a => a.Author)
                    .Include(a => a.Category)
                    .Where(a => a.ArticleStatus == ArticleStatus.Published);

                // Search in multiple fields
                var articles = await query
                    .Where(a =>
                        searchTerms.Any(term =>
                            a.Headline.ToLower().Contains(term) ||
                            a.ContentSummary.ToLower().Contains(term) ||
                            a.Content.ToLower().Contains(term) ||
                            a.Author.FirstName.ToLower().Contains(term) ||
                            a.Author.LastName.ToLower().Contains(term) ||
                            a.Category.Name.ToLower().Contains(term)
                        )
                    )
                    .OrderByDescending(a => a.DateStamp)
                    .Take(5) // Limit to top 5 most relevant articles
                    .ToListAsync();

                return articles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching articles for: {Message}", userMessage);
                return new List<Article>();
            }
        }

        private List<string> ExtractSearchTerms(string message)
        {
            // Simple keyword extraction - you can make this more sophisticated
            var words = message.Split(new char[] { ' ', ',', '.', '?', '!', ';', ':', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries);

            // Filter out common words and short terms
            var stopWords = new HashSet<string>
            {
                "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
                "of", "with", "by", "about", "what", "when", "where", "who", "why",
                "how", "is", "are", "was", "were", "be", "been", "have", "has", "had",
                "do", "does", "did", "will", "would", "could", "should", "may", "might",
                "can", "tell", "me", "you", "i", "we", "they", "them", "this", "that"
            };

            return words
                .Where(w => w.Length > 2 && !stopWords.Contains(w))
                .Distinct()
                .ToList();
        }

        private string BuildArticlesContext(List<Article> articles)
        {
            if (!articles.Any())
                return "No relevant articles found in the database.";

            var context = new StringBuilder();
            context.AppendLine("Here are the relevant articles from YesterdayNews database:");
            context.AppendLine();

            foreach (var article in articles)
            {
                context.AppendLine($"Title: {article.Headline}");
                context.AppendLine($"Author: {article.Author?.FirstName} {article.Author?.LastName}");
                context.AppendLine($"Category: {article.Category?.Name}");
                context.AppendLine($"Published: {article.DateStamp:yyyy-MM-dd}");
                context.AppendLine($"Summary: {article.ContentSummary}");

                // Include first 300 characters of content for context
                var contentPreview = article.Content.Length > 300
                    ? article.Content.Substring(0, 300) + "..."
                    : article.Content;
                context.AppendLine($"Content Preview: {contentPreview}");
                context.AppendLine($"Views: {article.Views}, Likes: {article.Likes}");
                context.AppendLine("---");
            }

            return context.ToString();
        }

        private List<ChatMessage> BuildChatMessages(string userMessage, List<ChatMessageViewModel> conversationHistory, string articlesContext, string generalInfo)
        {
            var messages = new List<ChatMessage>();

            // System message with context
            var systemPrompt = $@"You are a helpful news assistant for YesterdayNews website. Your role is to help users find and understand news articles from our database and provide insights about the website's content.

GENERAL DATABASE INFO:
{generalInfo}

IMPORTANT GUIDELINES:
1. You can answer questions about:
   - Specific articles and their content
   - Database statistics (number of articles, authors, categories, etc.)
   - Popular articles, trending topics, author information
   - Article engagement (views, likes)
   - Content categories and their popularity
   - Recent publications and activity

2. Only provide information based on the data provided in the context
3. If the user asks about something not available in the database, politely explain the limitations
4. Be conversational, helpful, and informative
5. When referencing articles, mention the title, author, and key details
6. For statistics queries, provide comprehensive and well-formatted information
7. Keep responses concise but informative
8. Always stay focused on YesterdayNews content

CONTEXT FROM DATABASE:
{articlesContext}

Answer the user's questions based only on the information provided above.";

            messages.Add(new SystemChatMessage(systemPrompt));

            // Add conversation history (last 6 messages to keep context reasonable)
            foreach (var historyMessage in conversationHistory.TakeLast(6))
            {
                if (historyMessage.Type.ToLower() == "user")
                {
                    messages.Add(new UserChatMessage(historyMessage.Content));
                }
                else if (historyMessage.Type.ToLower() == "bot")
                {
                    messages.Add(new AssistantChatMessage(historyMessage.Content));
                }
            }

            // Add current user message
            messages.Add(new UserChatMessage(userMessage));

            return messages;
        }

        private string AddArticleReferences(string response, List<Article> relevantArticles)
        {
            if (!relevantArticles.Any())
                return response;

            var references = new StringBuilder();
            references.AppendLine();
            references.AppendLine("📰 **Related Articles:**");

            foreach (var article in relevantArticles.Take(3)) // Show max 3 references
            {
                references.AppendLine($"• **{article.Headline}** by {article.Author?.FirstName} {article.Author?.LastName} ({article.DateStamp:MMM dd, yyyy})");
            }

            return response + references.ToString();
        }
    }
}