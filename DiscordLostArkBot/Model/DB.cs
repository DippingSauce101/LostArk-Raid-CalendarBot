using System.Collections.Generic;
using System.Linq;
using Discord;
using DiscordLostArkBot.Presenter;
using DiscordLostArkBot.Utilities;

namespace DiscordLostArkBot.Model
{
    /// <summary>
    /// 프로젝트가 커진다면 데이터 관련 클래스는 별개 어셈블리로 빼고 인터페이스를 통해 통신하도록 수정되겠지만
    /// 현재 규모로는 굳이 그럴 이유는 없으므로 미래를 위해 Presenter 로직만 분리해둠 
    /// </summary>
    public class DB : Singleton<DB>
    {
        private RaidInfoCollection RaidInfoCollection { get; }
        public DB()
        {
            //Initialize Datas
            RaidInfoCollection = new RaidInfoCollection();
            
            //Initialize Presenters
            Presenters.InitPresenters(RaidInfoCollection);
        }
    }
}