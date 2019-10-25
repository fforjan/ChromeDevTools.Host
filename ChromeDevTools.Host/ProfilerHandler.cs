namespace ChromeDevTools.Host
{
    using System.Threading.Tasks;

    using ChromeDevTools.Host.Runtime;
    using ChromeDevTools.Host.Runtime.Profiler;

    public class ProfilerHandler : IRuntimeHandle
    {
        private ChromeProtocolSession session;

        public Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {
            return Task.FromResult<ICommandResponse<EnableCommand>>(new EnableCommandResponse());
        }

        public void Register(ChromeProtocolSession session)
        {
            this.session = session;

            session.RegisterCommandHandler<EnableCommand>(EnableCommand);
        }
    }
}
