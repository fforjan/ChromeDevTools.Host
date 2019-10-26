namespace ChromeDevTools.Host.Handlers
{
    /// <summary>
    /// Handler interface
    /// </summary>
    public interface IRuntimeHandler
    {
        /// <summary>
        /// When this method is invoke, the <see cref="ChromeProtocolSession.RegisterCommandHandler{T}(System.Func{T, System.Threading.Tasks.Task{Runtime.ICommandResponse{T}}})"/>
        /// should be invoke for the different handlers of this class
        /// </summary>
        /// <param name="session"></param>
        void Register(ChromeProtocolSession session);
    }
}