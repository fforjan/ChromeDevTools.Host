namespace ChromeDevTools.Host
{
    using ChromeDevTools.Host.Runtime;
    using ChromeDevTools.Host.Runtime.Debugger;
    using System.Threading.Tasks;

    public class DebuggerHandler
    {

        private readonly ChromeProtocolSession session;

        public DebuggerHandler(ChromeProtocolSession session)
        {
            this.session = session;

            session.RegisterCommandHandler<EnableCommand>(this.EnableCommand);
        }

        public Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {

            return Task.FromResult<ICommandResponse<EnableCommand>>(new EnableCommandResponse
            {
                DebuggerId = "virtual debugger"
            });
        }
    }
}