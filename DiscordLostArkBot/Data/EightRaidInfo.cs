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

        public override bool AddOrChangePlayerRole(ulong userId, RaidPlayer.Role role)
        {
            var emptySeatIndex = GetEmptySeatIndex(role);
            if (emptySeatIndex == -1) return false;

            RaidPlayers[emptySeatIndex].UserId = userId;
            RaidPlayers[emptySeatIndex].UserRole = role;
            return true;
        }

        public override bool RemovePlayerRole(ulong userId, RaidPlayer.Role role)
        {
            for (var i = 0; i < players_count; i++)
                if (RaidPlayers[i].UserId == userId &&
                    RaidPlayers[i].UserRole == role)
                    RaidPlayers[i].UserId = RaidPlayer.UserEmpty;

            return false;
        }

        public override bool IsRoleFull(RaidPlayer.Role role)
        {
            var emptyRolesCount = RaidPlayers.Where(raidPlayer =>
            {
                return raidPlayer.UserRole == role && raidPlayer.IsEmpty();
            }).Count();
            return emptyRolesCount == 0;
        }

        public override int GetEmptySeatIndex(RaidPlayer.Role role)
        {
            for (var i = 0; i < players_count; i++)
                if (RaidPlayers[i].UserRole == role &&
                    RaidPlayers[i].IsEmpty())
                    return i;

            return -1;
        }
    }
}