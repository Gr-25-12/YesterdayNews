namespace YesterdayNews.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        // General Statistics
        public int TotalArticles { get; set; }
        public int PublishedArticles { get; set; }
        public int DraftArticles { get; set; }
        public int PendingReviewArticles { get; set; }
        public int RejectedArticles { get; set; }
        public int ArchivedArticles { get; set; }

        // User Statistics
        public int TotalUsers { get; set; }
        public int CustomersCount { get; set; }
        public int JournalistsCount { get; set; }
        public int EditorsCount { get; set; }
        public int AdminsCount { get; set; }
        public int ActiveUsers { get; set; }
        public int LockedUsers { get; set; }

        // Subscription Statistics
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int ExpiredSubscriptions { get; set; }

        // Revenue Statistics
        public decimal TotalRevenue { get; set; }
        public decimal RevenueLastWeek { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public decimal RevenueLast6Months { get; set; }

        // Subscription Counts by Time Period
        public int SubscriptionsLast7Days { get; set; }
        public int SubscriptionsLast30Days { get; set; }
        public int SubscriptionsLast6Months { get; set; }

        // Chart Data
        public List<ChartDataPoint> SubscriptionsByDay { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> RevenueByDay { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> SubscriptionsByType { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> ArticlesByStatus { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> UsersByRole { get; set; } = new List<ChartDataPoint>();

        // Recent Activities
        public List<RecentArticle> RecentArticles { get; set; } = new List<RecentArticle>();
        public List<RecentSubscription> RecentSubscriptions { get; set; } = new List<RecentSubscription>();

        // Top Performing Content
        public List<TopArticle> MostViewedArticles { get; set; } = new List<TopArticle>();
        public List<TopArticle> MostLikedArticles { get; set; } = new List<TopArticle>();
    }

    public class ChartDataPoint
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
        public string Color { get; set; }
    }

    public class RecentArticle
    {
        public int Id { get; set; }
        public string Headline { get; set; }
        public string AuthorName { get; set; }
        public string Status { get; set; }
        public DateTime DateCreated { get; set; }
        public string CategoryName { get; set; }
    }

    public class RecentSubscription
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string SubscriptionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool PaymentComplete { get; set; }
    }

    public class TopArticle
    {
        public int Id { get; set; }
        public string Headline { get; set; }
        public string AuthorName { get; set; }
        public int Views { get; set; }
        public int Likes { get; set; }
        public string CategoryName { get; set; }
        public DateTime DatePublished { get; set; }
    }
}
