using System;
using System.IO;
using System.Runtime.CompilerServices;
using DiscordLostArkBot.Service;
using DiscordLostArkBot.Utilities;
using Newtonsoft.Json;

namespace DiscordLostArkBot.Model
{
    /// <summary>
    ///     프로젝트가 커진다면 데이터 관련 클래스는 별개 어셈블리로 빼고 인터페이스를 통해 통신하도록 수정되겠지만
    ///     현재 규모로는 굳이 그럴 이유는 없으므로 미래를 위해 Presenter 로직만 분리해둠
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DB : Singleton<DB>
    {
        public const string SaveFileName = "LostArk_CalendarBot_SavedDatas.json";
        public static readonly string SAVE_FILE_PATH = AppContext.BaseDirectory + SaveFileName;
        
        [JsonObject(MemberSerialization.OptIn)]
        class DataHolder
        {
            [JsonProperty] public RaidInfoCollection RaidInfoCollection = new RaidInfoCollection();
        }

        #region Shortcuts to DataHolder
        
        public RaidInfoCollection RaidInfoCollection => _dataHolder == null ? null : _dataHolder.RaidInfoCollection;
        
        #endregion
        
        private DataHolder _dataHolder;

        public DB()
        {
            //Initialize Datas
            _dataHolder = new DataHolder();
        }

        public static DB Init()
        {
            //Initialize Presenters
            var dbIns = DB.Ins;
            if (dbIns.Load())
            {
                Console.WriteLine("Loaded Database!");
                Console.WriteLine($"{dbIns._dataHolder.RaidInfoCollection.GetCount()} Raid infos loaded...");
                for (int i = 0; i < dbIns._dataHolder.RaidInfoCollection.GetCount(); i++)
                {
                    var info = dbIns._dataHolder.RaidInfoCollection.ElementAt(i);
                    Console.WriteLine($"\t[{i}] {String.Format("{0,-27}", info.Title)} {info.DateTime.ToString("yyyy-MM-dd HH:mm")}");
                }
            }
            else
            {
                Console.WriteLine("Database load from file failed...");
                Console.WriteLine("Creating new db!");
            }
            ServiceHolder.InitServices(DB.Ins.RaidInfoCollection);
            return dbIns;
        }

        public bool Save()
        {
            try
            {
                var serializedText = JsonConvert.SerializeObject(_dataHolder, Formatting.Indented);
                File.WriteAllText(SAVE_FILE_PATH, serializedText );
                Console.WriteLine($"Saved at {SAVE_FILE_PATH}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!!Exception during saving raid infos data!!!");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public bool Load()
        {
            if (File.Exists(SAVE_FILE_PATH))
            {
                var text = File.ReadAllText(SAVE_FILE_PATH);
                var deserialized = JsonConvert.DeserializeObject<DataHolder>(text);
                if (deserialized != null)
                {
                    _dataHolder = deserialized;
                    return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }
    }
}