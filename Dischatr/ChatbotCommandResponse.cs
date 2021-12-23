using Discord.WebSocket;

namespace Dischatr {
    public class ChatbotCommandResponse<T> {
        public T Data { get; }
        public SocketMessage OriginalMessage { get; }
        public ChatbotCommandResponse(SocketMessage originalMessage, T data) {
            OriginalMessage = originalMessage;
            Data = data;
        }
    }
}
