using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Memory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace src.Plugins.Orchestrator;

public interface IOrchestratorPlugin {
    [SKFunction]
    Task<string> Execute(SKContext context);
}
public class OrchestratorPlugin : IOrchestratorPlugin {

    private readonly IKernel kernel;
    private readonly MemoryCache cache;

    public OrchestratorPlugin(IKernel kernel) {
        this.kernel = kernel;
        this.cache = new MemoryCache(Options.Create(new MemoryCacheOptions {}));
    }

    [SKFunction]
    public async Task<string> Execute(SKContext context) {
        string input = context.Variables["input"];

        if(this.cache.TryGetValue<string>(input.GetHashCode(), out string? result)) { return result; }

        var getIntent = this.kernel.Skills.GetFunction("Orchestrator", "GetIntent");
        var response = await getIntent.InvokeAsync(input);

        var inputs = new ContextVariables(input);

        ISKFunction? intentFunction = null;
        switch(response.Result) {
            case "Poem":
                intentFunction = this.kernel.Skills.GetFunction("Plugins", "Poem");
                break;
            case "Joke":
                intentFunction = this.kernel.Skills.GetFunction("Plugins", "Joke");
                inputs.TryAdd("style", "technology based");
                break;
            default:
                break;
        }

        ISKFunction search = this.kernel.Skills.GetFunction("memory", "recall");
        inputs.TryAdd("collection", "bookofnews");
        inputs.TryAdd("relevance", "0.7");
        inputs.TryAdd("limit", "5");
        var searchResponse = intentFunction != null
            ? await this.kernel.RunAsync(inputs, search, intentFunction)
            : await this.kernel.RunAsync(inputs, search);

        this.cache.Set<string>(input.GetHashCode(), searchResponse.Result);
        return searchResponse.Result;
    }
}