using System.Text.Json.Serialization;

public class ExternalNewsVM
{
    [JsonPropertyName("title")]
    public string Title { get; set; } 

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("urlToImage")]
    public string? UrlToImage { get; set; }

    [JsonPropertyName("publishedAt")]
    public DateTime? PublishedAt { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    // For the nested source object
    [JsonPropertyName("source")]
    public NewsSource? Source { get; set; }

    // Helper property to get source name easily
    [JsonIgnore]  // Add this to exclude from JSON serialization
    public string SourceName => Source?.Name ?? "Unknown Source";

    

}

public class NewsSource
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

public class NewsApiResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("totalResults")]
    public int TotalResults { get; set; }

    [JsonPropertyName("articles")]
    public List<ExternalNewsVM> Articles { get; set; } = new List<ExternalNewsVM>();
}