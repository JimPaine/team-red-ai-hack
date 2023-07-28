using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SemanticFunctions;
using Microsoft.SemanticKernel.SkillDefinition;

namespace src.Controllers;

[ApiController]
[Route("[controller]")]
public class SKController : ControllerBase
{

    private readonly IKernel kernel;
    private ISKFunction jokeFunction;
    private ISKFunction bookOfNewsFunction;
    public SKController(IKernel kernel)
    {
        this.kernel = kernel;

        var promptConfig = PromptTemplateConfig.FromJson(System.IO.File.ReadAllText("./Skills/Joke/config.json"));
        var template = new PromptTemplate(System.IO.File.ReadAllText("./Skills/Joke/skprompt.txt"), promptConfig, this.kernel.PromptTemplateEngine);
        var function = new SemanticFunctionConfig(promptConfig, template);

        this.jokeFunction = kernel.RegisterSemanticFunction("Joke", function);

        var bookOfNewsPromptConfig = PromptTemplateConfig.FromJson(System.IO.File.ReadAllText("./Skills/BookOfNews/config.json"));
        var bookOfNewsTemplate = new PromptTemplate(System.IO.File.ReadAllText("./Skills/BookOfNews/skprompt.txt"), bookOfNewsPromptConfig, this.kernel.PromptTemplateEngine);
        var bookOfNewsFunction = new SemanticFunctionConfig(bookOfNewsPromptConfig, bookOfNewsTemplate);

        this.bookOfNewsFunction = kernel.RegisterSemanticFunction("BookOfNews", bookOfNewsFunction);
    }

    // [HttpGet(Name = "GetJoke/style/{style}/topic/{topic}")]
    // [ProducesResponseType(StatusCodes.Status200OK)]
    // public async Task<string> Get(string style, string topic)
    // {
    //     var inputs = new ContextVariables();
    //     inputs.TryAdd("style", style);
    //     inputs.TryAdd("topic", topic);

    //     var response = await this.kernel.RunAsync(inputs, this.jokeFunction);

    //     return response.Result;
    // }

    [HttpGet(Name = "{search}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<string> BookOfNews(string search)
    {
        var memories = kernel.Memory.SearchAsync("bookofnews", search, 5);
        string myMemory = "";
        await foreach (MemoryQueryResult memory in memories)
        {
            myMemory += memory.Metadata.Text + " ";
        }

        Console.WriteLine("Memory to feed back into the prompt will be:\n  >> " + myMemory + "\n");

        var inputs = new ContextVariables();
        inputs.TryAdd("memories", myMemory);
        inputs.TryAdd("ask", search);


        var bookOfNews = await this.kernel.RunAsync(inputs, this.bookOfNewsFunction);

        return bookOfNews.Result;
    }
}