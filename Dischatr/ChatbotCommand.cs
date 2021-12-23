using Discord;
using Discord.WebSocket;
using System;

namespace Dischatr {
    public abstract class ChatbotCommand {

        #region Properties

        public string Key { get; } = string.Empty;
        public string Description { get; } = string.Empty;

        #endregion

        #region Constructor

        public ChatbotCommand(string key) => Key = key;

        #endregion

        #region Public Methods

        public abstract void Invoke(SocketMessage message, string[] parameters);

        #endregion

        #region Protected Methods

        protected virtual void React(SocketMessage message, IEmote reactionEmote) => message.AddReactionAsync(reactionEmote);
        protected virtual void Reply(SocketMessage message, string response) => OnReplyingWithMessage(new ChatbotCommandResponse<string>(message, response));
        protected virtual void Reply(SocketMessage message, Embed response) => OnReplyingWithEmbed(new ChatbotCommandResponse<Embed>(message, response));

        #endregion

        #region Events

        public event EventHandler<ChatbotCommandResponse<string>> ReplyingWithMessage;
        public event EventHandler<ChatbotCommandResponse<Embed>> ReplyingWithEmbed;

        #endregion

        #region Event Invocations

        protected void OnReplyingWithMessage(ChatbotCommandResponse<string> response) => ReplyingWithMessage(this, response);
        protected void OnReplyingWithEmbed(ChatbotCommandResponse<Embed> response) => ReplyingWithEmbed(this, response);

        #endregion

    }
}
