namespace DiscordLostArkBot.Utilities
{
    public class Singleton<T> where T : class, new()
    {
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Singleton()
        {
        }

        public static T Ins { get; } = new();
    }
}