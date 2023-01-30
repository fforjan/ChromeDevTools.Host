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

            // do some transforms based on the file context
            string output = "public class Dummy {}";


            var sourceText = SourceText.From(output, Encoding.UTF8);

            var filenameBase = Path.GetFileNameWithoutExtension(file.Path);

            context.AddSource($@"{filenameBase}.g.cs", sourceText);
        }
    }
}
