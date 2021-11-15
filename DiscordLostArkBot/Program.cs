using System.Threading.Tasks;
using DiscordLostArkBot.Discord;
using DiscordLostArkBot.Model;

namespace DiscordLostArkBot
{
    internal class Program
    {
        /// <summary>
        ///     프로그램의 진입점
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            //Ins 호출하여 초기화
            DB.Init();
            new Program().ProgramMain().GetAwaiter().GetResult();
        }

        public async Task ProgramMain()
        {
            var discordBot = DiscordBotClient.Ins;
            await discordBot.RunClient();
        }
    }
}