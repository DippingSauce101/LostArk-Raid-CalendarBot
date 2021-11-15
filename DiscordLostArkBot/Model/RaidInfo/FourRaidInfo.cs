using System.Linq;

namespace DiscordLostArkBot.Model.RaidInfo
{
    public class FourRaidInfo : RaidInfo
    {
        private const int players_count = 4;

        private readonly RaidPlayer.Role[] _partyRoles =
        {
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Support
        };

        public FourRaidInfo()
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