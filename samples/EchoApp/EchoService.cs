namespace EchoApp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using ChromeDevTools.Host;
    using ChromeDevTools.Host.AspNetCore;
    using ChromeDevTools.Host.Handlers.Runtime;

    public class EchoService
    {
        private readonly List<WebSocket> communicationSession = new();
        public async Task Echo(WebSocket webSocket)
        {
            await ChromeHostExtension.Sessions.ForEach(_ => Extensions.GetService<RuntimeHandler>(_).Log("New Connection"));

            communicationSession.Add(webSocket);
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var rawMessage = new ArraySegment<byte>(buffer, 0, result.Count);
                await Task.WhenAll(communicationSession.Select(_ =>
                     _.SendAsync(rawMessage, result.MessageType, result.EndOfMessage, CancellationToken.None)
                ));

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.ASCII.GetString(rawMessage);
                    await ChromeHostExtension.Sessions.ForEach(_ => Extensions.GetService<RuntimeHandler>(_).Log($"Received message: " + message));
                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            this.communicationSession.Remove(webSocket);

            await ChromeHostExtension.Sessions.ForEach(_ => Extensions.GetService<RuntimeHandler>(_).Log("Closed Connection"));

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

    }
}
