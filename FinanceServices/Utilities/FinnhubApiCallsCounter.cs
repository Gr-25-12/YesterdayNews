using Microsoft.Extensions.Logging;

namespace FinanceServices.Utilities
{
    public class FinnhubApiCallsCounter
    {
        private readonly ILogger<FinnhubApiCallsCounter> _logger;
        private static int SecondCounter { get; set; } = 0;
        private static int MinuteCounter { get; set; } = 0;
        private static int DayCounter { get; set; } = 0;
        private const int MAX_CALL_PERSECOND = 3; //30
        private const int MAX_CALL_PERMINUTE = 6; //60
        private const int MAX_CALL_PERDAY = 50;   //500

        private static DateTime lastSecondReset = DateTime.UtcNow;
        private static DateTime lastMinuteReset = DateTime.UtcNow;
        private static DateTime lastDayReset = DateTime.UtcNow;
        public FinnhubApiCallsCounter(ILogger<FinnhubApiCallsCounter> logger)
        {
            _logger = logger;
        }

        private readonly object _lock = new();
        public bool IsCallPossible()
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;

                // Reset counters if interval passed
                if ((now - lastSecondReset).TotalSeconds >= 1)
                    ResetSecondCounter(now);
                if ((now - lastMinuteReset).TotalMinutes >= 1)
                    ResetMinuteCounter(now);
                if ((now - lastDayReset).TotalDays >= 1)
                    ResetDayCounter(now);

                if (IsLimitReached())
                {
                    return false;
                }
                IncrementCounters();
                PrintToConsole();
                return true;
            }
        }
        private static void ResetSecondCounter(DateTime now)
        {
            SecondCounter = 0;
            lastSecondReset = now;
        }
        private static void ResetMinuteCounter(DateTime now)
        {
            MinuteCounter = 0;
            lastMinuteReset = now;
        }
        private static void ResetDayCounter(DateTime now)
        {
            DayCounter = 0;
            lastDayReset = now;
        }

        private static bool IsLimitReached()
        {
            if (SecondCounter >= MAX_CALL_PERSECOND ||
                    MinuteCounter >= MAX_CALL_PERMINUTE ||
                    DayCounter >= MAX_CALL_PERDAY)
            {
                return true;
            }
            return false;
        }

        private static void IncrementCounters()
        {
            SecondCounter++;
            MinuteCounter++;
            DayCounter++;
        }

        /// <summary>
        /// Use this for testing only
        /// </summary>
        public void PrintToConsole()
        {
            _logger.LogInformation($"Calls on SecondCounter: {SecondCounter}");
            _logger.LogInformation($"Calls on MinuteCounter: {MinuteCounter}");
            _logger.LogInformation($"Calls on DayCounter: {DayCounter}");
        }
    }
}
