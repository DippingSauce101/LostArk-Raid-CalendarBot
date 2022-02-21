using System;

namespace DiscordLostArkBot.Discord.Command
{
    internal struct ModifyRaidCommandParam : ICommandParam
    {
        public readonly ulong RaidDataId;
        public readonly string Title;
        public readonly DateTime? NewDateTime;

        public ModifyRaidCommandParam(ulong raidDataId, string title, DateTime? newDateTime)
        {
            RaidDataId = raidDataId;
            Title = title;
            NewDateTime = newDateTime;
        }
    }
}