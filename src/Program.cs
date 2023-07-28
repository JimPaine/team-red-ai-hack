using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IKernel>(o => {
    var builder = Kernel.Builder;

    Func<string, string> getConfig = key => o.GetService<IConfiguration>()?.GetValue<string>(key) ?? throw new Exception($"Azure OpenAI Configuration missing");

    builder.WithAzureChatCompletionService(
        getConfig("OpenAIModelId"),
        getConfig("AzureOpenAIEndpoint"),
        getConfig("AzureOpenAIKey"));

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
