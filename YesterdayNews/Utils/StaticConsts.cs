namespace YesterdayNews.Utils
{
    public static class StaticConsts
    {
        public const string ArticleDraft = "Draft";
        public const string ArticlePendingReview = "PendingReview";
        public const string ArticleRejected = "Rejected";
        public const string ArticlePublished = "Published";
        public const string ArticleArchived = "Archived";


        public const string Role_Customer = "Customer";
        public const string Role_Journalist = "Journalist";
        public const string Role_Admin = "Admin";
        public const string Role_Editor = "Editor";


        public const int Cookie_Expires_IN = 7;
        public const int ARTICLE_ARCHIVED_IN = 7; //Days


        public const string SubscriptionType_Monthly = "Monthly";
        public const string SubscriptionType_Yearly = "Yearly";
        public const string SubscriptionType_Quarterly = "Quarterly";
        public const string SubscriptionType_Weekly = "Weekly";

#if DEBUG
        public const string Home_URL = @"https://localhost:7195/";
#else
    public const string Home_URL = @"https://yesterdaynews.azurewebsites.net/";
#endif

        public const string YN_LOGO = "https://yesterdaystoragegr12.blob.core.windows.net/notarticles/YN_logo.png";

    }
}
