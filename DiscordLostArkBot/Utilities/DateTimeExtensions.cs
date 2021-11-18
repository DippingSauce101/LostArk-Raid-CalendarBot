using System;

namespace DiscordLostArkBot.Utilities
{
    public static class DateTimeExtensions
    {
        public static long ToUtcMillis(this DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }
    }
}