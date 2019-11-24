namespace ChromeDevTools.Host.Handlers.Debugging
{
    using System;
    using ChromeDevTools.Host.Runtime.Debugger;

    public class BreakPointHitEventArgs : EventArgs
    {
        public BreakableScriptPoint BreakPoint { get; set; }
        public ScriptInfo Script { get; set; }

        public string Reason { get; set; }

        public PausedEvent AsEvent()
        {
            var pausedEvent = new PausedEvent
            {
                HitBreakpoints = new[] { BreakPoint.GetBreakPointName(Script) },
                Reason = Reason ?? "breakpoint",
                CallFrames = BreakPoint.GetCallFrame(Script)
            };

            return pausedEvent;
        }
    }
}