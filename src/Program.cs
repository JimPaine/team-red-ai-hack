using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Memory.AzureCognitiveSearch;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Skills.Core;
using src;
using src.Plugins.Orchestrator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IKernel>(o =>
{
    var builder = Kernel.Builder;

    string getConfig(string key) => o.GetService<IConfiguration>()?.GetValue<string>(key) ?? throw new Exception($"{key} Configuration missing");

    builder.WithAzureChatCompletionService(
        getConfig("OpenAIModelId"),
        getConfig("AzureOpenAIEndpoint"),
        getConfig("AzureOpenAIKey"));

    builder.WithAzureTextEmbeddingGenerationService(
        "text-embedding-ada-002",
        getConfig("AzureOpenAIEndpoint"),
        getConfig("AzureOpenAIKey"));

    IMemoryStore store = new AzureCognitiveSearchMemoryStore(
        getConfig("AzureSearchEndpoint"),
        getConfig("AzureSearchApiKey")
    );

    builder.WithMemoryStorage(store);

    IKernel kernel = builder.Build();

    kernel.LoadSemanticPlugins(Directory.GetCurrentDirectory(), Path.Combine(Directory.GetCurrentDirectory(),"Plugins"));
    kernel.ImportSkill(new OrchestratorPlugin(kernel), "OrchestratorPlugin");
    kernel.ImportSkill(new TimeSkill(), "time");

    return kernel;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();