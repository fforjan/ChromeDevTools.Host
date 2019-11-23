namespace FwkConsoleApp
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ChromeDevTools.Host;
    using ChromeDevTools.Host.Handlers.Debugging;
    using ChromeDevTools.Host.Runtime;
    using ChromeDevTools.Host.Runtime.Debugger;

    public class MyDebuggerHandler : DebuggerHandler
    {
        private readonly IReadOnlyDictionary<string, ScriptInfo> scripts;

        public override bool IsEnable {
            get => base.IsEnable;
            protected set {
                base.IsEnable = value;
                if(value)
                {
                    ParseScripts();
                }
            }
        }

        public MyDebuggerHandler(params ScriptInfo[] scripts)
        {
            this.scripts = scripts.ToDictionary(_ => _.Id.ToString());
        }

        public override void Register(ChromeProtocolSession session)
        {
            base.Register(session);
            session.RegisterCommandHandler<GetScriptSourceCommand>(GetScriptSource);
        }

        private Task<ICommandResponse<GetScriptSourceCommand>> GetScriptSource(GetScriptSourceCommand arg)
        {
            
            return Task.FromResult<ICommandResponse<GetScriptSourceCommand>>(new GetScriptSourceCommandResponse { ScriptSource = scripts[arg.ScriptId].Content });
        }

        public void ParseScripts()
        {

            foreach (var script in scripts.Values)
            {
                Session.SendEvent(script.Parse()).Wait();
            }

            var myScript = scripts.Values.First();


            var pausedEvent = new PausedEvent
            {
                HitBreakpoints = new string[0],
                Reason = "Break on start",
                CallFrames = myScript.BreakPoints.Values.First().GetCallFrame(myScript)
            };

            Session.SendEvent(pausedEvent).Wait();
        }
    }
}