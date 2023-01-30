namespace ChromeDevTools.Analyzer;

using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

[Generator]
public class ScriptGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        // find anything that matches our files
        var myFiles = context.AdditionalFiles.Where(at => at.Path.EndsWith(".jsd"));
        
        foreach (var file in myFiles)
        {
            var content = file.GetText(context.CancellationToken);

             var className = Path.GetFileNameWithoutExtension(file.Path);

            var sourceText = SourceText.From(CSharpGenerator.GenerateScript(
                className,
                BreakPointInfo.ParseScript(content.ToString()))
                , Encoding.UTF8);

            context.AddSource($@"{className}.g.cs", sourceText);
        }
    }

   
}
