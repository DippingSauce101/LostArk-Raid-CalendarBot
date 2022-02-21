using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Discord;
using DiscordLostArkBot.Service;
using DiscordLostArkBot.Utilities;

namespace DiscordLostArkBot.Discord.Command.Parser
{
    internal class ModifyRaidInThreadCommandParamParser : ICommandParamParser<ModifyRaidCommandParam>
    {
        public bool Parse(string paramStr, out ModifyRaidCommandParam parsedParam, params object[] parseContext)
        {
            if (parseContext == null || parseContext.Length == 0 || ((parseContext[0] is IThreadChannel) == false))
                throw new InvalidDataException("Parse Context로 IThreadChannel이 필요합니다!");
            
            var raidDataIdParsed = GetRaidDataIdFromThreadChannel((IThreadChannel)parseContext[0], out var raidDataId);
            //title은 파싱 실패해도 노상관!
            var titleParsed = ParseRaidTitle(paramStr, out var parsedRaidTitle);
            var dateTimeParsed = paramStr.ParseParenthesisedDateTimeFromString(out var parsedDateTime);

            parsedParam = new ModifyRaidCommandParam(raidDataId,
                titleParsed ? parsedRaidTitle : null,
                dateTimeParsed ? parsedDateTime : null);
            return raidDataIdParsed && (dateTimeParsed || titleParsed);
        }
        
        private bool GetRaidDataIdFromThreadChannel(IThreadChannel channel, out ulong parsedRaidDataId)
        {
            if (channel != null)
            {
                var keyFromThreadUid = ServiceHolder.RaidInfo.DiscordKeyFromThreadUid(channel.Id);
                parsedRaidDataId = ServiceHolder.RaidInfo.GetRaidId(keyFromThreadUid);
                return true;
            }
            else
            {
                parsedRaidDataId = 0;
                return false;
            }
        }

        private bool ParseRaidTitle(string paramStr, out string parsedTitle)
        {
            var firstParam = paramStr.Split(' ').First();
            if (string.IsNullOrWhiteSpace(firstParam))
            {
                parsedTitle = null;
                return false;
            }

            var titleStart = paramStr.IndexOf(firstParam);

            var parenRegEx = new Regex(@"\(([^)]*)\)");
            var dateTimeParsed = false;
            string dateTimeParenthesised = null;
            foreach (Match match in parenRegEx.Matches(paramStr))
            {
                dateTimeParenthesised = match.Value;
                var dateTimeStr = match.Value.Substring(1, match.Value.Length - 2);
                if (dateTimeStr.ParseToDateTime(out var parsedDateTime))
                {
                    dateTimeParsed = true;
                    break;
                }
            }

            var titleEnd = paramStr.Length;
            if (dateTimeParsed)
            {
                titleEnd = paramStr.IndexOf(dateTimeParenthesised);
                if (titleEnd < titleStart)
                {
                    parsedTitle = null;
                    return false;
                }
            }

            parsedTitle = paramStr.Substring(titleStart, titleEnd - titleStart).Trim();
            return true;
        }
    }
}