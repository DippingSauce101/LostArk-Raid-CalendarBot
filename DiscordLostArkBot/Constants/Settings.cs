using DiscordLostArkBot.Utilities;

namespace DiscordLostArkBot.Constants
{
    public static class Settings
    {
        public static string DiscordBotToken;
        public static string NotionApiAuthToken;
        public static string NotionCalendarDbId;
        public static string NotionCalendarUrl;

        public static bool Load(IniFile iniFile)
        {
            DiscordBotToken = iniFile["Tokens"]["DiscordBotToken"].GetString();
            NotionCalendarUrl = iniFile["Tokens"]["NotionCalendarUrl"].GetString();
            NotionApiAuthToken = iniFile["Tokens"]["NotionApiAuthToken"].GetString();
            NotionCalendarDbId = iniFile["Tokens"]["NotionCalendarDbId"].GetString();

            return AreTokensValid();
        }

        private static bool AreTokensValid()
        {
            if (string.IsNullOrWhiteSpace(DiscordBotToken) ||
                string.IsNullOrWhiteSpace(NotionApiAuthToken) ||
                string.IsNullOrWhiteSpace(NotionCalendarDbId) ||
                string.IsNullOrWhiteSpace(NotionCalendarUrl))
            {
                return false;
            }

            return true;
        }
    }
}