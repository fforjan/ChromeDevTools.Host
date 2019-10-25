namespace ChromeDevTools.Host
{
    using ChromeDevTools.Host.Runtime;
    using ChromeDevTools.Host.Runtime.Debugger;
    using System.Threading.Tasks;

    public class DebuggerHandler : IRuntimeHandle
    {

        private ChromeProtocolSession session;

        public Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {

            return Task.FromResult<ICommandResponse<EnableCommand>>(new EnableCommandResponse
            {
                DebuggerId = "virtual debugger"
            });
        }

        public void Register(ChromeProtocolSession session)
        {
            this.session = session;

            session.RegisterCommandHandler<EnableCommand>(this.EnableCommand);
        }
    }
}