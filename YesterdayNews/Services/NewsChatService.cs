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
                stats.AppendLine("=== 📊 YESTERDAYNEWS DATABASE STATISTICS ===");
                stats.AppendLine();

                // --- Article counts
                var publishedCount = await _context.Articles.CountAsync(a => a.ArticleStatus == ArticleStatus.Published);
                var archivedCount = await _context.Articles.CountAsync(a => a.ArticleStatus == ArticleStatus.Archived);

                stats.AppendLine("📰 Article Counts:");
                stats.AppendLine($"  • Published: {publishedCount}");
                stats.AppendLine($"  • Archived: {archivedCount}");
                stats.AppendLine($"  • Total: {publishedCount + archivedCount}");
                stats.AppendLine();

                // --- Most viewed article
                var mostViewed = await _context.Articles
                    .Where(a => a.ArticleStatus == ArticleStatus.Published)
                    .OrderByDescending(a => a.Views)
                    .Select(a => new { a.Headline, a.Views, a.DateStamp })
                    .FirstOrDefaultAsync();

                if (mostViewed != null)
                {
                    stats.AppendLine("👀 Most Viewed Article:");
                    stats.AppendLine($"  • \"{mostViewed.Headline}\"");
                    stats.AppendLine($"    → {mostViewed.Views:N0} views ({mostViewed.DateStamp:MMM dd, yyyy})");
                    stats.AppendLine();
                }

                // --- Most liked article
                var mostLiked = await _context.Articles
                    .Where(a => a.ArticleStatus == ArticleStatus.Published)
                    .OrderByDescending(a => a.Likes)
                    .Select(a => new { a.Headline, a.Likes, a.DateStamp, a.Author.FullName })
                    .FirstOrDefaultAsync();

                if (mostLiked != null)
                {
                    stats.AppendLine("❤️ Most Liked Article:");
                    stats.AppendLine($"  • \"{mostLiked.Headline}\"");
                    stats.AppendLine($"    → {mostLiked.Likes:N0} likes ({mostLiked.DateStamp:MMM dd, yyyy})");
                    stats.AppendLine($"    → by {mostLiked.FullName}");
                    stats.AppendLine();
                }

                // --- Categories
                var categories = await _context.Articles
                    .Where(a => a.ArticleStatus == ArticleStatus.Published)
                    .Include(a => a.Category)
                    .GroupBy(a => a.Category.Name)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                stats.AppendLine("📂 Articles by Category:");
                foreach (var cat in categories)
                    stats.AppendLine($"  • {cat.Category}: {cat.Count}");
                stats.AppendLine();

                // --- Recent 5 articles
                var recent = await _context.Articles
                    .Where(a => a.ArticleStatus == ArticleStatus.Published)
                    .OrderByDescending(a => a.DateStamp)
                    .Take(5)
                    .Select(a => new { a.Headline, a.DateStamp })
                    .ToListAsync();

                stats.AppendLine("🆕 Recently Published:");
                foreach (var art in recent)
                    stats.AppendLine($"  • \"{art.Headline}\" ({art.DateStamp:MMM dd, yyyy})");
                stats.AppendLine();

                var plans = await _context.SubscriptionTypes.ToListAsync();
                stats.AppendLine("💳 Subscription Plans:");
                stats.AppendLine($"  • Total Plans: {plans.Count}");

                foreach (var plan in plans)
                {
                    stats.AppendLine($"    - {plan.TypeName}: {plan.Price:C}");
                }

                stats.AppendLine();



                var (editorCount, journalistCount) = await GetEditorsAndJournlistsNumber();


                stats.AppendLine("👥 User Statistics:\n");
                stats.AppendLine($"  • Editors (Editor): {editorCount}");
                stats.AppendLine($"  • Journalists: {journalistCount}");
               
                stats.AppendLine();

                // --- Engagement
                var totalViews = await _context.Articles.Where(a => a.ArticleStatus == ArticleStatus.Published).SumAsync(a => a.Views);
                var totalLikes = await _context.Articles.Where(a => a.ArticleStatus == ArticleStatus.Published).SumAsync(a => a.Likes);

                stats.AppendLine("📈 Engagement Stats:");
                stats.AppendLine($"  • Total Views: {totalViews:N0}");
                stats.AppendLine($"  • Total Likes: {totalLikes:N0}");
                stats.AppendLine($"  • Avg Views per Article: {(publishedCount > 0 ? totalViews / publishedCount : 0):N0}");
                stats.AppendLine($"  • Avg Likes per Article: {(publishedCount > 0 ? totalLikes / publishedCount : 0):N0}");

                return stats.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database statistics");
                return "⚠️ Error retrieving database statistics.";
            }
        }


        private async Task<string> GetGeneralDatabaseInfoAsync()
        {
            try
            {
                var publishedCount = await _context.Articles.CountAsync(a => a.ArticleStatus == ArticleStatus.Published);
                var (editorCount, journalistCount) = await GetEditorsAndJournlistsNumber();

                var categoryCount = await _context.Categories.CountAsync();

                return $"YesterdayNews has {publishedCount} published articles ,\n {journalistCount} authors, {editorCount} editors. \n And {categoryCount} categories.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting general database info");
                return "Database information unavailable.";
            }
        }

        private async Task<(int editorCount, int journalistCount)> GetEditorsAndJournlistsNumber()
        {

            var adminRoleId = await _context.Roles
                .Where(r => r.Name == "Admin")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var editorRoleId = await _context.Roles
                .Where(r => r.Name == "Editor")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            var journalistRoleId = await _context.Roles
                .Where(r => r.Name == "Journalist")
                .Select(r => r.Id)
                .FirstOrDefaultAsync();


            var editorCount = await _context.UserRoles
                .Where(ur => ur.RoleId == adminRoleId || ur.RoleId == editorRoleId)
                .Select(ur => ur.UserId)
                .Distinct()
                .CountAsync();


            var journalistCount = await _context.UserRoles
                .Where(ur => ur.RoleId == journalistRoleId)
                .Select(ur => ur.UserId)
                .Distinct()
                .CountAsync();

            return (editorCount, journalistCount);
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
            var systemPrompt = $@"You are a helpful news assistant for YesterdayNews website and you name is Sir Newston. Your role is to help users find and understand news articles from our website which is YN or Yesterday news and provide insights about the website's content.

GENERAL DATABASE INFO:
{generalInfo}

IMPORTANT GUIDELINES:
1. You can answer questions about:
   - Specific articles and their content
   - YN statistics (number of articles, authors, categories, etc.)
   - Popular articles, trending topics, author information
   - Article engagement (views, likes)
   - Content categories and their popularity
   - Recent publications and activity

2. Only provide information based on the data provided in the context
3. If the user asks about something not available in the database, politely explain the limitations
4. Be conversational, helpful, and informative
5. When referencing articles, mention the title,and key details if provided
6. For statistics queries, provide comprehensive and well-formatted information
7. Keep responses concise but informative
8. Always stay focused on YesterdayNews content
9. any questions about the users personal data or account should be directed to the support team and direct the users to read our policy 
10. If you are unsure about an answer, it's better to say you don't know than to provide incorrect information
11. you can say the actualy number of the authors and actual number of editors and not the normal users based on the data you have

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
                references.AppendLine($"• {Truncate(article.Headline)} ({article.DateStamp:MMM dd, yyyy})"); ;
            }

            return response + Environment.NewLine + Environment.NewLine + references.ToString();
        }
        private string Truncate(string text, int maxLength = 50)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return text.Length <= maxLength ? text : text.Substring(0, maxLength) + "...";
        }
    }
}