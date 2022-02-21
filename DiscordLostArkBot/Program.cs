using System;
using System.IO;
using System.Threading.Tasks;
using DiscordLostArkBot.Constants;
using DiscordLostArkBot.Discord;
using DiscordLostArkBot.Model;
using DiscordLostArkBot.Utilities;

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
            if (File.Exists("Tokens.ini") == false)
            {
                Console.WriteLine($"실행 파일 경로 {AppDomain.CurrentDomain.BaseDirectory}에 Tokens.ini 파일을 생성해 주세요.");
                return;
            }

            var ini = new IniFile();
            ini.Load("Tokens.ini");
            if (Settings.Load(ini) == false)
            {
                Console.WriteLine("잘못된 토큰 값이 감지되었습니다. Tokens.ini 파일 내용물을 확인해 주세요.");
                return;
            }
            
            Console.WriteLine($"Calendar url: {Settings.NotionCalendarUrl}");

            new Program().ProgramMain().GetAwaiter().GetResult();
        }

        public async Task ProgramMain()
        {
            DB.Init();
            var discordBot = DiscordBotClient.Ins;
            await discordBot.RunClient();
            await Task.Delay(-1);
        }
    }
}