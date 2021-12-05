using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using DiscordLostArkBot.Constants;
using DiscordLostArkBot.Discord;
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
        /// 8인 레이드 역할분담
        /// </summary>
        public static readonly RaidPlayer.Role[] EIGHT_RAID_ROLES =
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
        /// 4인 레이드 역할분담
        /// </summary>
        public static readonly RaidPlayer.Role[] FOUR_RAID_ROLES =
        {
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Deal,
            RaidPlayer.Role.Support
        };

        [JsonProperty] public ulong DataID;
        [JsonProperty] public DateTime RaidDateTime;
        [JsonProperty] public DiscordKey DiscordMessageKey;
        [JsonProperty] public string NotionCalenderPageId;
        [JsonProperty] public RaidPlayer[] RaidPlayers;
        [JsonProperty] public string Title;
        [JsonProperty] public ulong LeaderDiscordUserId;

        /// <summary>
        /// Extension 없이, Discord Key와 Raid DateTime을 키값으로 한 파일명 생성
        /// </summary>
        /// <returns></returns>
        public string GetRaidFileName()
        {
            return
                $"{DataID}";
        }

        /// <summary>
        /// Json Serializer의 경우 Paramless Constructer를 필요로 한다.
        /// 따라서 Create 함수를 쓰고, private로 생성을 막아두자.
        /// </summary>
        private RaidInfo()
        {
            
        }

        public static RaidInfo Create(int playerCount, ulong dataId, params RaidPlayer.Role[] roles)
        {
            RaidInfo info = new RaidInfo();
            if (playerCount != roles.Length)
            {
                throw new InvalidDataException("레이드 유저 숫자의 역할 데이터와 유저 수 데이터가 일치하지 않습니다!");
            }
            
            info.RaidPlayers = new RaidPlayer[playerCount];
            for (var i = 0; i < playerCount; i++)
                info.RaidPlayers[i] = new RaidPlayer
                {
                    UserRole = roles[i]
                };

            info.DataID = dataId;

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

            eb.AddField("딜러", GetFilledRoleCount(RaidPlayer.Role.Deal) + "/" + GetRoleSeatCount(RaidPlayer.Role.Deal),
                true);
            eb.AddField("서포터",
                GetFilledRoleCount(RaidPlayer.Role.Support) + "/" + GetRoleSeatCount(RaidPlayer.Role.Support), true);
            eb.AddField("코드", DataID.ToString());
            eb.Timestamp = RaidDateTime;
            
            return eb;
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
            return $@"{RaidEmoji.RoleToEmojiString(player.UserRole)}{player.UserName}";
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
            propertyValues.Add("날짜", new DatePropertyValue
            {
                Date = new Date
                {
                    Start = RaidDateTime,
                    End = RaidDateTime.AddHours(1)
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
            
            [JsonProperty] public ulong UserId = 0;
            [JsonProperty] public Role UserRole;

            public string UserName => DiscordBotClient.Ins.Client.GetUser(UserId).Username;

            public bool IsEmpty()
            {
                return UserId == UserEmpty;
            }
        }
    }
}