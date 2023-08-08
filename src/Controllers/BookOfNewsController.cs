using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SkillDefinition;

namespace src.Controllers;

[ApiController]
[Route("[controller]")]
public class BookOfNewsController : ControllerBase
{
    private readonly ISKFunction orchestrator;

    public BookOfNewsController(IKernel kernel)
    {
        this.orchestrator = kernel.Skills.GetFunction("OrchestratorPlugin", "Execute");
    }

    [HttpGet(Name = "{search}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<string> BookOfNews(string search)
    {
        var response = await this.orchestrator.InvokeAsync(search);
        return response.Result;
    }
}