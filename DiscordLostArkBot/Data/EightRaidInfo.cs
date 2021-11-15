using System.Linq;

namespace DiscordLostArkBot.Data
{
    public class EightRaidInfo : RaidInfo
    {
        private const int players_count = 8;

        private readonly RaidPlayer.Role[] _partyRoles =
        {
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Support,
            RaidPlayer.Role.Support
        };

        public EightRaidInfo()
        {
            RaidPlayers = new RaidPlayer[players_count];
            for (var i = 0; i < players_count; i++)
                RaidPlayers[i] = new RaidPlayer
                {
                    UserRole = _partyRoles[i]
                };
        }
    }
}