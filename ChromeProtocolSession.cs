namespace EchoApp
{
    using BaristaLabs.ChromeDevTools.Runtime;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Net.WebSockets;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;

    /// <summary>
    /// Represents a websocket connection to a running chrome instance that can be used to send commands and recieve events.
    ///</summary>
    public class ChromeSession : IDisposable
    {
        private readonly string m_endpointAddress;
        private readonly ILogger<ChromeSession> m_logger;

        private readonly ConcurrentDictionary<string, ConcurrentBag<Action<object>>> m_commandHandlers = new ConcurrentDictionary<string, ConcurrentBag<Action<object>>>();
        private readonly ConcurrentDictionary<Type, string> m_eventTypeMap = new ConcurrentDictionary<Type, string>();

        private ActionBlock<string> m_messageQueue;
        private WebSocket m_sessionSocket;
        private ManualResetEventSlim m_openEvent = new ManualResetEventSlim(false);
        private ManualResetEventSlim m_responseReceived = new ManualResetEventSlim(false);
        private LastResponseInfo m_lastResponse;
        private long m_currentCommandId = 0;

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
         
            // m_messageQueue = new ActionBlock<string>((Action<string>)ProcessIncomingMessage,
            //     new ExecutionDataflowBlockOptions {
            //         EnsureOrdered = true,
            //         MaxDegreeOfParallelism = 1,
            //     });

            // m_sessionSocket = webSocket;
            // m_sessionSocket.MessageReceived += Ws_MessageReceived;
            // m_sessionSocket.Error += Ws_Error;
            // m_sessionSocket.Opened += Ws_Opened;
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

              
            if(EventTypeMap.TryGetMethodNameForType<TEvent>(out var eventName)) {

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
        [DebuggerStepThrough]
        public Task SendEvent(string eventName, JToken @params, CancellationToken cancellationToken = default(CancellationToken), int? millisecondsTimeout = null, bool throwExceptionIfResponseNotReceived = true)
        {
            var message = new
            {
                id = Interlocked.Increment(ref m_currentCommandId),
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
                        // m_sessionSocket.Opened -= Ws_Opened;
                        // m_sessionSocket.Error -= Ws_Error;
                        // m_sessionSocket.MessageReceived -= Ws_MessageReceived;
                        // m_sessionSocket.Dispose();
                        m_sessionSocket = null;
                    }

                    if (m_messageQueue != null)
                    {
                        m_messageQueue.Complete();
                        m_messageQueue = null;
                    }

                    if (m_openEvent != null)
                    {
                        m_openEvent.Dispose();
                        m_openEvent = null;
                    }

                    if (m_responseReceived != null)
                    {
                        m_responseReceived.Dispose();
                        m_responseReceived = null;
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

        #region Nested Classes
        private class LastResponseInfo
        {
            public bool IsError = false;
            public JToken Result;
        }
        #endregion
    }
}