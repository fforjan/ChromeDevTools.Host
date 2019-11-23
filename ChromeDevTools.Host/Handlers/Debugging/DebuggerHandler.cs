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

        private Dictionary<string, ScriptInfo> scripts;
        public ChromeProtocolSession Session { get; private set; }

        public virtual bool IsEnable { get; protected set; }

        public DebuggerHandler()
        {
            scripts = new Dictionary<string, ScriptInfo>();
        }

        public void RegisterScripts(ScriptInfo scriptInfo)
        {
            this.scripts.Add(scriptInfo.Id.ToString(), scriptInfo);

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
        }

        private async Task<ICommandResponse<ResumeCommand>> ResumeCommand(ResumeCommand arg)
        {
            await this.ScriptsFromId.Values.SelectMany(_ => _.BreakPoints.Values).First(_ => _.IsBreaked).Continue();

            return new ResumeCommandResponse();
        }

        protected virtual void ParseScripts()
        {
            foreach (var script in ScriptsFromId.Values)
            {
                Session.SendEvent(script.Parse()).Wait();
            }
        }

        public IReadOnlyDictionary<string, ScriptInfo> ScriptsFromId { get { return scripts; } }
    }
}