namespace ChromeDevTools.Host.Handlers.Debugging
{
    using ChromeDevTools.Host;

    using ChromeDevTools.Host.Runtime;
    using ChromeDevTools.Host.Runtime.Debugger;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;


    /// <summary>
    /// Debugger implementation
    /// </summary>
    public class DebuggerHandler : IRuntimeHandler
    {       
        private Dictionary<string, ScriptInfo> scriptsById;
        private Dictionary<string, ScriptInfo> scriptsByUrl;

        public ChromeProtocolSession Session { get; private set; }

        public virtual bool IsEnable { get; protected set; }

        public DebuggerHandler()
        {
            scriptsById = new Dictionary<string, ScriptInfo>();
            scriptsByUrl = new Dictionary<string, ScriptInfo>();
        }

        public BreakableScriptPoint BreakOn { get; set; }

        public void RegisterScripts(ScriptInfo scriptInfo)
        {
            this.scriptsById.Add(scriptInfo.Id.ToString(), scriptInfo);
            this.scriptsByUrl.Add(scriptInfo.Url, scriptInfo);

            scriptInfo.BreakPointHit += BreakPointWasHit;
        }

        protected void BreakPointWasHit(object sender, BreakPointHitEventArgs e)
        {
            Session.SendEvent(e.AsEvent()).Wait();
        }
   
        public virtual void Register(ChromeProtocolSession session)
        {
            this.Session = session;

            session.RegisterCommandHandler<DisableCommand>(this.DisableCommand);
            session.RegisterCommandHandler<EnableCommand>(this.EnableCommand);

            session.RegisterCommandHandler<GetScriptSourceCommand>(GetScriptSource);
            session.RegisterCommandHandler<GetPossibleBreakpointsCommand>(this.GetPossibleBreakpointsCommand);

            session.RegisterCommandHandler<SetBreakpointsActiveCommand>(this.SetBreakpointsActiveCommand);
            session.RegisterCommandHandler<SetBreakpointByUrlCommand>(this.SetBreakpointByUrlCommand);
            session.RegisterCommandHandler<RemoveBreakpointCommand>(this.RemoveBreakpointCommand);

            session.RegisterCommandHandler<ResumeCommand>(this.ResumeCommand);
            session.RegisterCommandHandler<PauseCommand>(this.PauseCommand);
            session.RegisterCommandHandler<ContinueToLocationCommand>(this.ContinueToLocationCommand);
            session.RegisterCommandHandler<StepOverCommand>(this.StepOverCommand);

        }

        #region Commands implementation

