namespace ChromeDevTools.Host
{
    using ChromeDevTools.Host.Handlers;
    using ChromeDevTools.Host.Runtime;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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
        /// <summary>
        /// Only one send request can be emitted at a time.
        /// </summary>
        static readonly SemaphoreSlim sendLocker = new SemaphoreSlim(1, 1);

        private readonly ConcurrentDictionary<string, Func<JToken, Task<ICommandResponse>>> m_commandHandlers = new ConcurrentDictionary<string, Func<JToken, Task<ICommandResponse>>>();
        private readonly ConcurrentDictionary<Type, string> m_eventTypeMap = new ConcurrentDictionary<Type, string>();

        private readonly IDictionary<Type, object> serviceMapping = new Dictionary<Type, object>();

        public IReadOnlyCollection<IRuntimeHandler> RuntimeHandlers;
        
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
        public ChromeProtocolSession(WebSocket webSocket, params IRuntimeHandler[] handlers)
        {
            CommandTimeout = 5000;
            m_sessionSocket = webSocket;

            RuntimeHandlers = handlers;

            foreach (var handler in handlers)
            {
                handler.Register(this);
            }
        }

        /// <summary>
        /// Sends the specified command and returns the associated command response.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendEvent<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : class, IEvent
        {
           
                if (@event == null)
                    throw new ArgumentNullException(nameof(@event));


                if (EventTypeMap.TryGetMethodNameForType<TEvent>(out var eventName))
                {
                    await SendEvent(eventName, JToken.FromObject(@event), cancellationToken);
                }
          
        }

        /// <summary>
        /// Returns a JToken based on a command created with the specified command name and params.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="params"></param>
        /// <param name="cancellationToken"></param>
        /// 
        /// 
        /// <returns></returns>
        public async Task SendEvent(string eventName, JToken @params, CancellationToken cancellationToken = default)
        {
            var message = new
            {
                method = eventName,
                @params
            };


            LogTrace($"Sending {message.method}: {message.@params.ToString()}");

            var contents = JsonConvert.SerializeObject(message);

            await sendLocker.WaitAsync();
            try
            {

                 await m_sessionSocket.SendAsync(
                    new ArraySegment<byte>(
                         Encoding.ASCII.GetBytes(contents),
                        0,
                        contents.Length),
                    WebSocketMessageType.Text,
                    true,
                    cancellationToken);
            }
            finally
            {
                sendLocker.Release();
            }

        }

        protected virtual void LogTrace(string message, params object[] args)
        {
       
        }

        protected virtual void LogError(string message, params object[] args)
        {
       
        }


        #region IDisposable Support
        private bool m_isDisposed;
        private WebSocket m_sessionSocket;

        private void Dispose(bool disposing)
        {
            if (!m_isDisposed)
            {
                if (disposing)
                {

                    m_eventTypeMap.Clear();

                    m_sessionSocket = null;
                    
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
                        try
                        {
                            methodResult = await methodImplementation(methodParams);
                            errorMessage = null;
                        }
                        catch (Exception e)
                        {
                            errorMessage = e.ToString();
                        }
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

                await m_sessionSocket.SendAsync(new ArraySegment<byte>(Encoding.ASCII.GetBytes(requestResponse),
                        0,
                        requestResponse.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);

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
            if (!this.serviceMapping.ContainsKey(serviceType))
            {
                this.serviceMapping.Add(serviceType,
                    RuntimeHandlers.FirstOrDefault(_ => serviceType.IsInstanceOfType(_)));
            }

            return this.serviceMapping[serviceType];
        }
    }
}
