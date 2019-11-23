namespace FwkConsoleApp
{
    using System;
    using System.Threading.Tasks;
    using ChromeDevTools.Host;
    using ChromeDevTools.Host.Handlers;
    using ChromeDevTools.Host.Runtime;
    using ChromeDevTools.Host.Runtime.Debugger;

    public class MyDebuggerHandler : DebuggerHandler
    {
        public string script = "console.log('Hello world')";

        public override bool IsEnable {
            get => base.IsEnable;
            protected set {
                base.IsEnable = value;
                if(value)
                {
                    OnRegister();
                }
            }
        }

        public override void Register(ChromeProtocolSession session)
        {
            base.Register(session);
            session.RegisterCommandHandler<GetScriptSourceCommand>(GetScriptSource);
        }

        private Task<ICommandResponse<GetScriptSourceCommand>> GetScriptSource(GetScriptSourceCommand arg)
        {
            return Task.FromResult<ICommandResponse<GetScriptSourceCommand>>(new GetScriptSourceCommandResponse { ScriptSource = script });
        }

        public void OnRegister()
        {
            var scriptID = "42";
            var parsedEvent = new ScriptParsedEvent
            {
                Url ="Hello world",
                StartLine = 0,
                EndLine = 0,
                StartColumn = 8, // start after .
                EndColumn = 12 , // end after (
                Hash = script.GetHashCode().ToString(),
                ScriptId = scriptID,
                StackTrace = new ChromeDevTools.Host.Runtime.Runtime.StackTrace
                {
                    CallFrames = new []
                    {
                        new ChromeDevTools.Host.Runtime.Runtime.CallFrame
                        {
                            ColumnNumber= 8,
                            ScriptId = "42",
                            FunctionName = "runInThisContext",
                            LineNumber = 0,
                            Url = "Hello world",
                        }
                    }
                }
            };


            Session.SendEvent(parsedEvent).Wait();


            var pausedEvent = new PausedEvent
            {
                HitBreakpoints = new string[0],
                Reason = "Break on start",
                CallFrames = new CallFrame[]
                {
                    new CallFrame
                        {
                            CallFrameId = "topFrame",
                            Location = new Location
                            {
                                LineNumber = 8, ColumnNumber = 12, ScriptId = scriptID
                            },
                            FunctionName = "log",
                            Url = "Hello world",
                            This = new ChromeDevTools.Host.Runtime.Runtime.RemoteObject
                            {
                                Subtype = "null",
                                Type = "object",
                                Value = null
                            },
                            ScopeChain = new Scope[] { }
                            
                        }
                }
            };

            Session.SendEvent(pausedEvent).Wait();
        }
    }
}