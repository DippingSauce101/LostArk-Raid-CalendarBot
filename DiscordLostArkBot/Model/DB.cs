using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using DiscordLostArkBot.Constants;
using DiscordLostArkBot.Service;
using DiscordLostArkBot.Utilities;
using Newtonsoft.Json;

namespace DiscordLostArkBot.Model
{
    /// <summary>
    ///     프로젝트가 커진다면 데이터 관련 클래스는 별개 어셈블리로 빼고 인터페이스를 통해 통신하도록 수정되겠지만
    ///     현재 규모로는 굳이 그럴 이유는 없으므로 미래를 위해 Presenter 로직만 분리해둠
    /// </summary>
    public class DB : Singleton<DB>
    {
        /// <summary>
        ///     통짜 파일로 저장해도 되지만 레이드 정보 데이터가 늘어날수록 파일 Read/Write가 점점 느려질테니 RaidInfo 데이터 각각을
        ///     따로 파일로 저장하도록 수정함
        /// </summary>
        public RaidInfoCollection RaidInfoCollection = new();

        public static DB Init()
        {
            //Initialize Presenters
            var dbIns = Ins;
            if (dbIns.LoadAll())
            {
                Console.WriteLine("Loaded Database!");
                Console.WriteLine($"{Ins.RaidInfoCollection.GetCount()} Raid infos loaded...");
                for (var i = 0; i < Ins.RaidInfoCollection.GetCount(); i++)
                {
                    var info = Ins.RaidInfoCollection.ElementAt(i);
                    Console.WriteLine(
                        $"\t[{i}] {string.Format("{0,-27}", info.Title)} {info.RaidDateTimeUtc.ToString("yyyy-MM-dd HH:mm")}");
                }
            }
            else
            {
                Console.WriteLine("Database load from file failed...");
                Console.WriteLine("Creating new db!");
            }

            ServiceHolder.InitServices(Ins.RaidInfoCollection);
            return dbIns;
        }

        /// <summary>
        ///     저장된 데이터의 정합성 검사
        /// </summary>
        /// <returns></returns>
        // public static async Task<int> Validate()
        // {
        //     var raidInfoCollection = Ins.RaidInfoCollection;
        //     var discordClient = DiscordBotClient.Ins.Client;
        //     for (int i = 0; i < raidInfoCollection.GetCount(); i++)
        //     {
        //         var raidInfo = raidInfoCollection.ElementAt(i);
        //         var channelId = raidInfo.DiscordMessageKey.ChannelId;
        //         var messageId = raidInfo.DiscordMessageKey.MessageId;
        //         var messageChannel = discordClient.GetChannel(channelId) as IMessageChannel;
        //         if (messageChannel != null)
        //         {
        //             var message = await messageChannel.GetMessageAsync(messageId);
        //             asd
        //         }
        //         else
        //         {
        //             
        //         }
        //     }
        // }
        private static async Task<bool> ValidateUserRoleFromDiscordMessage(RaidInfo.RaidInfo savedInfo,
            IMessage message, RaidInfo.RaidInfo.RaidPlayer.Role targetRole)
        {
            var roleEmoji = RaidEmoji.RoleToDiscordEmoji(targetRole);
            //메세지의 리액션 정보를 읽어 저장된 데이터와 일치하는지 체크한다
            var reactedUsers = await message.GetReactionUsersAsync(roleEmoji, 10)
                .FlattenAsync();
            foreach (var reactedUser in reactedUsers)
                if (savedInfo.UserAlreadySeatedWithRole(reactedUser.Id, targetRole) == false)
                    return false;
            return true;
        }

        public bool SaveToFile(RaidInfo.RaidInfo raidInfo)
        {
            try
            {
                var savePath = GetSaveFilePath(raidInfo);
                var serializedText = JsonConvert.SerializeObject(raidInfo, Formatting.Indented);
                File.WriteAllText(savePath, serializedText);
                Console.WriteLine($"Saved at {savePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!!Exception during saving raid infos data!!!");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool Delete(RaidInfo.RaidInfo raidInfo)
        {
            try
            {
                var savePath = GetSaveFilePath(raidInfo);
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!!Exception during deleting raid infos data!!!");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool LoadAll()
        {
            var saveDir = GetSaveDirectoryPath();
            if (Directory.Exists(saveDir) == false) return true;
            var savedFilePaths = Directory.GetFiles(saveDir, "*.json");
            foreach (var filePath in savedFilePaths)
            {
                var text = File.ReadAllText(filePath);
                var deserialized = JsonConvert.DeserializeObject<RaidInfo.RaidInfo>(text);
                if (deserialized != null) RaidInfoCollection.Add(deserialized);
            }

            return true;
        }

        private static string GetSaveDirectoryPath()
        {
            var subDir = $"{AppContext.BaseDirectory}/RaidInfoDatas/";
            return subDir;
        }

        private static string GetSaveFilePath(RaidInfo.RaidInfo info)
        {
            var dir = GetSaveDirectoryPath();
            if (Directory.Exists(dir) == false) Directory.CreateDirectory(dir);

            //파일명 규칙 - [UTC_Millis]_[디스코드 채널 Id]_[디스코드 메세지 Id]
            var path =
                $"{dir}{info.GetRaidFileName()}.json";
            return path;
        }
    }
}