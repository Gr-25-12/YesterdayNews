using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using YesterdayNews.Services.IServices;
using YesterdayNews.Utils;


namespace ArchiveFunctions
{
    public class ArchiveTimer
    {
        private readonly ILogger _logger;
        private readonly IArticleServices _articleService;

        public ArchiveTimer(ILoggerFactory loggerFactory, IArticleServices articleServices)
        {
            _logger = loggerFactory.CreateLogger<ArchiveTimer>();
            _articleService = articleServices;
        }

        [Function("ArchiveTimer")]
        public async Task Run([TimerTrigger("0 0 */1 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# ArchiveTimer trigger function executed at: {DateTime.Now}");

            var archiveThreshold = DateTime.UtcNow.AddDays(-StaticConsts.ARTICLE_ARCHIVED_IN);
            int archived = await _articleService.TryArchiveOldArticles(archiveThreshold);
            if (archived > 0)
                _logger.LogInformation($"{archived} articles archived.");
        }
    }
}
