using Discord;
using Discord.WebSocket;
using System;
using System.Linq;

namespace Dischatr.Commands {
    public class SupportCommand : ChatbotCommand {

        /// <summary>
        /// Creates a new instance of the command for invocation using the specified key.
        /// </summary>
        public SupportCommand() : base("support") { }
        /// <summary>
        /// This method is where the magic of the command happens.
        /// </summary>
        /// <param name="message">This is the original message, as received from Discord.</param>
        /// <param name="parameters">This is an inferred collection of parameters received from the incoming message.</param>
        public override void Invoke(SocketMessage message, string[] parameters) {
            var patreon = new EmbedFieldBuilder { Name = "Patreon", Value = "The best way to support the developer is to [become a patron](https://www.patreon.com/tacosontitan).", IsInline = false };
            var coffee = new EmbedFieldBuilder { Name = "Buy Me a Coffee", Value = "Relationships are great, but [the developer likes coffee](https://www.buymeacoffee.com/tacosontitan)!", IsInline = false };
            var twitter = new EmbedFieldBuilder { Name = "Twitter", Value = "Stay up to date with what the developer is doing, [here](https://twitter.com/tacosontitan).", IsInline = false };
            var embedBuilder = new EmbedBuilder() {
                Title = "Support Dischatr",
                Description = "You can show your love and appreciation for the framework that made this chatbot possible in the various ways below!",
                Url = "https://github.com/tacosontitan/Dischatr/wiki/Support",
                Color = Color.Green,
                Fields = new EmbedFieldBuilder[] { patreon, coffee, twitter }.ToList(),
                Footer = new EmbedFooterBuilder { Text = "Thank you for consideration!", IconUrl = "" },
                Timestamp = DateTime.Now
            };

            // Show support for Dischatr.
            Reply(message, embedBuilder.Build());

            // Show support for the derived bot.
            var derivedEmbed = GenerateDerivedSupportEmbed();
            if (derivedEmbed != null)
                Reply(message, derivedEmbed);
        }
        protected virtual Embed GenerateDerivedSupportEmbed() => null;
    }
}