namespace EchoApp
{
    using BaristaLabs.ChromeDevTools.Runtime;
    using BaristaLabs.ChromeDevTools.Runtime.Runtime;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a websocket connection to a running chrome instance that can be used to send commands and recieve events.
    ///</summary>
    public class ChromeSession : IDisposable
    {
        private readonly ILogger<ChromeSession> m_logger;

        private readonly ConcurrentDictionary<string, Func<object,object>> m_commandHandlers = new ConcurrentDictionary<string, Func<object,object>>();
        private readonly ConcurrentDictionary<Type, string> m_eventTypeMap = new ConcurrentDictionary<Type, string>();

        private WebSocket m_sessionSocket;

        private long m_currentEventId = 0;

        /// <summary>
        /// Gets or sets the number of milliseconds to wait for a command to complete. Default is 5 seconds.
        /// </summary>
        public int CommandTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new ChromeSession to the specified WS endpoint with the specified logger implementation.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="endpointAddress"></param>
        public ChromeSession(ILogger<ChromeSession> logger, WebSocket webSocket)
        {
            CommandTimeout = 5000;
            m_logger = logger;
            this.m_sessionSocket = webSocket;
        }

        /// <summary>
        /// Sends the specified command and returns the associated command response.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="command"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="throwExceptionIfResponseNotReceived"></param>
        /// <returns></returns>
        public async Task SendEvent<TEvent>(TEvent @event, CancellationToken cancellationToken = default(CancellationToken), int? millisecondsTimeout = null, bool throwExceptionIfResponseNotReceived = true)
            where TEvent : IEvent
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));


            if (EventTypeMap.TryGetMethodNameForType<TEvent>(out var eventName))
            {
                await SendEvent(eventName, JToken.FromObject(@event), cancellationToken, millisecondsTimeout, throwExceptionIfResponseNotReceived);
            }
        }

        /// <summary>
        /// Returns a JToken based on a command created with the specified command name and params.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="params"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="throwExceptionIfResponseNotReceived"></param>
        /// <returns></returns>
        public Task SendEvent(string eventName, JToken @params, CancellationToken cancellationToken = default(CancellationToken), int? millisecondsTimeout = null, bool throwExceptionIfResponseNotReceived = true)
        {
            var message = new
            {
                id = Interlocked.Increment(ref m_currentEventId),
                method = eventName,
                @params = @params
            };

            if (millisecondsTimeout.HasValue == false)
                millisecondsTimeout = CommandTimeout;

            LogTrace("Sending {id} {method}: {params}", message.id, message.method, @params.ToString());

            var contents = JsonConvert.SerializeObject(message);

            return this.m_sessionSocket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(contents),
                                                                  offset: 0,
                                                                  count: contents.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);

        }

        private void LogTrace(string message, params object[] args)
        {
            if (m_logger == null)
                return;

            m_logger.LogTrace(message, args);
        }

        private void LogError(string message, params object[] args)
        {
            if (m_logger == null)
                return;

            m_logger.LogError(message, args);
        }


        #region IDisposable Support
        private bool m_isDisposed = false;

        private void Dispose(bool disposing)
        {
            if (!m_isDisposed)
            {
                if (disposing)
                {

                    m_eventTypeMap.Clear();

                    if (m_sessionSocket != null)
                    {
                        m_sessionSocket = null;
                    }
                }

                m_isDisposed = true;
            }
        }

        /// <summary>
        /// Disposes of the ChromeSession and frees all resources.
        ///</summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        public int Context {get { return 1; } }

        private Task SendContextCreated() {
            // {{
//   "id": 1,
//   "origin": "",
//   "name": "node[54441]",
//   "auxData": {
//     "isDefault": true
//   }
// }}
            return this.SendEvent(new ExecutionContextCreatedEvent {
                Context = new ExecutionContextDescription {
                    Id = this.Context,
                    Origin = "",
                    Name = "virtual context",
                    AuxData = new {
                        isDefault = true
                    }
                }
            });
        }


        public async Task Process(CancellationToken cancellationToken)
        {
            await this.SendContextCreated();

            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult request = await this.m_sessionSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            while (!request.CloseStatus.HasValue)
            {
                var message = Encoding.ASCII.GetString(buffer.Take(request.Count).ToArray());
                var messageObject = JObject.Parse(message);

                JToken id = messageObject["id"];

                object methodResult = null;
                string errorMessage = "not implemented";
                if (messageObject.TryGetValue("method", out JToken method)
                && messageObject.TryGetValue("params", out JToken methodParams))
                {
                    if(this.m_commandHandlers.TryGetValue(method.ToString(), out var methodImplementation)) {
                        methodResult = methodImplementation(methodParams);
                        errorMessage = null;
                    }
                }

                // we got an execution, let's send the answer
                var requestResponse = JsonConvert.SerializeObject(new JObject(
                    new JProperty("id", id),
                    new JProperty(errorMessage == null ? "error" : "result", errorMessage ?? methodResult)
                ));

                await this.m_sessionSocket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(requestResponse),
                                                                  offset: 0,
                                                                  count: requestResponse.Length),
                                   messageType: WebSocketMessageType.Text,
                                   endOfMessage: true,
                                   cancellationToken: CancellationToken.None);

                // wait for the next request
                request = await this.m_sessionSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            }
            await this.m_sessionSocket.CloseAsync(request.CloseStatus.Value, request.CloseStatusDescription, cancellationToken);
        }
    }
}