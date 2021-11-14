using System;
using System.Threading.Tasks;
using Discord;

namespace DiscordLostArkBot.Discord
{
    public static class CacheableExtensions
    {
        public static async Task<IUserMessage> GetUserMessageAsync(this Cacheable<IUserMessage, ulong> message)
        {
            IUserMessage userMessage;
            if (message.HasValue == false)
                userMessage = await message.GetOrDownloadAsync();
            else
                userMessage = message.Value;

            if (userMessage == null) Console.WriteLine("Message download failed!");

            return userMessage;
        }
        
        public static async Task<IMessage> GetMessageAsync(this Cacheable<IMessage, ulong> cacheableMessage)
        {
            IMessage message;
            if (cacheableMessage.HasValue == false)
                message = await cacheableMessage.GetOrDownloadAsync();
            else
                message = cacheableMessage.Value;

            if (message == null) Console.WriteLine("Message download failed!");
            return message;
        }
    }
}