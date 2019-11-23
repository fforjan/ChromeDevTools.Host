namespace FwkConsoleApp
{
    using System.Linq;
    using System.Threading.Tasks;
    using ChromeDevTools.Host;
    using ChromeDevTools.Host.Handlers.Debugging;
    using ChromeDevTools.Host.Runtime;
    using ChromeDevTools.Host.Runtime.Debugger;

    public class MyDebuggerHandler : DebuggerHandler
    {
        public override bool IsEnable {
            get => base.IsEnable;
            protected set {
                base.IsEnable = value;
            }
        }

        public MyDebuggerHandler(MyScript script)
        {
            this.RegisterScripts(script);            
        }

        public override void Register(ChromeProtocolSession session)
        {
            base.Register(session);
            session.RegisterCommandHandler<GetScriptSourceCommand>(GetScriptSource);
        }

        private Task<ICommandResponse<GetScriptSourceCommand>> GetScriptSource(GetScriptSourceCommand arg)
        {
            
            return Task.FromResult<ICommandResponse<GetScriptSourceCommand>>(new GetScriptSourceCommandResponse { ScriptSource = ScriptsById[arg.ScriptId].Content });
        }
    }

}