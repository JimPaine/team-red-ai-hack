using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IKernel>(o => {
    var builder = Kernel.Builder;

    Func<string, string> getConfig = key => o.GetService<IConfiguration>()?.GetValue<string>(key) ?? throw new Exception($"Azure OpenAI Configuration missing");

    builder.WithAzureChatCompletionService(
        getConfig("OpenAIModelId"),
        getConfig("AzureOpenAIEndpoint"),
        getConfig("AzureOpenAIKey"));

    builder.WithAzureTextEmbeddingGenerationService(
        "text-embedding-ada-002",
        getConfig("AzureOpenAIEndpoint"),
        getConfig("AzureOpenAIKey"));

    builder.WithMemoryStorage(new VolatileMemoryStore());

    return builder.Build();
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
