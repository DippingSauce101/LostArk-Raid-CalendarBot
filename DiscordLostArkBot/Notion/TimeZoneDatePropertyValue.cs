using Newtonsoft.Json;
using Notion.Client;

namespace DiscordLostArkBot.Notion
{
    /// <summary>
    ///     Date property value object with ISO 8601 datetime.
    /// </summary>
    public class TimeZoneDatePropertyValue : PropertyValue
    {
        public override PropertyValueType Type => PropertyValueType.Date;

        /// <summary>
        ///     Date
        /// </summary>
        [JsonProperty("date")]
        public TimeZoneDate Date { get; set; }
    }

    /// <summary>
    ///     Date value object with ISO 8601 datetime.
    /// </summary>
    public class TimeZoneDate
    {
        /// <summary>
        ///     Start date with optional time.
        /// </summary>
        [JsonProperty("start")]
        public string Start { get; set; }

        /// <summary>
        ///     End date with optional time.
        /// </summary>
        [JsonProperty("end")]
        public string End { get; set; }
    }
}