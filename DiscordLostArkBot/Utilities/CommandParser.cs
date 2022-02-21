using DiscordLostArkBot.Discord.Command.Parser;

namespace DiscordLostArkBot.Utilities
{
    internal static class CommandParser
    {
        public static CreateRaidCommandParamParser CreateRaid = new CreateRaidCommandParamParser();
        public static ModifyRaidCommandParamParser ModifyRaid = new ModifyRaidCommandParamParser();
        public static ModifyRaidInThreadCommandParamParser ModifyRaidInThread = new ModifyRaidInThreadCommandParamParser();
    }
}