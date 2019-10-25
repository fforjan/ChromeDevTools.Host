namespace ChromeDevTools.Host
{
    using BaristaLabs.ChromeDevTools.Runtime;
    using BaristaLabs.ChromeDevTools.Runtime.Runtime;
    using System;
    using System.Threading.Tasks;

    public class RuntimeHandle
    {

        private ChromeSession session;

        public RuntimeHandle(ChromeSession session)
        {
            this.session = session;

            session.RegisterCommandHandler<EnableCommand>(EnableCommand);
        }

        public async Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {

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

            return new EnableCommandResponse();
        }

        public Task Log(string logEntry)
        {
            return session.SendEvent(GetLogEvent(logEntry));
        }

        private ConsoleAPICalledEvent GetLogEvent(string logMessage)
        {
            var logEvent = new ConsoleAPICalledEvent
            {
                Type = "log",
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