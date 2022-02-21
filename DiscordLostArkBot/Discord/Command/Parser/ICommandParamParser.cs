namespace DiscordLostArkBot.Discord.Command.Parser
{
    internal interface ICommandParamParser<T> where T : ICommandParam
    {
        bool Parse(string paramStr, out T parsedParam, params object[] parseContext);
    }
}