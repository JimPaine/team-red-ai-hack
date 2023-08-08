using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Memory;

namespace src.Plugins.Orchestrator;

public interface IOrchestratorPlugin {
    [SKFunction]
    Task<string> Execute(SKContext context);
}
public class OrchestratorPlugin : IOrchestratorPlugin {

    private readonly IKernel kernel;
    public OrchestratorPlugin(IKernel kernel) {
        this.kernel = kernel;
    }

    [SKFunction]
    public async Task<string> Execute(SKContext context) {
        string input = context.Variables["input"];

        var getIntent = this.kernel.Skills.GetFunction("Orchestrator", "GetIntent");
        Task<SKContext> response = getIntent.InvokeAsync(input);
        var memories = this.kernel.Memory.SearchAsync("bookofnews", input, 5);

        string myMemory = "";
        await foreach (MemoryQueryResult memory in memories)
        {
            myMemory += memory.Metadata.Text + " ";
        }
        await response;

        var inputs = new ContextVariables(input);
        inputs.TryAdd("memories", myMemory);

        ISKFunction? intentFunction = null;
        switch(response.Result.Result) {
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

        ISKFunction search = this.kernel.Skills.GetFunction("Plugins", "SearchMemories");
        var searchResponse = intentFunction != null
            ? await this.kernel.RunAsync(inputs, search, intentFunction)
            : await this.kernel.RunAsync(inputs, search);

        return searchResponse.Result;
    }
}