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
            new Program().ProgramMain().GetAwaiter().GetResult();
        }

        public async Task ProgramMain()
        {
            DB.Init();
            var discordBot = DiscordBotClient.Ins;
            await discordBot.RunClient();
        }
    }
}