namespace ChromeDevTools.Analyzer;

using System.Collections.Generic;
using System.Linq;

public static class CSharpGenerator {
     public static string GenerateScript(string baseFileName, (string finalScript, IReadOnlyList<BreakPointInfo> breakpoints) info)
    {
        string @namespace  = null;
        var baseClassName = baseFileName;

        var namespaceSep = baseClassName.LastIndexOf(".", System.StringComparison.OrdinalIgnoreCase);
        if(namespaceSep != -1) {
            @namespace = baseFileName.Substring(0, namespaceSep);
            baseClassName = baseFileName.Substring(namespaceSep+1);
        }

        var output ="";

        if(@namespace != null) {
             output += $"namespace {@namespace} {{\n";
        }
        
        output += $"public partial class {baseClassName} : ChromeDevTools.Host.Handlers.Debugging.ScriptInfo\n";
        output += "{\n";
        output += "    public const string Script = @\"" + info.finalScript;
        output += "\";\n";
        output += "\n";
        output += $"    public {baseClassName}() : base(Script, nameof({baseClassName}),\n";
        output += string.Join(",\n",
            info.breakpoints.Select( _ => 
            {
                return $"        new ChromeDevTools.Host.Handlers.Debugging.BreakableScriptPoint({_.BreakpointName}, " + 
                       $"({_.Line}, {_.Column}, \"{_.FunctionName}\"))";
            }));
        output += "\n    )\n";
        output += "    {}\n";
        output += "}\n";

        if(@namespace != null) {
             output += "}\n";
        }

        return output;
    }
}
