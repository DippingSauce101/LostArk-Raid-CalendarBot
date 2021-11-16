namespace DiscordLostArkBot.Discord
{
    public static class ULongExtensions
    {
        public static string DiscordUserIdToRefString(this ulong val)
        {
            return $"<@{val}>";
        }
    }
}