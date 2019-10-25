namespace ChromeDevTools.Host
{
    using BaristaLabs.ChromeDevTools.Runtime;
    using BaristaLabs.ChromeDevTools.Runtime.Debugger;
    using System.Threading.Tasks;

    public class DebuggerHandler
    {

        private readonly ChromeSession session;

        public DebuggerHandler(ChromeSession session)
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