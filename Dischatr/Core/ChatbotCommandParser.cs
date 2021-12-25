using Discord.WebSocket;
using System.Collections.Generic;

namespace Dischatr {
    public sealed class ChatbotCommandParser {

        #region Fields

        private readonly string _commandPrefix;
        private readonly string _commandTerminator;

        #endregion

        #region Constructor

        public ChatbotCommandParser(string commandPrefix, string commandTerminator) {
            _commandPrefix = commandPrefix;
            _commandTerminator = commandTerminator;
        }

        #endregion

        #region Public Methods

        public IEnumerable<ChatbotCommandPacket> GetCommandPackets(SocketMessage message) {
            // Create an object to represent the final collection of packets.
            var packets = new List<ChatbotCommandPacket>();
            var messageContent = message.Content;

            // Get all potential commands.
            string[] potentialCommands = messageContent.Split(_commandPrefix);
            if (potentialCommands?.Length > 0) {
                foreach (string potentialCommand in potentialCommands) {

                    // Get the contained command data.
                    string containedCommand = potentialCommand;
                    if (potentialCommand.Contains(_commandTerminator))
                        containedCommand = potentialCommand.Substring(0, potentialCommand.IndexOf(_commandTerminator));

                    // Split on white space to get the command key and parameters.
                    var key = string.Empty;
                    var arguments = new List<string>();
                    string[] commandLineSplitOnWhiteSpace = containedCommand.Split(' ');
                    if (commandLineSplitOnWhiteSpace.Length > 0) {
                        key = commandLineSplitOnWhiteSpace[0];

                        // Get any arguments for the packet.
                        if (commandLineSplitOnWhiteSpace.Length > 1) {
                            string[] rawArguments = containedCommand.Replace(key, string.Empty).Split(',');
                            var cleanArguments = new List<string>();
                            foreach (string rawArgument in rawArguments)
                                cleanArguments.Add(rawArgument.Trim());

                            arguments = cleanArguments;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(key))
                        packets.Add(new ChatbotCommandPacket(message, key, arguments.Count > 0 ? arguments.ToArray() : null));
                }
            }

            return packets;
        }

        #endregion

    }
}
