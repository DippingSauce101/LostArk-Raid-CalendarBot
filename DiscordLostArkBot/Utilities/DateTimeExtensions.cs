using System;

namespace DiscordLostArkBot.Utilities
{
    public static class DateTimeExtensions
    {
        public static long ToUtcMillis(this DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static string ToIso8601(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }

        public static DateTime KstToUtc(this DateTime dateTime)
        {
            dateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime);
            DateTime utcDateTime = new DateTime(dateTime.Ticks, DateTimeKind.Utc);
            return utcDateTime;
        }
        
        public static DateTime UtcToKst(this DateTime dateTime)
        {
            var kst = TimeZoneInfo.FindSystemTimeZoneById("Korea Standard Time");
            dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, kst);
            DateTime kstDateTime = new DateTime(dateTime.Ticks, DateTimeKind.Local);
            return kstDateTime;
        }
    }
}