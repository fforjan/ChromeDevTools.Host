namespace ChromeDevTools.Host.Handlers.Debugging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ChromeDevTools.Host.Runtime.Debugger;


    /// <summary>
    /// Description of a script.
    /// A script as 2 key components:
    /// - its textual content
    /// - a set a breakable point.
    /// A breakable point should be consider like a statement where the debugger can be stop.
    /// It is not requested to have all statements describe into the script.
    /// Action related to location will be executed on the 'closest' one.
    /// </summary>
    public class ScriptInfo
    {
        private static int NextID = 1;
        private readonly ScriptParsedEvent scriptParsedEvent;

        /// <summary>
        /// Initialize a script description
        /// </summary>
        /// <param name="content">script content</param>
        /// <param name="url">script url (name)</param>
        /// <param name="breakablePoints">list of breakable points</param>
        public ScriptInfo(string content, string url, params BreakableScriptPoint[] breakablePoints)
        {
            this.Content = content;
            this.Url = url;

            var contentLines = this.Content.Split('n');

            this.StartLine = 0;
            this.EndLine = contentLines.Length - 1;

            this.StartColumn = 0;
            this.EndColumn = contentLines[this.EndLine].Length - 1;

            this.HashCode = content.GetHashCode().ToString();

            this.BreakableScriptPoint = breakablePoints.ToDictionary(_ => _.Name);
            MonitorBreablePoints(breakablePoints);

            this.scriptParsedEvent = new ScriptParsedEvent
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

        /// <summary>
        /// Get script id
        /// </summary>
        public int Id { get; } = ++NextID;

        /// <summary>
        /// Get script hash code
        /// </summary>

        public string HashCode { get; }

        /// <summary>
        /// Get the list of breakable point included into this script.
        /// </summary>
        public IReadOnlyDictionary<string, BreakableScriptPoint> BreakableScriptPoint { get; }

        /// <summary>
        /// Get script content
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Get script url (name)
        /// </summary>
        public string Url { get; }

        public int StartLine { get; }

        public int EndLine { get; }

        public int StartColumn { get; }

        public int EndColumn { get; }

        public ScriptParsedEvent Parse()
        {
            return scriptParsedEvent;
        }

        /// <summary>
        /// Event emitted when a breakable point was 'hit'        
        /// </summary>
        public event EventHandler<BreakPointHitEventArgs> BreakPointHit;

        /// <summary>
        /// Monitor the <see cref="BreakableScriptPoint.BreakPointHit" event and link it to our
        /// own <see cref="BreakPointHit"/> event/>
        /// </summary>
        /// <param name="breakablePoints"></param>
        private void MonitorBreablePoints(BreakableScriptPoint[] breakablePoints)
        {
            void breakPointWasHit(object sender, BreakPointHitEventArgs arg)
            {
                arg.Script = this;
                this.BreakPointHit.Invoke(this, arg);
            }

            foreach (var breakPoint in breakablePoints)
            {
                breakPoint.BreakPointHit += breakPointWasHit;
            }
        }
    }
}