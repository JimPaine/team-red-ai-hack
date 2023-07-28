using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Memory.AzureCognitiveSearch;
using Microsoft.SemanticKernel.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IKernel>(o =>
{
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

    IMemoryStore store = new AzureCognitiveSearchMemoryStore(
        getConfig("AzureSearchEndpoint"),
        getConfig("AzureSearchApiKey")
    );

    builder.WithMemoryStorage(store);

    IKernel kernel = builder.Build();
    // GetEmeddingsForBookOfNews(kernel).Wait(); // Uncomment to generate embeddings for Book of News

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

async Task GetEmeddingsForBookOfNews(IKernel kernel)
{

    int counter = 0;
    foreach (string chunk in ChunkTextFile("./bookofnews.txt", 140))
    {
        Console.WriteLine($"Chunk {counter}: {chunk}");

        await kernel.Memory.SaveInformationAsync("bookofnews", id: $"Chunk {counter++}",
            text: chunk);
    }
}


List<string> ChunkTextFile(string filePath, int recommendedLength)
{
    List<string> chunks = new List<string>();
    string text = File.ReadAllText(filePath);

    int startIndex = 0;
    while (startIndex < text.Length)
    {
        int endIndex = startIndex + recommendedLength;
        if (endIndex > text.Length) endIndex = text.Length;

        while (endIndex < text.Length && !char.IsWhiteSpace(text[endIndex]))
        {
            endIndex++;
        }
        string chunk = text.Substring(startIndex, endIndex - startIndex);
        chunks.Add(chunk.Trim());
        startIndex = endIndex;
    }
    return chunks;
}