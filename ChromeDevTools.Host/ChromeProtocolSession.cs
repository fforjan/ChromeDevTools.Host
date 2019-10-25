

using System.Collections.Generic;

namespace ChromeDevTools.Host
{
    using ChromeDevTools.Host.Runtime;

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
    public class ChromeProtocolSession : IDisposable, IServiceProvider
    {
        private readonly ConcurrentDictionary<string, Func<JToken, Task<ICommandResponse>>> m_commandHandlers = new ConcurrentDictionary<string, Func<JToken, Task<ICommandResponse>>>();
        private readonly ConcurrentDictionary<Type, string> m_eventTypeMap = new ConcurrentDictionary<Type, string>();

        public IReadOnlyDictionary<Type, IRuntimeHandle> RuntimeHandlers;
        
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
        public ChromeProtocolSession(WebSocket webSocket, params IRuntimeHandle[] handlers)
        {
            CommandTimeout = 5000;
            m_sessionSocket = webSocket;

            this.RuntimeHandlers = handlers.ToDictionary(_ => _.GetType());
        }

        /// <summary>
        /// Sends the specified command and returns the associated command response.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <param name="millisecondsTimeout"></param>
        /// <param name="throwExceptionIfResponseNotReceived"></param>
        /// <returns></returns>
        public async Task SendEvent<TEvent>(TEvent @event, CancellationToken cancellationToken = default, int? millisecondsTimeout = null, bool throwExceptionIfResponseNotReceived = true)
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
        public Task SendEvent(string eventName, JToken @params, CancellationToken cancellationToken = default, int? millisecondsTimeout = null, bool throwExceptionIfResponseNotReceived = true)
        {
            var message = new
            {
                method = eventName,
                @params
            };

            if (millisecondsTimeout.HasValue == false)
                millisecondsTimeout = CommandTimeout;

            LogTrace($"Sending {message.method}: {message.@params.ToString()}");

            var contents = JsonConvert.SerializeObject(message);

            return m_sessionSocket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(contents),
                    offset: 0,
                    count: contents.Length),
                messageType: WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: CancellationToken.None);

        }

        private void LogTrace(string message, params object[] args)
        {
       
        }

        private void LogError(string message, params object[] args)
        {
       
        }


        #region IDisposable Support
        private bool m_isDisposed = false;
        private WebSocket m_sessionSocket;

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

        public async Task Process(CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult request = await m_sessionSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            while (!request.CloseStatus.HasValue)
            {
                var message = Encoding.ASCII.GetString(buffer.Take(request.Count).ToArray());
                var messageObject = JObject.Parse(message);

                JToken id = messageObject["id"];

                object methodResult = null;
                string errorMessage = "not implemented";
                if (messageObject.TryGetValue("method", out JToken method))
                {
                    messageObject.TryGetValue("params", out JToken methodParams);
                    if (m_commandHandlers.TryGetValue(method.ToString(), out var methodImplementation))
                    {
                        methodResult = await methodImplementation(methodParams);
                        errorMessage = null;
                    }
                }

                object result;
                if (errorMessage == null)
                {
                    result = new
                    {
                        id,
                        result = methodResult
                    };
                }
                else
                {
                    result = new
                    {
                        id,
                        error = errorMessage
                    };
                }

                // we got an execution, let's send the answer
                var requestResponse = JsonConvert.SerializeObject(result);

                await m_sessionSocket.SendAsync(buffer: new ArraySegment<byte>(array: Encoding.ASCII.GetBytes(requestResponse),
                        offset: 0,
                        count: requestResponse.Length),
                    messageType: WebSocketMessageType.Text,
                    endOfMessage: true,
                    cancellationToken: CancellationToken.None);

                // wait for the next request
                request = await m_sessionSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
            }
            await m_sessionSocket.CloseAsync(request.CloseStatus.Value, request.CloseStatusDescription, cancellationToken);
        }

        public void RegisterCommandHandler<T>(Func<T, Task<ICommandResponse<T>>> handler)
        where T : class, ICommand
        {
            m_commandHandlers[((T)Activator.CreateInstance(typeof(T))).CommandName] = async _ => await handler(_?.ToObject<T>());
        }

        public object GetService(Type serviceType)
        {
            if (this.RuntimeHandlers.TryGetValue(serviceType, out var runtimeHandle))
            {
                return runtimeHandle;
            }

            return null;
        }
    }
}
