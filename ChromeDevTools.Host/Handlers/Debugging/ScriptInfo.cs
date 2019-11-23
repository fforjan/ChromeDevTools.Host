namespace ChromeDevTools.Host.Handlers.Debugging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ChromeDevTools.Host.Runtime.Debugger;

    public class ScriptInfo
    {
        private static int NextID = 1;

        public ScriptInfo(string content, string url, params BreakPoint[] breakPoints)
        {
            this.Content = content;
            this.Url = url;

            var contentLines = this.Content.Split('n');

            this.StartLine = 0;
            this.EndLine = contentLines.Length - 1;

            this.StartColumn = 0;
            this.EndColumn = contentLines[this.EndLine].Length - 1;

            this.HashCode = content.GetHashCode().ToString();

            this.BreakPoints = breakPoints.ToDictionary(_ => _.Name);

            foreach(var breakPoint in breakPoints)
            {
                breakPoint.BreakPointHit += this.BreakPointWasHit;
            }
        }


        protected void BreakPointWasHit(object sender, BreakPointHitEventArgs arg)
        {
            arg.Script = this;
            this.BreakPointHit.Invoke(this, arg);
        }

        public int Id { get; } = ++NextID;


        public string HashCode { get; }

        public IReadOnlyDictionary<string, BreakPoint> BreakPoints { get; }

        public string Content { get; }

        public string Url { get; }

        public int StartLine { get; }

        public int EndLine { get; }

        public int StartColumn { get; }

        public int EndColumn { get; }

        public ScriptParsedEvent Parse()
        {
            return new ScriptParsedEvent
            {
                Url = this.Url,
                StartLine = this.StartLine,
                EndLine = this.EndLine,
                StartColumn = this.StartColumn,
                EndColumn = this.EndColumn,
                Hash = this.HashCode,
                ScriptId = this.Id.ToString()
            };
        }

        public event EventHandler<BreakPointHitEventArgs> BreakPointHit;
    }
}