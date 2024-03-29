namespace ChromeDevTools.Host.Runtime.Debugger
{
    using Newtonsoft.Json;

    /// <summary>
    /// This method is deprecated - use Debugger.stepInto with breakOnAsyncCall and
    /// Debugger.pauseOnAsyncTask instead. Steps into next scheduled async task if any is scheduled
    /// before next pause. Returns success when async task is actually scheduled, returns error if no
    /// task were scheduled or another scheduleStepIntoAsync was called.
    /// </summary>
    public sealed class ScheduleStepIntoAsyncCommand : ICommand
    {
        private const string ChromeRemoteInterface_CommandName = "Debugger.scheduleStepIntoAsync";
        
        [JsonIgnore]
        public string CommandName
        {
            get { return ChromeRemoteInterface_CommandName; }
        }

    }

    public sealed class ScheduleStepIntoAsyncCommandResponse : ICommandResponse<ScheduleStepIntoAsyncCommand>
    {
    }
}