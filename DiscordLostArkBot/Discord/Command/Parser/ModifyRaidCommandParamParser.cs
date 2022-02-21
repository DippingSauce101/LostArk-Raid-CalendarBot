using System;
using System.Linq;
using System.Text.RegularExpressions;
using DiscordLostArkBot.Utilities;

namespace DiscordLostArkBot.Discord.Command.Parser
{
    internal class ModifyRaidCommandParamParser: ICommandParamParser<ModifyRaidCommandParam>
    {
        public bool Parse(string paramStr, out ModifyRaidCommandParam parsedParam, params object[] parseContext)
        {
            var raidDataIdParsed = ParseRaidDataId(paramStr, out var parsedRaidDataId);
            //title은 파싱 실패해도 노상관!
            var titleParsed = ParseRaidTitle(paramStr, out var parsedRaidTitle);
            var dateTimeParsed = paramStr.ParseParenthesisedDateTimeFromString(out var parsedDateTime);

            parsedParam = new ModifyRaidCommandParam(parsedRaidDataId,
                titleParsed ? parsedRaidTitle : null,
                dateTimeParsed ? parsedDateTime : null);
            return raidDataIdParsed && (dateTimeParsed || titleParsed);
        }

        private bool ParseRaidDataId(string paramStr, out ulong parsedRaidDataId)
        {
            var firstParam = paramStr.Split(' ').First();
            if (string.IsNullOrWhiteSpace(firstParam))
            {
                parsedRaidDataId = 0;
                return false;
            }

            if (ulong.TryParse(firstParam, out parsedRaidDataId)) return true;

            return false;
        }

        private bool ParseRaidTitle(string paramStr, out string parsedTitle)
        {
            var firstParam = paramStr.Split(' ').First();
            if (string.IsNullOrWhiteSpace(firstParam))
            {
                parsedTitle = null;
                return false;
            }

            var titleStart = paramStr.IndexOf(firstParam) + firstParam.Length;

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