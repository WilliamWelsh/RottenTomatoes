using System;

namespace RottenTomatoes
{
    public class ServerHandler : IEquatable<ServerHandler>
    {
        public ulong GuildID { get; }
        public SearchHandler SearchHandler { get; }

        public ServerHandler (ulong guildID, SearchHandler handler)
        {
            GuildID = guildID;
            SearchHandler = handler;
        }

        public bool Equals(ServerHandler other) => GuildID == other.GuildID;
        public override bool Equals(object obj) => Equals(obj as ServerHandler);
        public override int GetHashCode() => 0;
    }
}