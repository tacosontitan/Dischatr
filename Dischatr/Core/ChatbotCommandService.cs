using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dischatr {
    /// <summary>
    /// A simple singleton enabling thread safe access to commands.
    /// </summary>
    public sealed class ChatbotCommandService {

        #region Fields

        private static readonly object _instanceLock = new();
        private static ChatbotCommandService _commandService = null;
        private ChatbotCommandParser _commandReader = null;
        private readonly List<ChatbotCommand> _commands = new();

        #endregion

        #region Properties

        /// <summary>
        /// What string should initiate commands with the chatbot?
        /// </summary>
        public string Prefix { get; set; } = "!";
        /// <summary>
        /// What string should terminate commands with the chatbot?
        /// </summary>
        public string Terminator { get; set; } = ";";
        /// <summary>
        /// The active instance of the command service.
        /// </summary>
        public static ChatbotCommandService Instance {
            get {
                if (_commandService == null)
                    lock (_instanceLock)
                        _commandService = new ChatbotCommandService();

                return _commandService;
            }
        }

        #endregion

        #region Constructor

        private ChatbotCommandService() { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize the command service.
        /// </summary>
        public void Initialize() {
            try {
                _commandReader = new ChatbotCommandParser(Prefix, Terminator);
            } catch (Exception e) { OnExceptionOccurred(e); }
        }
        /// <summary>
        /// Discover any commands that may be present in the entry assembly.
        /// </summary>
        public void Discover() {
            try {
                var commandAssembly = Assembly.GetEntryAssembly();
                var currentAssembly = Assembly.GetExecutingAssembly();
                var types = commandAssembly.GetTypes().Union(currentAssembly.GetTypes());
                IEnumerable<Type> commandTypes = types.Where(w => !w.IsAbstract && typeof(ChatbotCommand).IsAssignableFrom(w));
                foreach (Type commandType in commandTypes) {
                    try {
                        var command = (ChatbotCommand)Activator.CreateInstance(commandType);
                        command.ReplyingWithEmbed += OnEmbedReceived;
                        command.ReplyingWithMessage += OnMessageReceived;
                        _commands.Add(command);
                    } catch (Exception e) { OnExceptionOccurred(e); }
                }
            } catch (Exception e) { OnExceptionOccurred(e); }
        }
        /// <summary>
        /// Process a socket message in order to process any commands that may be present.
        /// </summary>
        /// <param name="message">The message that should be processed by the service.</param>
        public void ProcessSocketMessage(SocketMessage message) {
            try {
                var commandPackets = _commandReader.GetCommandPackets(message);
                if (commandPackets?.Count() > 0)
                    foreach (var commandPacket in commandPackets)
                        ProcessCommandPacket(message, commandPacket);
            } catch (Exception e) { OnExceptionOccurred(e); }
        }

        #endregion

        #region Internal Methods

        internal void GetCommandKeys(out IEnumerable<string> keys) => keys = _commands.Select(s => s.Key);

        #endregion

        #region Private Methods

        private void ProcessCommandPacket(SocketMessage message, ChatbotCommandPacket commandPacket) {
            try {
                if (_commands.Any(command => command.Key.Equals(commandPacket.Key, StringComparison.InvariantCultureIgnoreCase))) {
                    var command = _commands.FirstOrDefault(command => command.Key.Equals(commandPacket.Key, StringComparison.InvariantCultureIgnoreCase));
                    if (command != null)
                        command.Invoke(message, commandPacket.Parameters?.ToArray());
                }
            } catch (Exception e) { OnExceptionOccurred(e); }
        }

        #endregion

        #region Events

        /// <summary>
        /// The command service encountered an exception during processing.
        /// </summary>
        public event EventHandler<Exception> ExceptionOccurred;
        /// <summary>
        /// The command service received a message from one of the loaded commands.
        /// </summary>
        public event EventHandler<ChatbotCommandResponse<string>> MessageReceived;
        /// <summary>
        /// The command service received an embed from one of the loaded commands.
        /// </summary>
        public event EventHandler<ChatbotCommandResponse<Embed>> EmbedReceived;

        #endregion

        #region Event Invocation Methods

        private void OnExceptionOccurred(Exception e) => ExceptionOccurred?.Invoke(this, e);
        private void OnMessageReceived(object sender, ChatbotCommandResponse<string> response) => MessageReceived?.Invoke(this, response);
        private void OnEmbedReceived(object sender, ChatbotCommandResponse<Embed> response) => EmbedReceived?.Invoke(this, response);

        #endregion

    }
}
