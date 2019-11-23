﻿namespace ChromeDevTools.Host.Handlers.Debugging
{
    using System;
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


        public Task BreakPointTask {
            get {

                // if enabled, wait for it
                if (IsEnabled)
                {
                    if (breakPointTask == null)
                    {
                        breakPointTask = new TaskCompletionSource<bool>();
                    }

                    this.BreakPointHit?.Invoke(this, new BreakPointHitEventArgs { BreakPoint = this });

                    return breakPointTask.Task;
                }

                return Task.CompletedTask;
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
                    This = new Runtime.Runtime.RemoteObject
                    {
                        Subtype = "null",
                        Type = "object",
                        Value = null
                    },
                    ScopeChain = new Scope[] { }

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