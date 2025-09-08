namespace YesterdayNews.Utils
{
    public class LocationUtils
    {


        private static readonly string[] NoisySuffixes = new[]
       {
            "Municipality", "Kommun", "County", "City",
            "Region", "District", "Province", "Prefecture"
        };


        public static string CleanCityName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "Unknown";

            foreach (var suffix in NoisySuffixes)
            {
                if (raw.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    raw = raw.Substring(0, raw.Length - suffix.Length).Trim();
                }
            }

            return raw;
        }

    }
}
