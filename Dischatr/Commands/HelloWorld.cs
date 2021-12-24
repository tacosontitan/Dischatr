using Discord.WebSocket;

namespace Dischatr {
    public sealed class HelloWorldCommand : ChatbotCommand {
        public HelloWorldCommand() : base("hello") { }
        public override void Invoke(SocketMessage message, string[] parameters) => Reply(message, $"Hello {message.Author.Mention}!");
    }
}