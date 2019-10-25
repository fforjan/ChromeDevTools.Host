using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChromeDevTools.Host.AspNetCore
{
    public class WebSockerWrapper : IWebSocket
    {
        public WebSocket WebSocket { get; }

        public WebSockerWrapper(WebSocket webSocket)
        {
            WebSocket = webSocket;
        }

        public Task SendAsync(string contents, CancellationToken token)
        {
            return this.WebSocket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(contents),
                    offset: 0,
                    count: contents.Length),
                messageType: WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: CancellationToken.None);
        }

        public Task CloseAsync(string statusDescription, CancellationToken token)
        {
            return this.WebSocket.CloseAsync(WebSocketCloseStatus.Empty, statusDescription, token);
        }

        public async Task<Receiveinfo> ReceiveAsync(CancellationToken token)
        {
            var buffer = new byte[1024 * 4];

            var request = await this.WebSocket.ReceiveAsync(buffer, CancellationToken.None);

            if (request.CloseStatus.HasValue)
            {
                return new Receiveinfo
                {
                    IsClosed = true,
                    StatusDescription = request.CloseStatusDescription
                };
            }
            else
            {
                return new Receiveinfo
                {
                    IsClosed = false,
                    Content = Encoding.ASCII.GetString(new ArraySegment<byte>(buffer, 0, request.Count))
                };
            }
        }
    }
}