using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;

namespace src.Controllers;

[ApiController]
[Route("[controller]")]
public class EmbeddingController : ControllerBase
{
    private readonly IKernel kernel;
    public EmbeddingController(IKernel kernel)
    {
        this.kernel = kernel;
    }

    [HttpGet]
    public async Task Get()
    {
        await GetEmeddingsForBookOfNews();
    }

    private async Task GetEmeddingsForBookOfNews()
    {
        int counter = 0;
        foreach (string chunk in ChunkTextFile("./bookofnews.txt", 140))
        {
            Console.WriteLine($"Chunk {counter}: {chunk}");

            await this.kernel.Memory.SaveInformationAsync("bookofnews", id: $"Chunk {counter++}",
                text: chunk);
        }
    }

    private List<string> ChunkTextFile(string filePath, int recommendedLength)
    {
        List<string> chunks = new List<string>();
        string text = System.IO.File.ReadAllText(filePath);

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
}