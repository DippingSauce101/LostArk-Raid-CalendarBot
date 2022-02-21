using System;

namespace DiscordLostArkBot.Discord.Command
{
    internal struct CreateRaidCommandParam : ICommandParam
    {
        public readonly string Title;
        public readonly DateTime Time;

        public CreateRaidCommandParam(string title, DateTime time)
        {
            Title = title;
            Time = time;
        }
    }
}