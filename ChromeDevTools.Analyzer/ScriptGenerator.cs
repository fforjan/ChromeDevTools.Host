namespace ChromeDevTools.Analyzer;

using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
[Generator]
public class ScriptGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var myFiles = context.AdditionalTextsProvider
            .Where(at => at.Path.EndsWith(".jsd"))
            .Select((file, cancellationToken) => new { file, content = file.GetText(cancellationToken) });

        context.RegisterSourceOutput(myFiles, (spc, file) =>
        {
            var className = Path.GetFileNameWithoutExtension(file.file.Path);

            var sourceText = SourceText.From(CSharpGenerator.GenerateScript(
                className,
                BreakPointInfo.ParseScript(file.content.ToString()))
                , Encoding.UTF8);

            spc.AddSource($@"{className}.g.cs", sourceText);
        });
    }
}
