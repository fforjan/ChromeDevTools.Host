namespace ChromeDevTools.Host.Handlers
{
	using ChromeDevTools.Host;

	using ChromeDevTools.Host.Runtime;
    using ChromeDevTools.Host.Runtime.Debugger;
    using System.Threading.Tasks;

    public class DebuggerHandler : IRuntimeHandler
    {
        public ChromeProtocolSession Session { get; private set; }

        public virtual bool IsEnable { get; protected set; }

        public Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {
            this.IsEnable = true;
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
            this.Session = session;


            session.RegisterCommandHandler<DisableCommand>(this.DisableCommand);
            session.RegisterCommandHandler<EnableCommand>(this.EnableCommand);
        }
    }
}