using Discord.WebSocket;
using System.Reflection;

namespace Dischatr {
    public sealed class VersionCommand : ChatbotCommand {
        public VersionCommand() : base("version") { }
        public override void Invoke(SocketMessage message, string[] parameters) {
            var actualBotVersion = Assembly.GetEntryAssembly().GetName().Version.ToString();
            var dischatrVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Reply(message, $"```Bot Version {actualBotVersion}\nDischatr Version: {dischatrVersion}```");
        }
    }
}