using System;
using System.Text.RegularExpressions;
using DiscordLostArkBot.Utilities;

namespace DiscordLostArkBot.Discord.Command.Parser
{
    internal class CreateRaidCommandParamParser : ICommandParamParser<CreateRaidCommandParam>
    {
        public bool Parse(string paramStr, out CreateRaidCommandParam parsedParam, params object[] parseContext)
        {
            var parseSuccessed = paramStr.ParseParenthesisedDateTimeFromString(out var parsedDateTime);
            var title = paramStr.ParseTitleWithoutDateTime();
            parsedParam = new CreateRaidCommandParam(title, parsedDateTime);
            return parseSuccessed;
        }
    }
}