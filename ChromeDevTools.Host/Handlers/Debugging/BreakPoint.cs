namespace ChromeDevTools.Host.Handlers.Debugging
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using ChromeDevTools.Host.Runtime.Debugger;

    public class BreakPoint
    {
        public (int lineNumber, int columnNumber, string functionName) Info { get; }

        private TaskCompletionSource<bool> breakPointTask;

        public BreakPoint(string name, (int lineNumber, int columnNumber, string functionName) info)
        {
            Name = name;
            this.Info = info;
        }

        public string Name { get; }

        public bool IsEnabled
        {
            get;
            set;
        }

        public bool IsBreaked { get { return this.breakPointTask != null; } }

        public Task Continue()
        {
            var currentTask = breakPointTask;
            breakPointTask = null;
            currentTask.SetResult(true);

            return currentTask.Task;
        }


        public Task GetBreakPointTask(CancellationToken cancellationToken, Action beforeBreak, Action afterBreak)
        {
            // if enabled, wait for it
            if (IsEnabled)
            {
                if (breakPointTask == null)
                {
                    this.breakPointTask = new TaskCompletionSource<bool>();
                    cancellationToken.Register(() => this.Continue());
                }

                beforeBreak();
                this.BreakPointHit?.Invoke(this, new BreakPointHitEventArgs { BreakPoint = this });
                breakPointTask.Task.ContinueWith((_) => afterBreak);

                return breakPointTask.Task;
            }

            return Task.CompletedTask; 
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
                                ObjectId = nameof(Runtime.RuntimeHandler.LocalObject)
                            }
                        }
                    }
                }
            };
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