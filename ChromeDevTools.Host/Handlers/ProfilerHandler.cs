namespace ChromeDevTools.Host.Handlers
{
    using System.Threading.Tasks;

    using ChromeDevTools.Host.Runtime;
    using ChromeDevTools.Host.Runtime.Profiler;

    public class ProfilerHandler : IRuntimeHandler
    {
        private ChromeProtocolSession session;

        public virtual bool IsEnable { get; protected set; }

        public Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {
            this.IsEnable = true;
            return Task.FromResult<ICommandResponse<EnableCommand>>(new EnableCommandResponse());
        }

        public Task<ICommandResponse<DisableCommand>> DisableCommand(DisableCommand command)
        {
            this.IsEnable = false;
            return Task.FromResult<ICommandResponse<DisableCommand>>(new DisableCommandResponse());
        }


        public virtual void Register(ChromeProtocolSession session)
        {
            this.IsEnable = false;
            this.session = session;

            session.RegisterCommandHandler<EnableCommand>(EnableCommand);
            session.RegisterCommandHandler<DisableCommand>(DisableCommand);
        }
    }
}
