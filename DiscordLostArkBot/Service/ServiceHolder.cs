using DiscordLostArkBot.Model;

namespace DiscordLostArkBot.Service
{
    public static class ServiceHolder
    {
        public static RaidInfoService RaidInfo { get; private set; }

        public static void InitServices(RaidInfoCollection raidInfoCollection)
        {
            RaidInfo = new RaidInfoService(raidInfoCollection);
        }
    }
}