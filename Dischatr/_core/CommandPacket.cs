using Discord.WebSocket;
using System.Collections.Generic;

namespace Dischatr {
    public class CommandPacket {
        public string Key { get; set; }
        public IEnumerable<string> Parameters { get; set; }
        public SocketMessage OriginalMessage { get; set; }
        public CommandPacket(SocketMessage originalMessage, string key, params string[] parameters) {
            OriginalMessage = originalMessage;
            Key = key;
            Parameters = parameters;
        }
    }
}
