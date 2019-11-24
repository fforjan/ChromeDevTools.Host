namespace ChromeDevTools.Host.Handlers.Debugging
{
    using System;
    using ChromeDevTools.Host.Runtime.Debugger;

    /// <summary>
    /// Information when a breakable point was hit
    /// </summary>
    public class BreakPointHitEventArgs : EventArgs
    {
        /// <summary>
        /// Breakable point info
        /// </summary>
        public BreakableScriptPoint BreakPoint { get; set; }

        /// <summary>
        /// Script info
        /// </summary>
        public ScriptInfo Script { get; set; }

        /// <summary>
        /// Reason for emitting this event
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Convert this event information to a <see cref="PausedEvent"/> event.
        /// </summary>
        /// <returns></returns>
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