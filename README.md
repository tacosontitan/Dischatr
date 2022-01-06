# Rapid Chatbot Creation
Dischatr is a minimal library intended to assist with the rapid creation of chatbots for use with Discord. It enables:

 - Rapid command creation.
 - Custom command prefixing.
 - Automated nicknaming.
 - TODO: Document all features.

To create a chatbot, you simply need an access token and a basic snippet of code:

```
var accessToken = "MyDiScOrDaCcEsStOkEn";
var chatbot = new Chatbot(accessToken) { Nickname = "My Bot's Nickname" };
chatbot.Initialize();
```

From there, everything else is automated; all commands within your assembly deriving from `ChatbotCommand` are discovered on initialization via reflection and invoked upon a match with the `ChatbotCommand.Key` property.

# Hello World
Creating a command for your chatbot is as easy as deriving from `ChatbotCommand` and overriding the `Invoke(SocketMessage, string[])` method:

```
using Dischatr;
using Discord.WebSocket;
using System;

namespace Sample {
    public sealed class HelloWorldCommand : ChatbotCommand {
        public HelloWorldCommand() : base("hello") { }
        public override void Invoke(SocketMessage message, string[] parameters) => Reply(new(message, "Hello World!"));
    }
}
```
