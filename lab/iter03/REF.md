# Reference & Hints

## Helpful Resources

### Package Requirements
```bash
dotnet add package Microsoft.SemanticKernel
dotnet add package Microsoft.SemanticKernel.Plugins.Memory --prerelease
```

### Required Namespaces
```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
```

### Suppress Experimental Warnings
```csharp
#pragma warning disable SKEXP0001  // Type is for evaluation purposes only
#pragma warning disable SKEXP0010  // Type is for evaluation purposes only
#pragma warning disable SKEXP0050  // Type is for evaluation purposes only
```

---

## TODO 1 Hints: Initialize Semantic Memory

### Creating VolatileMemoryStore
```csharp
var memoryStore = new VolatileMemoryStore();
```

### Building Semantic Memory with MemoryBuilder
```csharp
var memory = new MemoryBuilder()
    .WithMemoryStore(memoryStore)
    .WithOpenAITextEmbeddingGeneration("text-embedding-ada-002", apiKey)
    .Build();
```

### Saving Information to Memory
```csharp
await memory.SaveInformationAsync(
    collection: "CollectionName",
    text: "The text to store as embedding",
    id: "unique_id",
    description: "Description of this memory"
);
```

### What Happens When You Save?
1. Your text is sent to OpenAI's embedding API
2. Returns a 1536-dimensional vector representing semantic meaning
3. Vector is stored in VolatileMemoryStore with the ID
4. Later searches use cosine similarity against these vectors

---

## TODO 2 Hints: Conversation Loop with Semantic Search

### Creating ChatHistory
```csharp
var history = new ChatHistory();
history.AddSystemMessage("Your system instructions here");
```

### Semantic Memory Search
```csharp
var results = memory.SearchAsync(
    collection: "CollectionName",
    query: userInput,
    limit: 2,
    minRelevanceScore: 0.7
);

await foreach (var result in results)
{
    // result.Metadata.Text contains the stored text
    // result.Relevance is the similarity score (0.0 to 1.0)
}
```

### Building Context String
```csharp
var contextBuilder = new StringBuilder();
await foreach (var result in results)
{
    contextBuilder.AppendLine($"- {result.Metadata.Text} (relevance: {result.Relevance:F2})");
}
```

### Adding Message with Context to ChatHistory
```csharp
history.AddUserMessage($@"User Query: {userInput}

Relevant User Context:
{context}");
```

### Getting AI Response
```csharp
var settings = new OpenAIPromptExecutionSettings
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
};

var response = await chatService.GetChatMessageContentAsync(history, settings, kernel);
history.AddMessage(response.Role, response.Content);
Console.WriteLine($"\nAssistant: {response.Content}\n");
```

### Loop Structure
```csharp
while (true)
{
    Console.Write("You: ");
    string? userInput = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(userInput) || 
        userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }
    
    // Search memory, build context, get response...
}
```

---

## Understanding Semantic Search

### How It Works
1. User enters query: "What am I learning?"
2. Query is converted to embedding vector (1536 dimensions)
3. VolatileMemoryStore compares query vector to all stored vectors
4. Returns results ranked by cosine similarity
5. Relevance scores show how semantically similar each result is

### Why This Is Powerful
- Query: "machine learning" matches stored: "AI and artificial intelligence"
- Works by **meaning**, not keywords
- Same technology used in ChatGPT, Copilot, RAG systems

### Relevance Score Interpretation
- 0.9 - 1.0: Very similar meaning
- 0.8 - 0.9: Similar meaning
- 0.7 - 0.8: Somewhat related
- Below 0.7: Not very relevant (filtered out in this lab)

---

## Common Pitfalls

1. **Forgetting to await memory operations**: `SaveInformationAsync()` and `SearchAsync()` are async
2. **Not handling empty search results**: Searches may return no results if relevance < 0.7
3. **Mixing up collection names**: Must use same collection for save and search
4. **Not maintaining ChatHistory**: Create once, reuse across all turns
5. **Hardcoding API key**: Should use environment variables or config

---

## Testing Your Implementation

### Verify Memory Storage
After initialization, try searching for something generic:
```csharp
var testResults = memory.SearchAsync("UserPreferences", "learning", limit: 5, minRelevanceScore: 0.1);
await foreach (var r in testResults)
{
    Console.WriteLine($"Found: {r.Metadata.Id} with score {r.Relevance}");
}
```

### Test Semantic Understanding
Try queries that don't match exact words:
- Store: "loves machine learning"
- Query: "interested in AI" → should still match!

---

<details>
<summary><strong>Full Reference Solution (Click to Expand)</strong></summary>

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using System.Text;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0050

namespace ConversationalMemoryAssistant;

public class Task
{
    public static async Task<ISemanticTextMemory> InitializeMemoryAsync(string apiKey)
    {
        var memoryStore = new VolatileMemoryStore();
        
        var memory = new MemoryBuilder()
            .WithMemoryStore(memoryStore)
            .WithOpenAITextEmbeddingGeneration("text-embedding-ada-002", apiKey)
            .Build();

        const string collectionName = "UserPreferences";

        await memory.SaveInformationAsync(
            collection: collectionName,
            text: "User loves learning about AI and machine learning. Prefers detailed technical explanations.",
            id: "pref_learning_style",
            description: "User's learning preferences"
        );

        await memory.SaveInformationAsync(
            collection: collectionName,
            text: "User is working on a project involving semantic search and vector databases.",
            id: "pref_current_project",
            description: "User's current project context"
        );

        return memory;
    }

    public static async System.Threading.Tasks.Task RunConversationLoopAsync(
        Kernel kernel,
        IChatCompletionService chatService,
        ISemanticTextMemory memory)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(
            "You are a helpful learning assistant. Use the provided context about the user's " +
            "preferences and background to provide personalized, relevant responses. " +
            "Reference the user's interests and current work when appropriate."
        );

        Console.WriteLine("=== Conversational Assistant with Semantic Memory ===");
        Console.WriteLine("Type 'exit' or 'quit' to end the conversation.\n");

        while (true)
        {
            Console.Write("You: ");
            string? userInput = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(userInput) || 
                userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Goodbye!");
                break;
            }

            var searchResults = memory.SearchAsync(
                collection: "UserPreferences",
                query: userInput,
                limit: 2,
                minRelevanceScore: 0.7
            );

            var contextBuilder = new StringBuilder();
            await foreach (var result in searchResults)
            {
                contextBuilder.AppendLine($"- {result.Metadata.Text} (relevance: {result.Relevance:F2})");
            }

            string context = contextBuilder.Length > 0 
                ? contextBuilder.ToString() 
                : "No relevant context found.";

            history.AddUserMessage($@"User Query: {userInput}

Relevant User Context:
{context}");

            var settings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var response = await chatService.GetChatMessageContentAsync(history, settings, kernel);
            history.AddMessage(response.Role, response.Content ?? string.Empty);
            
            Console.WriteLine($"\nAssistant: {response.Content}\n");
        }
    }
}
```

</details>
