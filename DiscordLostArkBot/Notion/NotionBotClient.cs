using System;
using System.Threading.Tasks;
using DiscordLostArkBot.Data;
using DiscordLostArkBot.Setting;
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

        public async Task CreatePage(RaidInfo raidInfo)
        {
            var pageProperties = raidInfo.GetNotionPageProperties();
            var pageCreateParams = new PagesCreateParameters();
            pageCreateParams.Parent = _calendarDbParent;
            pageCreateParams.Properties = pageProperties;
            var createdPage = await _client.Pages.CreateAsync(pageCreateParams);
            raidInfo.NotionCalenderPageId = createdPage.Id;
            Console.WriteLine("CreateAsync Notion!");
        }

        public async Task UpdatePage(RaidInfo raidInfo)
        {
            var pageProperties = raidInfo.GetNotionPageProperties();
            await _client.Pages.UpdatePropertiesAsync(raidInfo.NotionCalenderPageId, pageProperties);
        }

        public async Task DeletePage(RaidInfo raidInfo)
        {
            await _client.Blocks.DeleteAsync(raidInfo.NotionCalenderPageId);
        }
    }
}