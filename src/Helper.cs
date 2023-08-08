using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SemanticFunctions;

namespace src;

public static class KernelHelper {
    public static void LoadSemanticPlugins(this IKernel kernel, string parent, string current) {
        string[] files = Directory.GetFiles(current);
        string? config = files.FirstOrDefault(x => x.Contains("config.json"));
        string? template = files.FirstOrDefault(x => x.Contains("skprompt.txt"));

        if (template != null)
        {
            PromptTemplateConfig promptConfig = config != null ? PromptTemplateConfig.FromJson(File.ReadAllText(config)) : new PromptTemplateConfig();
            PromptTemplate promptTemplate = new(File.ReadAllText(template), promptConfig, kernel.PromptTemplateEngine);
            SemanticFunctionConfig function = new(promptConfig, promptTemplate);

            kernel.RegisterSemanticFunction(Path.GetFileNameWithoutExtension(parent), Path.GetFileNameWithoutExtension(current), function);
        }
        foreach (var child in Directory.GetDirectories(current))
        {
            LoadSemanticPlugins(kernel, current, child);
        }
    }
}