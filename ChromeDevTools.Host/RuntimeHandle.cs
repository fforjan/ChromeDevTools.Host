namespace ChromeDevTools.Host
{
    using BaristaLabs.ChromeDevTools.Runtime;
    using BaristaLabs.ChromeDevTools.Runtime.Runtime;
    using System;
    using System.Threading.Tasks;

    public class RuntimeHandle
    {
        private readonly ChromeProtocolSession session;

        public RuntimeHandle(ChromeProtocolSession session)
        {
            this.session = session;
            this.IsEnable = false;

            session.RegisterCommandHandler<EnableCommand>(EnableCommand);
        }

        public bool IsEnable { get; private set; }

        public async Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {
            if (!this.IsEnable)
            {
                this.IsEnable = true;
                await session.SendEvent(new ExecutionContextCreatedEvent
                {
                    Context = new ExecutionContextDescription
                    {
                        Id = Context,
                        Origin = "",
                        Name = "virtual context",
                        AuxData = new
                        {
                            isDefault = true
                        }
                    }
                });
            }

            return new EnableCommandResponse();
        }

        public Task Log(string logEntry)
        {
            if (this.IsEnable)
            {
                return session.SendEvent(GetLogEvent(logEntry, "log"));
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public Task Warning(string logEntry)
        {
            if (this.IsEnable)
            {
                return session.SendEvent(GetLogEvent(logEntry, "warning"));
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public Task Error(string logEntry)
        {
            if (this.IsEnable)
            {
                return session.SendEvent(GetLogEvent(logEntry, "error"));
            }
            else
            {
                return Task.CompletedTask;
            }
        } 
        
        public Task Debug(string logEntry)
        {
            if (this.IsEnable)
            {
                return session.SendEvent(GetLogEvent(logEntry, "debug"));
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private ConsoleAPICalledEvent GetLogEvent(string logMessage, string level)
        {
            var logEvent = new ConsoleAPICalledEvent
            {
                Type = level,
                Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                ExecutionContextId = Context,
                Args = new RemoteObject[] {
                   new RemoteObject{
                       Type = "string",
                       Value = logMessage
                   }
               }
            };

            return logEvent;
        }

        public int Context { get { return 1; } }

    }
}