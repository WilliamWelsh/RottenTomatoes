using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace RottenTomatoes
{
    /// <summary>
    /// A list of active server handlers
    /// </summary>
    public static class ActiveServerHandlers
    {
        // List of server handlers
        public static List<ServerHandler> Servers;

        // Constructor
        static ActiveServerHandlers() => Servers = new List<ServerHandler>();

        // Get a server's server handler
        public static ServerHandler GetServerHandler(ulong guildID)
        {
            // Search our list for the server we want
            var result = from server in Servers
                         where server.GuildID == guildID
                         select server;

            var serverHandler = result.FirstOrDefault();

            // Return i if it's not null
            if (serverHandler != null) return serverHandler;

            // If it's null, create a new one, add it to the list, and return it
            serverHandler = new ServerHandler
            {
                GuildID = guildID,
                SearchHandler = new SearchHandler()
            };
            Servers.Add(serverHandler);

            return serverHandler;
        }
    }
}