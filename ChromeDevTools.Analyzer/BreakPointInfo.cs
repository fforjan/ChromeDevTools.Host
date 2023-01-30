namespace ChromeDevTools.Analyzer;

using System.Collections.Generic;
using System.Linq;

public class BreakPointInfo {

    public int Line { get; set; }
    public int Column { get; set; }

    public string BreakpointName { get; set; }

    public string FunctionName { get; set; }

    public static (string finalScript, IReadOnlyList<BreakPointInfo> breakpoints) ParseScript(string script)
    {
        var finalScript = string.Empty;
        var breakPoints = new List<BreakPointInfo>();

        var currentLine = 0;
        var currentColumn = 1;

        int i = 0;
        while(i < script.Length)
        {
            var character = script[i];
            if (character == '@')
            {
                breakPoints.Add(ParseBreakPoint(ref i, currentLine, currentColumn));
            }
            else
            {
                if (character == '\n')
                {
                    currentLine++;
                    currentColumn = 1;
                }
                if (character == '\n')

                {
                    currentColumn = 1;
                }
                else
                {
                    currentColumn++;
                }
                finalScript += character;
                ++i;
            }
        }

        return (finalScript, breakPoints);

        BreakPointInfo ParseBreakPoint(ref int i, int line, int column)
        {
            i++;
            var breakPointName = string.Empty;
            while (script[i] != '@')
            {
                breakPointName += script[i];
                i++;
            }

            var functionIndex = ++i;
            var functionName = string.Empty;
            while (char.IsLetterOrDigit(script[functionIndex]))
            {
                functionName += script[functionIndex++];
            }

            return new BreakPointInfo
            {
                Line = line,
                Column = column,
                BreakpointName = breakPointName,
                FunctionName = string.IsNullOrWhiteSpace(functionName)
                    ? breakPointName
                    : functionName
            };
        }
    }
}