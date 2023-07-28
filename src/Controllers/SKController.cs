using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;

namespace src.Controllers;

[ApiController]
[Route("[controller]")]
public class SKController : ControllerBase {

    private readonly IKernel kernel;
    public SKController(IKernel kernel) {
        this.kernel = kernel;
    }

    [HttpGet(Name = "GetJoke/style/{style}/topic/{topic}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<string> Get(string style, string topic)
    {
         var promptConfig = PromptTemplateConfig.FromJson(System.IO.File.ReadAllText("./Skills/Joke/config.json"));
        var template = new PromptTemplate(System.IO.File.ReadAllText("./Skills/Joke/skprompt.txt"), promptConfig, this.kernel.PromptTemplateEngine);
        var function = new SemanticFunctionConfig(promptConfig, template);

        var func = kernel.RegisterSemanticFunction("Joke", function);

        var inputs = new ContextVariables();
        inputs.TryAdd("style", style);
        inputs.TryAdd("topic", topic);

        var response = await kernel.RunAsync(inputs, func);

        return response.Result;
    }
}