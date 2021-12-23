using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Dischatr {
    public class Chatbot {

        #region Fields

        private readonly string _accessToken = string.Empty;
        private DiscordSocketClient _discordClient = null;
        private List<ChatbotCommand> _commands = new List<ChatbotCommand>();

        #endregion

        #region Properties

        /// <summary>
        /// Should the chatbot ignore messages from other bots?
        /// </summary>
        public bool IgnoreBotMessages { get; set; } = true;
        /// <summary>
        /// What string should initiate commands with the chatbot?
        /// </summary>
        public string CommandPrefix { get; set; } = "!";
        /// <summary>
        /// What string should terminate commands with the chatbot?
        /// </summary>
        public string CommandTerminator { get; set; } = ";";
        /// <summary>
        /// What nickname should the chatbot give itself?
        /// </summary>
        public string Nickname { get; set; } = "Dischatr";
        /// <summary>
        /// The mention for the chatbot.
        /// </summary>
        /// <remarks>This value is `null` until after login is complete.</remarks>
        public string Mention => _discordClient?.CurrentUser?.Mention;
        /// <summary>
        /// The home guild ID for the chatbot.
        /// </summary>
        public ulong? HomeGuildId { get; set; } = null;
        /// <summary>
        /// The testing channel ID for the chatbot.
        /// </summary>
        public ulong? TestingChannelId { get; set; } = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance of a chatbot for use with Discord, initiated with the specified access token.
        /// </summary>
        /// <param name="accessToken">The chatbot's access token, provided by Discord.</param>
        public Chatbot(string accessToken) => _accessToken = accessToken;

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the chatbot.
        /// </summary>
        /// <remarks>This method will subscribe to events for the client, login, and run the chatbot's code until application closure.</remarks>
        public void Initialize() {
            OnInitializing();
            _discordClient = new DiscordSocketClient();
            _discordClient.MessageReceived += DiscordMessageReceived;
            _discordClient.Connected += Connected;
            _discordClient.Disconnected += Disconnected;
            _discordClient.JoinedGuild += DiscordClient_JoinedGuild;

            // Login and run the bot.
            LoginAndRunAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Attempt to retrieve command packets from a message.
        /// </summary>
        /// <param name="message">The message to read from.</param>
        /// <param name="commandPackets">The retrieved command packets.</param>
        /// <returns>Returns true if no errors were encountered while retrieving command packets, otherwise false.</returns>
        protected virtual bool TryGetCommandPackets(SocketMessage socketMessage, out IEnumerable<CommandPacket> commandPackets) {
            // Create an object to represent the final collection of packets.
            var packets = new List<CommandPacket>();
            var message = socketMessage.Content;
            commandPackets = null;

            // Get all potential commands.
            try {
                string[] potentialCommands = message.Split(CommandPrefix);
                if (potentialCommands?.Length > 0) {
                    foreach (string potentialCommand in potentialCommands) {

                        // Get the contained command data.
                        string containedCommand = potentialCommand;
                        if (potentialCommand.Contains(CommandTerminator))
                            containedCommand = potentialCommand.Substring(0, potentialCommand.IndexOf(CommandTerminator));

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
                            packets.Add(new CommandPacket(socketMessage, key, arguments.Count > 0 ? arguments.ToArray() : null));
                    }
                }
            } catch (Exception e) {
                OnExceptionOccurred(e);
                return false;
            }

            commandPackets = packets;
            return true;
        }

        #endregion

        #region Private Methods

        private async Task LoginAndRunAsync() {
            OnConnectionStateChanged(ConnectionState.Connecting);
            await _discordClient.LoginAsync(TokenType.Bot, _accessToken);
            OnInitialized();

            DiscoverCommands();

            await _discordClient.StartAsync();
            OnStarted();

            // Wait forever.
            await Task.Delay(-1);
        }
        private Task Connected() {
            OnConnectionStateChanged(ConnectionState.Connected);
            return Task.CompletedTask;
        }
        private Task Disconnected(Exception arg) {
            OnConnectionStateChanged(ConnectionState.Disconnected);
            return Task.CompletedTask;
        }
        private Task DiscordMessageReceived(SocketMessage message) {
            // Let subscribers know that a message was recieved.
            OnMessageReceived(message);

            // Messages from known bots are ignored.
            if (message.Author.IsBot && IgnoreBotMessages)
                return Task.CompletedTask;

            // Attempt to process any commands that may be present.
            if (TryGetCommandPackets(message, out IEnumerable<CommandPacket> commandPackets)) {
                foreach (var commandPacket in commandPackets) {
                    try {
                        if (_commands.Any(command => command.Key.Equals(commandPacket.Key, StringComparison.InvariantCultureIgnoreCase))) {
                            var command = _commands.FirstOrDefault(command => command.Key.Equals(commandPacket.Key, StringComparison.InvariantCultureIgnoreCase));
                            if (command != null)
                                command.Invoke(message, commandPacket.Parameters?.ToArray());
                        }
                    }
                    catch (Exception e) { OnExceptionOccurred(e); }
                }
            }

            return Task.CompletedTask;
        }
        private Task DiscordClient_JoinedGuild(SocketGuild arg) {
            arg.CurrentUser.ModifyAsync(x => x.Nickname = Nickname);
            return Task.CompletedTask;
        }
        private void DiscoverCommands() {
            OnDiscoveringCommands();
            var commandAssembly = Assembly.GetEntryAssembly();
            IEnumerable<Type> commandTypes = commandAssembly.GetTypes().Where(w => !w.IsAbstract && w.GetType() != typeof(ChatbotCommand) && typeof(ChatbotCommand).IsAssignableFrom(w));
            foreach (Type commandType in commandTypes) {
                try {
                    var command = (ChatbotCommand)Activator.CreateInstance(commandType);
                    command.ReplyingWithEmbed += Command_ReplyingWithEmbed;
                    command.ReplyingWithMessage += Command_ReplyingWithMessage;
                    _commands.Add(command);
                } catch (Exception e) { OnExceptionOccurred(e); }
            }

            OnCommandsDiscovered();
        }
        private void Command_ReplyingWithMessage(object sender, ChatbotCommandResponse<string> e) {
            var channel = _discordClient.GetChannel(e.OriginalMessage.Channel.Id) as IMessageChannel;
            channel.SendMessageAsync(e.Data).GetAwaiter().GetResult();
        }
        private void Command_ReplyingWithEmbed(object sender, ChatbotCommandResponse<Embed> e) {
            var channel = _discordClient.GetChannel(e.OriginalMessage.Channel.Id) as IMessageChannel;
            channel.SendMessageAsync(null, false, e.Data).GetAwaiter().GetResult();
        }

        #endregion

        #region Events

        /// <summary>
        /// The chatbot is starting the initialization process.
        /// </summary>
        public event EventHandler Initializing;
        /// <summary>
        /// The chatbot has finished initializing successfully.
        /// </summary>
        public event EventHandler Initialized;
        /// <summary>
        /// The chatbot is starting to discover commands in its executing assembly.
        /// </summary>
        public event EventHandler DiscoveringCommands;
        /// <summary>
        /// The chatbot has successfully identified commands in its executing assembly.
        /// </summary>
        public event EventHandler CommandsDiscovered;
        /// <summary>
        /// The chatbot is officially up and running.
        /// </summary>
        public event EventHandler Started;
        /// <summary>
        /// The chatbot encountered an exception during typical processing.
        /// </summary>
        public event EventHandler<Exception> ExceptionOccurred;
        /// <summary>
        /// The chatbot received a message from Discord.
        /// </summary>
        public event EventHandler<SocketMessage> MessageReceived;
        /// <summary>
        /// The chatbot is getting ready to send a message to Discord.
        /// </summary>
        public event EventHandler<SocketMessage> MessageSending;
        /// <summary>
        /// The chatbot successfully sent a message to Discord.
        /// </summary>
        public event EventHandler<SocketMessage> MessageSent;
        /// <summary>
        /// The state of the chatbot's connection to Discord has changed.
        /// </summary>
        public event EventHandler<ConnectionState> ConnectionStateChanged;

        #endregion

        #region Event Invocation Methods

        protected void OnInitializing() => Initializing?.Invoke(this, new EventArgs());
        protected void OnInitialized() => Initialized?.Invoke(this, new EventArgs());
        protected void OnDiscoveringCommands() => DiscoveringCommands?.Invoke(this, new EventArgs());
        protected void OnCommandsDiscovered() => CommandsDiscovered?.Invoke(this, new EventArgs());
        protected void OnStarted() => Started?.Invoke(this, new EventArgs());
        protected void OnExceptionOccurred(Exception e) => ExceptionOccurred?.Invoke(this, e);
        protected void OnMessageReceived(SocketMessage message) => MessageReceived?.Invoke(this, message);
        protected void OnMessageSending(SocketMessage message) => MessageSending?.Invoke(this, message);
        protected void OnMessageSent(SocketMessage message) => MessageSent?.Invoke(this, message);
        protected void OnConnectionStateChanged(ConnectionState connectionState) => ConnectionStateChanged?.Invoke(this, connectionState);

        #endregion

    }
}
