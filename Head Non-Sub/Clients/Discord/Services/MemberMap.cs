using System;
using System.IO;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MapEntities = HeadNonSub.Entities.Discord.MemberMap;

namespace HeadNonSub.Clients.Discord.Services {

    public class MemberMap {

        private readonly SocketCommandContext _Context;

        public MemberMap(SocketCommandContext context) => _Context = context;

        /// <summary>
        /// Generate the member map.
        /// </summary>
        /// <returns>Path to the generated json file.</returns>
        public string Generate() {
            MapEntities.MemberMap map = new MapEntities.MemberMap();

            foreach (SocketGuildUser user in _Context.Guild.Users) {
                MapEntities.Member member = new MapEntities.Member {
                    Id = user.Id,
                    Username = user.ToString(),
                    Nickname = user.Nickname,
                    Created = user.CreatedAt.UtcDateTime,
                    Joined = user.JoinedAt.HasValue ? user.JoinedAt.Value.UtcDateTime : null as DateTime?,
                    AvatarUrl = user.GetAvatarUrl()
                };

                // Roles
                foreach (SocketRole role in user.Roles) {
                    if (role.Name != "@everyone") {
                        member.Roles.Add(new MapEntities.Role {
                            Id = role.Id,
                            Name = role.Name
                        });
                    }
                }

                map.Members.Add(member);
            }

            // Serialize
            try {
                string tempUserFiles = Path.Combine(Constants.TemporaryDirectory, _Context.Guild.Id.ToString(), _Context.User.Id.ToString());
                Directory.CreateDirectory(tempUserFiles);
                string jsonFile = Path.Combine(tempUserFiles, "MemberMap.json");

                if (File.Exists(jsonFile)) {
                    File.Delete(jsonFile);
                }

                string json = JsonConvert.SerializeObject(map, new JsonSerializerSettings() {
                    ContractResolver = new DefaultContractResolver {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    },
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                });

                File.WriteAllText(jsonFile, json);

                if (File.Exists(jsonFile)) {
                    return jsonFile;
                } else {
                    throw new FileNotFoundException("The member map file was not found after generation.");
                }
            } catch (Exception ex) {
                LoggingManager.Log.Error(ex);
                return string.Empty;
            }
        }

    }

}
