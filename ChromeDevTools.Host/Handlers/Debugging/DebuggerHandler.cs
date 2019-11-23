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

        private Dictionary<string, ScriptInfo> scriptsById;
        private Dictionary<string, ScriptInfo> scriptsByUrl;

        public ChromeProtocolSession Session { get; private set; }

        public virtual bool IsEnable { get; protected set; }

        public DebuggerHandler()
        {
            scriptsById = new Dictionary<string, ScriptInfo>();
            scriptsByUrl = new Dictionary<string, ScriptInfo>();
        }

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
            session.RegisterCommandHandler<ResumeCommand>(this.ResumeCommand);
            session.RegisterCommandHandler<GetPossibleBreakpointsCommand>(this.GetPossibleBreakpointsCommand);

            session.RegisterCommandHandler<SetBreakpointsActiveCommand>(this.SetBreakpointsActiveCommand);

            session.RegisterCommandHandler<SetBreakpointByUrlCommand>(this.SetBreakpointByUrlCommand);

            session.RegisterCommandHandler<RemoveBreakpointCommand>(this.RemoveBreakpointCommand);
        }

        private Task<ICommandResponse<RemoveBreakpointCommand>> RemoveBreakpointCommand(RemoveBreakpointCommand arg)
        {
            var info = arg.BreakpointId.Split('/');

            ScriptsByUrl[info[0]].BreakPoints[info[1]].IsEnabled = false;

            return Task.FromResult<ICommandResponse<RemoveBreakpointCommand>>(new RemoveBreakpointCommandResponse());
        }

        private Task<ICommandResponse<SetBreakpointByUrlCommand>> SetBreakpointByUrlCommand(SetBreakpointByUrlCommand arg)
        {
            var result = new SetBreakpointByUrlCommandResponse();

            var url = arg.Url ?? arg.UrlRegex.Trim('|');

            var script = this.ScriptsByUrl[url];

            BreakPoint closestBreakpoint = null;
            var closestLineDistance = long.MaxValue;
            var closestColumnDistance = long.MaxValue;

            foreach (var breakPoint in script.BreakPoints.Values)
            {
                var currentLineDistance = Math.Abs(breakPoint.Info.lineNumber - arg.LineNumber);
                var currentColumnDistance = arg.ColumnNumber.HasValue ? Math.Abs(breakPoint.Info.columnNumber - arg.ColumnNumber.Value) : breakPoint.Info.columnNumber;
                if (currentLineDistance < closestLineDistance
                    || (currentLineDistance == closestLineDistance && currentColumnDistance < closestColumnDistance))
                {
                    closestBreakpoint = breakPoint;
                    closestLineDistance = currentLineDistance;
                    closestColumnDistance = currentColumnDistance;
                }
            }

            closestBreakpoint.IsEnabled = true;

            result.BreakpointId = script.Url + "/" + closestBreakpoint.Name;
            result.Locations = new[] { closestBreakpoint.AsLocation(script) };

            return Task.FromResult<ICommandResponse<SetBreakpointByUrlCommand>>(result);
        }

        private async Task<ICommandResponse<ResumeCommand>> ResumeCommand(ResumeCommand arg)
        {
            await this.ScriptsById.Values.SelectMany(_ => _.BreakPoints.Values).First(_ => _.IsBreaked).Continue();

            return new ResumeCommandResponse();
        }

        private Task<ICommandResponse<GetPossibleBreakpointsCommand>> GetPossibleBreakpointsCommand(GetPossibleBreakpointsCommand arg)
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

            return Task.FromResult<ICommandResponse<GetPossibleBreakpointsCommand >>(result);
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