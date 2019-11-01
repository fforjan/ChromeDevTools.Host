namespace ChromeDevTools.Host.Handlers
{
	using ChromeDevTools.Host;

	using ChromeDevTools.Host.Runtime;
    using ChromeDevTools.Host.Runtime.Debugger;
    using System.Threading.Tasks;

    public class DebuggerHandler : IRuntimeHandler
    {
        private ChromeProtocolSession session;

        public virtual bool IsEnable { get; protected set; }

        public Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {
            return Task.FromResult<ICommandResponse<EnableCommand>>(new EnableCommandResponse
            {
                DebuggerId = "virtual debugger"
            });
        }

        public Task<ICommandResponse<DisableCommand>> DisableCommand(DisableCommand command)
        {
            this.IsEnable = false;
            return Task.FromResult<ICommandResponse<DisableCommand>>(new DisableCommandResponse());
        }

        public virtual void Register(ChromeProtocolSession session)
        {
            this.session = session;


            session.RegisterCommandHandler<DisableCommand>(this.DisableCommand);
            session.RegisterCommandHandler<EnableCommand>(this.EnableCommand);
        }
    }
}