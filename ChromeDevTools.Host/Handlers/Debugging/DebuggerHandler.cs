namespace ChromeDevTools.Host.Handlers.Debugging
{
    using ChromeDevTools.Host;

    using ChromeDevTools.Host.Runtime;
    using ChromeDevTools.Host.Runtime.Debugger;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class DebuggerHandler : IRuntimeHandler
    {
        public static readonly BreakPoint AnyBreakpoint = new BreakPoint("Any", (-1,-1,"Any"));

        private Dictionary<string, ScriptInfo> scriptsById;
        private Dictionary<string, ScriptInfo> scriptsByUrl;

        public ChromeProtocolSession Session { get; private set; }

        public virtual bool IsEnable { get; protected set; }

        public DebuggerHandler()
        {
            scriptsById = new Dictionary<string, ScriptInfo>();
            scriptsByUrl = new Dictionary<string, ScriptInfo>();
        }

        public BreakPoint BreakOn { get; set; }

        public void RegisterScripts(ScriptInfo scriptInfo)
        {
            this.scriptsById.Add(scriptInfo.Id.ToString(), scriptInfo);
            this.scriptsByUrl.Add(scriptInfo.Url, scriptInfo);

            scriptInfo.BreakPointHit += BreakPointWasHit;
        }

        protected void BreakPointWasHit(object sender, BreakPointHitEventArgs e)
        {
            Session.SendEvent(e.AsEvent());
        }

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

        public virtual void Register(ChromeProtocolSession session)
        {
            this.Session = session;

            session.RegisterCommandHandler<DisableCommand>(this.DisableCommand);
            session.RegisterCommandHandler<EnableCommand>(this.EnableCommand);
            

            session.RegisterCommandHandler<GetPossibleBreakpointsCommand>(this.GetPossibleBreakpointsCommand);

            session.RegisterCommandHandler<SetBreakpointsActiveCommand>(this.SetBreakpointsActiveCommand);
            session.RegisterCommandHandler<SetBreakpointByUrlCommand>(this.SetBreakpointByUrlCommand);
            session.RegisterCommandHandler<RemoveBreakpointCommand>(this.RemoveBreakpointCommand);

            session.RegisterCommandHandler<ResumeCommand>(this.ResumeCommand);
            session.RegisterCommandHandler<PauseCommand>(this.PauseCommand);
            session.RegisterCommandHandler<ContinueToLocationCommand>(this.ContinueToLocationCommand);
            session.RegisterCommandHandler<StepOverCommand>(this.StepOverCommand);

        }

        private Task<ICommandResponse<RemoveBreakpointCommand>> RemoveBreakpointCommand(RemoveBreakpointCommand arg)
        {
            var info = arg.BreakpointId.Split('/');

            ScriptsByUrl[info[0]].BreakPoints[info[1]].IsEnabled = false;

            return Task.FromResult<ICommandResponse<RemoveBreakpointCommand>>(new RemoveBreakpointCommandResponse());
        }

        private Task<ICommandResponse<SetBreakpointByUrlCommand>> SetBreakpointByUrlCommand(SetBreakpointByUrlCommand arg)
        {
            return Task.Run<ICommandResponse<SetBreakpointByUrlCommand>>(() =>
            {
                var result = new SetBreakpointByUrlCommandResponse();

                var url = arg.Url ?? arg.UrlRegex.Trim('|');

                var script = this.ScriptsByUrl[url];

                var closestBreakpoint = GetClosetBreakPoint(script, arg.LineNumber, arg.ColumnNumber);

                closestBreakpoint.IsEnabled = true;

                result.BreakpointId = script.Url + "/" + closestBreakpoint.Name;
                result.Locations = new[] { closestBreakpoint.AsLocation(script) };

                return result;
            });
        }

        private BreakPoint GetClosetBreakPoint(ScriptInfo script, long lineNumber, long? columnNumber)
        {

            BreakPoint closestBreakpoint = null;
            var closestLineDistance = long.MaxValue;
            var closestColumnDistance = long.MaxValue;

            foreach (var breakPoint in script.BreakPoints.Values)
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

        private Task<ICommandResponse<ResumeCommand>> ResumeCommand(ResumeCommand arg)
        {
            Continue();

            return Task.FromResult < ICommandResponse < ResumeCommand >>( new ResumeCommandResponse());
        }

        private void Continue()
        {
            this.ScriptsById.Values.SelectMany(_ => _.BreakPoints.Values).First(_ => _.IsBreaked).Continue();
        }

        private Task<ICommandResponse<PauseCommand>> PauseCommand(PauseCommand arg)
        {
            this.BreakOn = AnyBreakpoint;

            return Task.FromResult<ICommandResponse<PauseCommand>>(new PauseCommandResponse());
        }

        private Task<ICommandResponse<StepOverCommand>> StepOverCommand(StepOverCommand arg)
        {
            this.BreakOn = AnyBreakpoint;
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

                bool isIncluded(BreakPoint breakPoint)
                {
                    var afterStart = breakPoint.Info.lineNumber > arg.Start.LineNumber
                                || (breakPoint.Info.lineNumber == arg.Start.LineNumber
                                    && breakPoint.Info.columnNumber >= arg.Start.ColumnNumber);
                    var beforeEnd = breakPoint.Info.lineNumber < arg.End.LineNumber
                                || (breakPoint.Info.lineNumber == arg.End.LineNumber
                                    && breakPoint.Info.columnNumber <= arg.End.ColumnNumber);

                    return afterStart && beforeEnd;
                }

                result.Locations = script.BreakPoints.Values
                    .Where(isIncluded)
                    .Select(_ => _.AsBreakLocation(script)).ToArray();

                return result;
            });
        }

        protected virtual Task<ICommandResponse<SetBreakpointsActiveCommand>> SetBreakpointsActiveCommand(SetBreakpointsActiveCommand arg) {

            return Task.FromResult<ICommandResponse<SetBreakpointsActiveCommand>>(new SetBreakpointsActiveCommandResponse());
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