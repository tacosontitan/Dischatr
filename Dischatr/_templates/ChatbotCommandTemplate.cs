using Dischatr;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace $safeprojectname$ {
    public sealed class $safeitemname$Command : ChatbotCommand {

        /// <summary>
        /// Creates a new instance of the command for invocation using the specified key.
        /// </summary>
        public $safeitemname$Command() : base("key") { }
        /// <summary>
        /// This method is where the magic of the command happens.
        /// </summary>
        /// <param name="message">This is the original message, as received from Discord.</param>
        /// <param name="parameters">This is an inferred collection of parameters received from the incoming message.</param>
        public override void Invoke(SocketMessage message, string[] parameters) {
            // TODO: Do some cool things here.
            // TODO: Reply to messages with messages using `Reply`:
            Reply(message, "Woah, pump the command brakes there chatbot; this command is unimplemented.");
        }
    }
}