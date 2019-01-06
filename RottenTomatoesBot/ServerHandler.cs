using System;

namespace RottenTomatoes
{
    public class ServerHandler : IEquatable<ServerHandler>
    {
        public ulong GuildID { get; set; }
        public RottenTomatoesHandler Handler { get; set; }

        public ServerHandler (ulong guildID, RottenTomatoesHandler handler)
        {
            GuildID = guildID;
            Handler = handler;
        }

        public bool Equals(ServerHandler other)
        {
            if (GuildID == other.GuildID)
                return true;
            return false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ServerHandler);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}