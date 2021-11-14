using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using DiscordLostArkBot.Discord;
using Notion.Client;
using Color = Discord.Color;

namespace DiscordLostArkBot.Data
{
    public abstract class RaidInfo
    {
        public ulong ChannelId;
        public DateTime DateTime;
        public ulong MessageId;
        public string NotionCalenderPageId;
        public RaidPlayer[] RaidPlayers;
        public string Title;

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
                            Content = GetNotionRaidPlayerString()
                        },
                        PlainText = GetNotionRaidPlayerString()
                    }
                }
            });
            propertyValues.Add("날짜", new DatePropertyValue
            {
                Date = new Date
                {
                    Start = DateTime,
                    End = DateTime.AddHours(1)
                }
            });
            return propertyValues;
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
            return eb;
        }

        public string GetNotionRaidPlayerString()
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

        private string GetDiscordRaidPlayerString(int index)
        {
            var player = RaidPlayers[index];
            if (player.IsEmpty())
                return $@"{RoleToEmojiString(player.UserRole)}{index + 1}번{RoleToKrString(player.UserRole)}";
            return $@"{RoleToEmojiString(player.UserRole)}<@{player.UserId}>";
        }

        private string GetNotionRaidPlayerString(int index)
        {
            var player = RaidPlayers[index];
            if (player.IsEmpty())
                return $@"{RoleToEmojiString(player.UserRole)}{index + 1}번{RoleToKrString(player.UserRole)}";
            return $@"{RoleToEmojiString(player.UserRole)}{player.UserName}";
        }

        public static string RoleToKrString(RaidPlayer.Role role)
        {
            return role == RaidPlayer.Role.Deal ? "딜러" : "서포터";
        }

        public static string RoleToEmojiString(RaidPlayer.Role role)
        {
            return role == RaidPlayer.Role.Deal ? Db.EmojiSwordCrossed : Db.EmojiShield;
        }

        public static RaidPlayer.Role EmojiStringToRole(string emojiStr)
        {
            if (emojiStr.Equals(Db.EmojiShield))
                return RaidPlayer.Role.Support;
            if (emojiStr.Equals(Db.EmojiSwordCrossed))
                return RaidPlayer.Role.Deal;
            throw new NotImplementedException();
        }

        public abstract bool AddOrChangePlayerRole(ulong userId, RaidPlayer.Role role);
        public abstract bool RemovePlayerRole(ulong userId, RaidPlayer.Role role);
        public abstract bool IsRoleFull(RaidPlayer.Role role);

        public bool UserAlreadySeated(ulong userId)
        {
            var player = RaidPlayers.Where(player => { return player.UserId == userId; }).FirstOrDefault();
            if (player == null) return false;
            return true;
        }

        public RaidPlayer.Role GetUserRole(ulong userId)
        {
            var player = RaidPlayers.Where(player => { return player.UserId == userId; }).FirstOrDefault();
            return player.UserRole;
        }

        public abstract int GetEmptySeatIndex(RaidPlayer.Role role);

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

        public class RaidPlayer
        {
            public enum Role
            {
                Deal,
                Support
            }

            public const ulong UserEmpty = 0;
            public ulong UserId = 0;
            public Role UserRole;

            public string UserName => DiscordBotClient.Ins.Client.GetUser(UserId).Username;

            public bool IsEmpty()
            {
                return UserId == UserEmpty;
            }
        }
    }
}