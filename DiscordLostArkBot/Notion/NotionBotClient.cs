using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordLostArkBot.Constants;
using DiscordLostArkBot.Model.RaidInfo;
using DiscordLostArkBot.Presenter;
using DiscordLostArkBot.Utilities;
using Notion.Client;

namespace DiscordLostArkBot.Notion
{
    public class NotionBotClient : Singleton<NotionBotClient>
    {
        private readonly DatabaseParentInput _calendarDbParent;
        private readonly NotionClient _client;

        public NotionBotClient()
        {
            _client = NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = Settings.NotionApiAuthToken
            });

            _calendarDbParent = new DatabaseParentInput();
            _calendarDbParent.DatabaseId = Settings.NotionCalendarDbId;
        }

        public async Task CreatePage(RaidInfo.DiscordKey discordKey, Dictionary<string, PropertyValue> pageProperties)
        {
            var pageCreateParams = new PagesCreateParameters();
            pageCreateParams.Parent = _calendarDbParent;
            pageCreateParams.Properties = pageProperties;
            var createdPage = await _client.Pages.CreateAsync(pageCreateParams);
            var saved = Presenters.RaidInfo.SetNotionCalendarPageId(discordKey, createdPage.Id);
            if (saved)
            {
                Console.WriteLine("CreateAsync Notion!");
            }
            else
            {
                Console.WriteLine("Save notion calander page key failed! Maybe raid info data lost or calendar key is invalid?");
            }
        }

        public async Task UpdatePage(string notionCalendarPageId, Dictionary<string, PropertyValue> pageProperties)
        {
            await _client.Pages.UpdatePropertiesAsync(notionCalendarPageId, pageProperties);
        }

        public async Task DeletePage(string notionCalendarPageId)
        {
            await _client.Blocks.DeleteAsync(notionCalendarPageId);
        }
    }
}