using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using DiscordLostArkBot.Constants;
using DiscordLostArkBot.Discord;
using DiscordLostArkBot.Notion;
using DiscordLostArkBot.Utilities;
using Newtonsoft.Json;
using Notion.Client;
using Color = Discord.Color;

namespace DiscordLostArkBot.Model.RaidInfo
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RaidInfo
    {
        /// <summary>
        ///     8인 레이드 역할분담
        /// </summary>
        public static readonly RaidPlayer.Role[] EightRaidRoles =
        {
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Support,
            RaidPlayer.Role.Support
        };

        /// <summary>
        ///     4인 레이드 역할분담
        /// </summary>
        public static readonly RaidPlayer.Role[] FourRaidRoles =
        {
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Support
        };

        [JsonProperty] public ulong DataId;
        [JsonProperty] public DiscordKey DiscordMessageKey;
        [JsonProperty] public ulong DiscordMessageThreadId;
        [JsonProperty] public ulong LeaderDiscordUserId;
        [JsonProperty] public string NotionCalenderPageId;
        [JsonProperty] public DateTime RaidDateTimeUtc;
        [JsonProperty] public RaidPlayer[] RaidPlayers;
        [JsonProperty] public string Title;

        /// <summary>
        ///     Json Serializer의 경우 Paramless Constructer를 필요로 한다.
        ///     따라서 Create 함수를 쓰고, private로 생성을 막아두자.
        /// </summary>
        private RaidInfo()
        {
        }

        /// <summary>
        ///     Extension 없이, Discord Key와 Raid DateTime을 키값으로 한 파일명 생성
        /// </summary>
        /// <returns></returns>
        public string GetRaidFileName()
        {
            return $"{DataId}";
        }

        public static RaidInfo Create(int playerCount, ulong dataId, params RaidPlayer.Role[] roles)
        {
            var info = new RaidInfo();
            if (playerCount != roles.Length) throw new InvalidDataException("레이드 유저 숫자의 역할 데이터와 유저 수 데이터가 일치하지 않습니다!");

            info.RaidPlayers = new RaidPlayer[playerCount];
            for (var i = 0; i < playerCount; i++)
                info.RaidPlayers[i] = new RaidPlayer
                {
                    UserRole = roles[i]
                };

            info.DataId = dataId;

            return info;
        }

        public string GetDiscordTitleMessage()
        {
            return $"{RaidPlayers.Length}인 레이드 / 공대장: {LeaderDiscordUserId.DiscordUserIdToRefString()}";
        }

        public EmbedBuilder GetEmbedBuilder()
        {
            var eb = new EmbedBuilder();
            eb.Color = Color.Blue;
            eb.Title = Title;
            var description = string.Empty;
            for (var i = 0; i < RaidPlayers.Length; i++)
            {
                description += GetDiscordRaidPlayerString(i);
                if (i != RaidPlayers.Length - 1)
                {
                    description += Environment.NewLine;
                    description += Environment.NewLine;
                }
            }

            eb.Description = description;

            // eb.AddField("딜러", GetFilledRoleCount(RaidPlayer.Role.Deal) + "/" + GetRoleSeatCount(RaidPlayer.Role.Deal),
            //     true);
            // eb.AddField("서포터",
            //     GetFilledRoleCount(RaidPlayer.Role.Support) + "/" + GetRoleSeatCount(RaidPlayer.Role.Support), true);

            eb.AddField("날짜", RaidDateTimeUtc.UtcToKst().ToString(@"yy\/MM\/dd (ddd요일) HH:mm"));
            eb.AddField("수정 코드", DataId.ToString());

            //eb.Timestamp = RaidDateTimeUtc;

            return eb;
        }

        public ComponentBuilder GetComponentBuilder()
        {
            var cb = new ComponentBuilder()
                .WithButton("일정확인", style: ButtonStyle.Link, url: Settings.NotionCalendarUrl);
            return cb;
        }

        public string GetDiscordRaidPlayerString(int index)
        {
            var player = RaidPlayers[index];
            if (player.IsEmpty())
                return
                    $@"{RaidEmoji.RoleToEmojiString(player.UserRole)}{index + 1}번{RaidEmoji.RoleToKrString(player.UserRole)}";
            return $@"{RaidEmoji.RoleToEmojiString(player.UserRole)}{player.UserId.DiscordUserIdToRefString()}";
        }

        public string GetNotionRaidPlayerListString()
        {
            var str = string.Empty;
            for (var i = 0; i < RaidPlayers.Length; i++)
            {
                str += GetNotionRaidPlayerString(i);
                if (i < RaidPlayers.Length - 1)
                    str += ", ";
            }

            return str;
        }

        public string GetNotionRaidPlayerString(int index)
        {
            var player = RaidPlayers[index];
            if (player.IsEmpty())
                return
                    $@"{RaidEmoji.RoleToEmojiString(player.UserRole)}{index + 1}번{RaidEmoji.RoleToKrString(player.UserRole)}";
            return $@"{RaidEmoji.RoleToEmojiString(player.UserRole)}{player.CachedUserName}";
        }

        public async Task RefreshUserCache()
        {
            foreach (var raidPlayer in RaidPlayers) await raidPlayer.RefreshUserInfoFromDiscord();
        }

        public Dictionary<string, PropertyValue> GetNotionPageProperties()
        {
            var propertyValues = new Dictionary<string, PropertyValue>();
            propertyValues.Add("title", new TitlePropertyValue
            {
                Title = new List<RichTextBase>
                {
                    new RichTextText
                    {
                        Type = RichTextType.Text,
                        Text = new Text
                        {
                            Content = Title
                        },
                        PlainText = Title
                    }
                }
            });
            propertyValues.Add("레이드명", new RichTextPropertyValue
            {
                RichText = new List<RichTextBase>
                {
                    new RichTextText
                    {
                        Type = RichTextType.Text,
                        Text = new Text
                        {
                            Content = Title
                        },
                        PlainText = Title
                    }
                }
            });
            propertyValues.Add("수정코드", new RichTextPropertyValue
            {
                RichText = new List<RichTextBase>
                {
                    new RichTextText
                    {
                        Type = RichTextType.Text,
                        Text = new Text
                        {
                            Content = DataId.ToString()
                        },
                        PlainText = DataId.ToString()
                    }
                }
            });
            propertyValues.Add("참가자", new RichTextPropertyValue
            {
                RichText = new List<RichTextBase>
                {
                    new RichTextText
                    {
                        Type = RichTextType.Text,
                        Text = new Text
                        {
                            Content = GetNotionRaidPlayerListString()
                        },
                        PlainText = GetNotionRaidPlayerListString()
                    }
                }
            });

            var kstRaidDateTime = RaidDateTimeUtc.UtcToKst();
            propertyValues.Add("날짜", new TimeZoneDatePropertyValue
            {
                Date = new TimeZoneDate
                {
                    Start = kstRaidDateTime.ToIso8601(),
                    End = kstRaidDateTime.AddHours(1).ToIso8601()
                }
            });
            return propertyValues;
        }

        public bool UserAlreadySeated(ulong userId)
        {
            var player = RaidPlayers.Where(player => { return player.UserId == userId; }).FirstOrDefault();
            if (player == null) return false;
            return true;
        }

        public bool UserAlreadySeatedWithRole(ulong userId, RaidPlayer.Role role)
        {
            return GetUserRole(userId) == role;
        }

        public RaidPlayer.Role GetUserRole(ulong userId)
        {
            var player = RaidPlayers.Where(player => { return player.UserId == userId; }).FirstOrDefault();
            if (player != null) return player.UserRole;

            return RaidPlayer.Role.Error;
        }

        public bool IsRoleFull(RaidPlayer.Role role)
        {
            var emptyRolesCount = RaidPlayers.Where(raidPlayer =>
            {
                return raidPlayer.UserRole == role && raidPlayer.IsEmpty();
            }).Count();
            return emptyRolesCount == 0;
        }

        public int GetEmptySeatIndex(RaidPlayer.Role role)
        {
            for (var i = 0; i < RaidPlayers.Length; i++)
                if (RaidPlayers[i].UserRole == role &&
                    RaidPlayers[i].IsEmpty())
                    return i;

            return -1;
        }

        public int GetRoleSeatCount(RaidPlayer.Role role)
        {
            return RaidPlayers.Where(player => { return player.UserRole == role; })
                .Count();
        }

        public int GetFilledRoleCount(RaidPlayer.Role role)
        {
            return RaidPlayers.Where(player =>
                {
                    return player.UserRole == role &&
                           player.UserId != RaidPlayer.UserEmpty;
                })
                .Count();
        }

        [JsonObject(MemberSerialization.OptIn)]
        public struct DiscordKey
        {
            [JsonProperty] public ulong ChannelId;
            [JsonProperty] public ulong MessageId;

            public DiscordKey(ulong channelId, ulong messageId)
            {
                ChannelId = channelId;
                MessageId = messageId;
            }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class RaidPlayer
        {
            public enum Role
            {
                Error,
                Deal,
                Support
            }

            public const ulong UserEmpty = 0;

            [JsonProperty] public ulong UserId;
            [JsonProperty] public Role UserRole;

            //BugFix - GetUser()의 경우 캐싱이 안 되어 있을 수 있음!! Async 작동이 필요하다!!
            [field: JsonProperty] public string CachedUserName { get; private set; }

            public async Task<string> RefreshUserInfoFromDiscord()
            {
                if (UserId == 0 || UserRole == Role.Error) return null;
                var cachedUser = DiscordBotClient.Ins.Client.GetUser(UserId);
                if (cachedUser != null)
                {
                    CachedUserName = cachedUser.Username;
                }
                else
                {
                    var user = await DiscordBotClient.Ins.Client.GetUserAsync(UserId);
                    if (user != null) CachedUserName = user.Username;
                }

                return CachedUserName;
            }

            public bool IsEmpty()
            {
                return UserId == UserEmpty;
            }
        }
    }
}