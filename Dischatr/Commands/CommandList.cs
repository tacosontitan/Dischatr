using Discord;
using Discord.WebSocket;
using System.Collections.Generic;

namespace Dischatr.Commands {
    public sealed class CommandListCommand : ChatbotCommand {
        public CommandListCommand() : base("list") { }
        public override void Invoke(SocketMessage message, string[] parameters) {
            ChatbotCommandService.Instance.GetCommandKeys(out IEnumerable<string> commandKeys);
            var embedBuilder = new EmbedBuilder {
                Color = Color.Red,
                Title = $"Dischatr Commands",
                Description = @$"You need a list of commands? I got you fam! Prefix any of the following with `{ChatbotCommandService.Instance.Prefix}` and you're ready to go!

```{string.Join('\n', commandKeys)}```"
            };
            Reply(message, embedBuilder.Build());
        }
    }
}