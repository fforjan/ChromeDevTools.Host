namespace EchoApp
{
    using BaristaLabs.ChromeDevTools.Runtime;
    using BaristaLabs.ChromeDevTools.Runtime.Runtime;
    using System;
    using System.Threading.Tasks;

    public class  RuntimeHandle  {

        private ChromeSession session;

        public RuntimeHandle(ChromeSession session) {
            this.session = session;

            session.RegisterCommandHandler<EnableCommand>(this.EnableCommand);
        }

        public async Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command) {

            await this.session.SendEvent(new ExecutionContextCreatedEvent {
                Context = new ExecutionContextDescription {
                    Id = this.Context,
                    Origin = "",
                    Name = "virtual context",
                    AuxData = new {
                        isDefault = true
                    }
                }
            });

            return new EnableCommandResponse();
        }

        internal Task Log(string logEntry)
        {
            return this.session.SendEvent(GetLogEvent(logEntry));
        }

         private ConsoleAPICalledEvent GetLogEvent(string logMessage) {
            var logEvent  = new ConsoleAPICalledEvent {
               Type = "log",
               Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
               ExecutionContextId = this.Context,
               Args= new RemoteObject[] {
                   new RemoteObject{
                       Type = "string",
                       Value = logMessage
                   }
               }
            };

            return logEvent;
        }

        public int Context {get { return 1; } }

    }
}