        public Task<ICommandResponse<EnableCommand>> EnableCommand(EnableCommand command)
        {
            if (!this.IsEnable)
            {
                this.IsEnable = true;
                ParseScripts();
            }


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

        private Task<ICommandResponse<RemoveBreakpointCommand>> RemoveBreakpointCommand(RemoveBreakpointCommand arg)
        {
            var info = arg.BreakpointId.Split('/');

            ScriptsByUrl[info[0]].BreakableScriptPoint[info[1]].IsBreakPointSet = false;

            return Task.FromResult<ICommandResponse<RemoveBreakpointCommand>>(new RemoveBreakpointCommandResponse());
        }

        private Task<ICommandResponse<SetBreakpointByUrlCommand>> SetBreakpointByUrlCommand(SetBreakpointByUrlCommand arg)
        {
            return Task.Run<ICommandResponse<SetBreakpointByUrlCommand>>(() =>
            {
                var result = new SetBreakpointByUrlCommandResponse();

                var url = arg.Url ?? arg.UrlRegex.Split('|')[1]; // FIXME - implement javascript regexp

                var script = this.ScriptsByUrl[url];

                var closestBreakpoint = GetClosetBreakPoint(script, arg.LineNumber, arg.ColumnNumber);

                closestBreakpoint.IsBreakPointSet = true;

                result.BreakpointId = script.Url + "/" + closestBreakpoint.Name;
                result.Locations = new[] { closestBreakpoint.AsLocation(script) };

                return result;
            });
        }

        private Task<ICommandResponse<ResumeCommand>> ResumeCommand(ResumeCommand arg)
        {
            Continue();

            return Task.FromResult<ICommandResponse<ResumeCommand>>(new ResumeCommandResponse());
        }

        private Task<ICommandResponse<PauseCommand>> PauseCommand(PauseCommand arg)
        {
            this.BreakOn = BreakableScriptPoint.Any;

            return Task.FromResult<ICommandResponse<PauseCommand>>(new PauseCommandResponse());
        }

        private Task<ICommandResponse<StepOverCommand>> StepOverCommand(StepOverCommand arg)
        {
            this.BreakOn = BreakableScriptPoint.Any;
            Continue();
            return Task.FromResult<ICommandResponse<StepOverCommand>>(new StepOverCommandResponse());
        }

        private Task<ICommandResponse<ContinueToLocationCommand>> ContinueToLocationCommand(ContinueToLocationCommand arg)
        {
            this.BreakOn = GetClosetBreakPoint(
                this.ScriptsById[arg.Location.ScriptId],
                arg.Location.LineNumber,
                arg.Location.ColumnNumber);

            Continue();

            return Task.FromResult<ICommandResponse<ContinueToLocationCommand>>(new ContinueToLocationCommandResponse());
        }

        private Task<ICommandResponse<GetPossibleBreakpointsCommand>> GetPossibleBreakpointsCommand(GetPossibleBreakpointsCommand arg)
        {
            return Task.Run<ICommandResponse<GetPossibleBreakpointsCommand>>(() =>
            {
                var result = new GetPossibleBreakpointsCommandResponse();

                var script = ScriptsById[arg.Start.ScriptId];

                bool isIncluded(BreakableScriptPoint breakPoint)
                {
                    var afterStart = breakPoint.Info.lineNumber > arg.Start.LineNumber
                                || (breakPoint.Info.lineNumber == arg.Start.LineNumber
                                    && breakPoint.Info.columnNumber >= arg.Start.ColumnNumber);
                    var beforeEnd = breakPoint.Info.lineNumber < arg.End.LineNumber
                                || (breakPoint.Info.lineNumber == arg.End.LineNumber
                                    && breakPoint.Info.columnNumber <= arg.End.ColumnNumber);

                    return afterStart && beforeEnd;
                }

                result.Locations = script.BreakableScriptPoint.Values
                    .Where(isIncluded)
                    .Select(_ => _.AsBreakLocation(script)).ToArray();

                return result;
            });
        }

        private Task<ICommandResponse<SetBreakpointsActiveCommand>> SetBreakpointsActiveCommand(SetBreakpointsActiveCommand arg)
        {

            return Task.FromResult<ICommandResponse<SetBreakpointsActiveCommand>>(new SetBreakpointsActiveCommandResponse());
        }

        private Task<ICommandResponse<GetScriptSourceCommand>> GetScriptSource(GetScriptSourceCommand arg)
        {

            return Task.FromResult<ICommandResponse<GetScriptSourceCommand>>(new GetScriptSourceCommandResponse { ScriptSource = ScriptsById[arg.ScriptId].Content });
        }

        #endregion


        private void Continue()
        {
            this.ScriptsById.Values.SelectMany(_ => _.BreakableScriptPoint.Values).First(_ => _.IsBreaked).Continue();
        }

        private BreakableScriptPoint GetClosetBreakPoint(ScriptInfo script, long lineNumber, long? columnNumber)
        {

            BreakableScriptPoint closestBreakpoint = null;
            var closestLineDistance = long.MaxValue;
            var closestColumnDistance = long.MaxValue;

            foreach (var breakPoint in script.BreakableScriptPoint.Values)
            {
                var currentLineDistance = Math.Abs(breakPoint.Info.lineNumber - lineNumber);
                var currentColumnDistance = columnNumber.HasValue ? Math.Abs(breakPoint.Info.columnNumber - columnNumber.Value) : breakPoint.Info.columnNumber;
                if (currentLineDistance < closestLineDistance
                    || (currentLineDistance == closestLineDistance && currentColumnDistance < closestColumnDistance))
                {
                    closestBreakpoint = breakPoint;
                    closestLineDistance = currentLineDistance;
                    closestColumnDistance = currentColumnDistance;
                }
            }

            return closestBreakpoint;
        }

        protected virtual void ParseScripts()
        {
            foreach (var script in scriptsById.Values)
            {
                Session.SendEvent(script.Parse()).Wait();
            }
        }

        public IReadOnlyDictionary<string, ScriptInfo> ScriptsById { get { return scriptsById; } }
        public IReadOnlyDictionary<string, ScriptInfo> ScriptsByUrl { get { return scriptsByUrl; } }
    }
}