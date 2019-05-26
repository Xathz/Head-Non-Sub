using System;
using System.Collections.Generic;
using System.IO;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MapEntities = HeadNonSub.Entities.Discord.ServerMap;

namespace HeadNonSub.Clients.Discord.Services {

    public class ServerMap {

        private SocketCommandContext _Context;

        public ServerMap(SocketCommandContext context) => _Context = context;

        /// <summary>
        /// Generate the server map.
        /// </summary>
        /// <returns>Path to the generated json file.</returns>
        public string Generate() {
            MapEntities.ServerMap map = new MapEntities.ServerMap {
                Id = _Context.Guild.Id,
                Name = _Context.Guild.Name,
                Created = _Context.Guild.CreatedAt.UtcDateTime,
                Owner = new MapEntities.User {
                    Id = _Context.Guild.Owner.Id,
                    Name = _Context.Guild.Owner.ToString(),
                    Created = _Context.Guild.Owner.CreatedAt.UtcDateTime,
                    Joined = _Context.Guild.Owner.JoinedAt.HasValue ? _Context.Guild.Owner.JoinedAt.Value.UtcDateTime : null as DateTime?,
                    AvatarUrl = _Context.Guild.Owner.GetAvatarUrl()
                },
                TotalMembers = _Context.Guild.MemberCount,
                VoiceRegion = _Context.Guild.VoiceRegionId,
                VerificationLevel = _Context.Guild.VerificationLevel.ToString(),
                IconUrl = _Context.Guild.IconUrl
            };

            // Roles
            foreach (SocketRole guildRole in _Context.Guild.Roles) {
                MapEntities.Role role = new MapEntities.Role {
                    Id = guildRole.Id,
                    Name = guildRole.Name,
                    Created = guildRole.CreatedAt.UtcDateTime,
                    Color = guildRole.Color.RawValue == 0 ? "#ffffff" : guildRole.Color.ToString(),
                    Mentionable = guildRole.IsMentionable,
                    Hoisted = guildRole.IsHoisted
                };

                guildRole.Permissions.ToList().ForEach(x => role.Permissions.Add(x.ToString()));

                map.Roles.Add(role);
            }

            // Categories
            foreach (SocketCategoryChannel guildCategory in _Context.Guild.CategoryChannels) {
                MapEntities.Category category = new MapEntities.Category {
                    Id = guildCategory.Id,
                    Name = guildCategory.Name,
                    Created = guildCategory.CreatedAt.UtcDateTime
                };

                // Channel in the category
                foreach (SocketGuildChannel categoryChannel in guildCategory.Channels) {
                    MapEntities.Channel channel = new MapEntities.Channel {
                        Id = categoryChannel.Id,
                        Name = categoryChannel.Name,
                        Created = categoryChannel.CreatedAt.UtcDateTime
                    };

                    if (categoryChannel is SocketTextChannel textChannel) {
                        channel.Type = MapEntities.ChannelType.Text;
                        channel.Topic = textChannel.Topic;
                        channel.NSFW = textChannel.IsNsfw;
                    } else if (categoryChannel is SocketVoiceChannel voiceChannel) {
                        channel.Type = MapEntities.ChannelType.Voice;
                        channel.UserLimit = voiceChannel.UserLimit;
                        channel.Bitrate = voiceChannel.Bitrate;
                    }

                    // Permission overwrites for channel
                    channel.PermissionOverwrites.AddRange(ProcessPermissionOverwrites(categoryChannel.PermissionOverwrites));

                    category.Channels.Add(channel);
                }

                // Permission overwrites for category
                category.PermissionOverwrites.AddRange(ProcessPermissionOverwrites(guildCategory.PermissionOverwrites));

                map.Categories.Add(category);
            }

            // Categoryless Channels
            List<SocketGuildChannel> categoryless = new List<SocketGuildChannel>();
            foreach (SocketGuildChannel guildChannel in _Context.Guild.Channels) {
                if (guildChannel is SocketTextChannel textChannel) {
                    if (textChannel.Category is null) {
                        categoryless.Add(guildChannel);
                    }
                } else if (guildChannel is SocketVoiceChannel voiceChannel) {
                    if (voiceChannel.Category is null) {
                        categoryless.Add(guildChannel);
                    }
                }
            }

            foreach (SocketGuildChannel categorylessChannel in categoryless) {
                MapEntities.Channel channel = new MapEntities.Channel {
                    Id = categorylessChannel.Id,
                    Name = categorylessChannel.Name,
                    Created = categorylessChannel.CreatedAt.UtcDateTime
                };

                if (categorylessChannel is SocketTextChannel textChannel) {
                    channel.Type = MapEntities.ChannelType.Text;
                    channel.Topic = textChannel.Topic;
                    channel.NSFW = textChannel.IsNsfw;
                } else if (categorylessChannel is SocketVoiceChannel voiceChannel) {
                    channel.Type = MapEntities.ChannelType.Voice;
                    channel.UserLimit = voiceChannel.UserLimit;
                    channel.Bitrate = voiceChannel.Bitrate;
                }

                // Permission overwrites
                channel.PermissionOverwrites.AddRange(ProcessPermissionOverwrites(categorylessChannel.PermissionOverwrites));

                map.CategorylessChannels.Add(channel);
            }

            // Emotes
            foreach (GuildEmote guildEmote in _Context.Guild.Emotes) {
                MapEntities.Emote emote = new MapEntities.Emote {
                    Id = guildEmote.Id,
                    Name = guildEmote.Name,
                    Created = guildEmote.CreatedAt.UtcDateTime,
                    Animated = guildEmote.Animated,
                    Url = guildEmote.Url
                };

                SocketGuildUser user = guildEmote.CreatorId.HasValue ? _Context.Guild.GetUser(guildEmote.CreatorId.Value) : null;
                if (user is SocketGuildUser) {
                    emote.CreatorId = user.Id;
                    emote.CreatorName = user.ToString();
                }

                map.Emotes.Add(emote);
            }

            // Serialize
            try {
                string tempUserFiles = Path.Combine(Constants.TemporaryDirectory, _Context.Guild.Id.ToString(), _Context.User.Id.ToString());
                Directory.CreateDirectory(tempUserFiles);
                string jsonFile = Path.Combine(tempUserFiles, "ServerMap.json");

                if (File.Exists(jsonFile)) {
                    File.Delete(jsonFile);
                }

                using (StreamWriter streamWriter = new StreamWriter(jsonFile))
                using (JsonWriter jsonWriter = new JsonTextWriter(streamWriter)) {
                    DefaultContractResolver contractResolver = new DefaultContractResolver {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    };

                    JsonSerializer jsonSerializer = new JsonSerializer() {
                        ContractResolver = contractResolver,
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented
                    };

                    jsonSerializer.Serialize(jsonWriter, map, typeof(MapEntities.ServerMap));
                }

                if (File.Exists(jsonFile)) {
                    return jsonFile;
                } else {
                    throw new FileNotFoundException("The server map file was not found after generation.");
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return string.Empty;
            }
        }

        private List<MapEntities.PermissionOverwrite> ProcessPermissionOverwrites(IReadOnlyCollection<Overwrite> overwrites) {
            List<MapEntities.PermissionOverwrite> processedOverwrites = new List<MapEntities.PermissionOverwrite>();

            foreach (Overwrite overwrite in overwrites) {
                MapEntities.PermissionOverwrite permissionOverwrite = new MapEntities.PermissionOverwrite();

                if (overwrite.TargetType == PermissionTarget.Role) {
                    permissionOverwrite.Target = MapEntities.PermissionTarget.Role;
                    SocketRole targetRole = _Context.Guild.GetRole(overwrite.TargetId);
                    permissionOverwrite.Id = targetRole.Id;
                    permissionOverwrite.Name = targetRole.Name;

                } else if (overwrite.TargetType == PermissionTarget.User) {
                    permissionOverwrite.Target = MapEntities.PermissionTarget.User;
                    SocketGuildUser targetUser = _Context.Guild.GetUser(overwrite.TargetId);
                    permissionOverwrite.Id = targetUser.Id;
                    permissionOverwrite.Name = targetUser.ToString();

                }

                overwrite.Permissions.ToAllowList().ForEach(x => permissionOverwrite.Permissions.Add(x.ToString(), MapEntities.PermissionValue.Allow));
                overwrite.Permissions.ToDenyList().ForEach(x => permissionOverwrite.Permissions.Add(x.ToString(), MapEntities.PermissionValue.Deny));

                processedOverwrites.Add(permissionOverwrite);
            }

            return processedOverwrites;
        }

    }

}
