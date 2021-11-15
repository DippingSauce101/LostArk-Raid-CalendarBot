using DiscordLostArkBot.Model;

namespace DiscordLostArkBot.Presenter
{
    public static class Presenters
    {
        public static RaidInfoPresenter RaidInfo { get; private set; }

        public static void InitPresenters(RaidInfoCollection raidInfoCollection)
        {
            RaidInfo = new RaidInfoPresenter(raidInfoCollection);
        }
    }
}