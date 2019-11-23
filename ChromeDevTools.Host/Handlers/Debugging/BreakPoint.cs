namespace ChromeDevTools.Host.Handlers.Debugging
{

    using ChromeDevTools.Host.Runtime.Debugger;

    public class BreakPoint
    {
        private readonly (int lineNumber, int columnNumber, string functionName) info;

        public BreakPoint(string name, (int lineNumber, int columnNumber, string functionName) info)
        {
            Name = name;
            this.info = info;
        }

        public string Name { get; }

        public CallFrame[] GetCallFrame(ScriptInfo relatedScript)
        {
            return new[] {
                new CallFrame
                {
                    CallFrameId = $"topFrameFor{Name}",
                    Location = new Location
                    {
                        LineNumber = info.lineNumber,
                        ColumnNumber = info.columnNumber,
                        ScriptId = relatedScript.Id.ToString()
                    },
                    FunctionName = info.functionName,
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

    }
}