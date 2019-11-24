namespace ChromeDevTools.Host.Handlers.Debugging
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ChromeDevTools.Host.Runtime.Debugger;

    public class BreakableScriptPoint
    {
        public (int lineNumber, int columnNumber, string functionName) Info { get; }

        private object locker = new object();

        private TaskCompletionSource<bool> breakPointTask;

        public BreakableScriptPoint(string name, (int lineNumber, int columnNumber, string functionName) info)
        {
            Name = name;
            this.Info = info;
        }

        public string Name { get; }

        public bool IsBreakPointSet
        {
            get;
            set;
        }

        public bool IsBreaked { get { return this.breakPointTask != null; } }

        public void Continue()
        {
            lock (locker)
            {
                var currentTask = breakPointTask;
                breakPointTask = null;
                currentTask?.SetResult(true);
            }
        }


        public Task GetBreakPointTask(CancellationToken cancellationToken)
        {
            lock (locker)
            {
                if (breakPointTask == null)
                {
                    this.breakPointTask = new TaskCompletionSource<bool>();
                    cancellationToken.Register(this.Continue);
                }

                this.BreakPointHit?.Invoke(this, new BreakPointHitEventArgs { BreakPoint = this });

                return breakPointTask.Task;
            }
        }

        public CallFrame[] GetCallFrame(ScriptInfo relatedScript)
        {
            return new[] {
                new CallFrame
                {
                    CallFrameId = $"topFrameFor{Name}",
                    Location = AsLocation(relatedScript),
                    FunctionName = Info.functionName,
                    Url = relatedScript.Url,
                    This = new Host.Runtime.Runtime.RemoteObject
                    {
                        Subtype = "null",
                        Type = "object",
                        Value = null
                    },
                    ScopeChain = new [] {
                        new Scope
                        {
                            Type = "local",
                            Object = new Host.Runtime.Runtime.RemoteObject
                            {
                                Type = "object",
                                ClassName = "Object",
                                Description = "Object",
                                ObjectId = GetContextId(relatedScript)
                            }
                        }
                    }
                }
            };
        }

        public string GetContextId(ScriptInfo relatedScript)
        {
            return GetBreakPointName(relatedScript) + ":1";
        }

        public string GetBreakPointName(ScriptInfo relatedScript)
        {
            return relatedScript.Url + "/" + Name ;
        }

        public Location AsLocation(ScriptInfo relatedScript)
        {
            return new Location
            {
                LineNumber = Info.lineNumber,
                ColumnNumber = Info.columnNumber,
                ScriptId = relatedScript.Id.ToString()
            };
        }

        public BreakLocation AsBreakLocation(ScriptInfo relatedScript)
        {
            return new BreakLocation
            {
                ColumnNumber = this.Info.columnNumber,
                LineNumber = this.Info.lineNumber,
                ScriptId = relatedScript.Id.ToString()
            };
        }

        public event EventHandler<BreakPointHitEventArgs> BreakPointHit;
    }
}