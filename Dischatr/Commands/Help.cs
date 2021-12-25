using Discord;
using Discord.WebSocket;

namespace Dischatr.Commands {
    public sealed class HelpCommand : ChatbotCommand {
        public HelpCommand() : base("help") { }
        public override void Invoke(SocketMessage message, string[] parameters) {
            var embedBuilder = new EmbedBuilder {
                Color = Color.Red,
                Title = $"Dischatr Help",
                Description = @$"Oh, umm, hello there! 👋

Sorry that you're having trouble using me. If it's a list of available commands you're after, then you can use the `list` command to get that information. If that's not what you're looking for, then this bot was built with Dischatr. You can find more information on the official GitHub by clicking this embedded message.",
                Url = "https://github.com/tacosontitan/Dischatr/wiki"
            };
            Reply(message, embedBuilder.Build());
        }
    }
}