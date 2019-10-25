namespace ChromeDevTools.Host
{
    using System.Threading.Tasks;

    using BaristaLabs.ChromeDevTools.Runtime;
    using BaristaLabs.ChromeDevTools.Runtime.Profiler;

    public class ProfilerHandler
    {

        private readonly ChromeProtocolSession session;

        public ProfilerHandler(ChromeProtocolSession session)
        {
            this.session = session;

            session.RegisterCommandHandler<EnableCommand>(EnableCommand);
        }

        public Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {

            return Task.FromResult<ICommandResponse<EnableCommand>>(new EnableCommandResponse());
        }
    }
}
