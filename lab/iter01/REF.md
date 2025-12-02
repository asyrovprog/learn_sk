# Reference and Hints

## Hint for TODO 1 – Initialize Kernel with Chat Completion Service

**Key Concepts:**
- The Kernel is the central orchestrator in Semantic Kernel
- Chat completion services enable conversational AI capabilities
- The builder pattern is used to configure the kernel

**Helpful Pointers:**
- Look at the Kernel.CreateBuilder() method
- The AddOpenAIChatCompletion extension method takes: serviceId, modelId, and apiKey
- Don't forget to call Build() on the builder

**Common Mistakes:**
- Forgetting to call Build() after adding services
- Incorrect parameter order in AddOpenAIChatCompletion

---

## Hint for TODO 2 – Manage Chat History

**Key Concepts:**
- ChatHistory maintains the conversation context
- Streaming allows real-time display of responses
- Both user and assistant messages must be added to maintain context

**Helpful Pointers:**
- Use _chatHistory.AddUserMessage() for user input
- Get the chat service from kernel using GetRequiredService<IChatCompletionService>()
- Stream the response and collect all chunks with StringBuilder
- Don't forget to add the complete assistant response to _chatHistory

**Common Mistakes:**
- Only adding user messages but not assistant responses
- Not collecting all streaming chunks before returning
- Forgetting to return the complete response string

---

## Hint for TODO 3 – Display Conversation History

**Key Concepts:**
- ChatHistory contains a collection of ChatMessageContent objects
- Each message has a Role property (User or Assistant) and Content

**Helpful Pointers:**
- Iterate through _chatHistory
- Access message.Role and message.Content
- Use Console.WriteLine for output with clear formatting

**Common Mistakes:**
- Not handling empty content
- Poor formatting making conversation hard to read

---

<details>
<summary>Reference Solution (open after completion)</summary>

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace ConversationalMemoryLab;

public class ChatManager
{
    private readonly string _apiKey;
    private readonly ChatHistory _chatHistory;

    public ChatManager(string apiKey)
    {
        _apiKey = apiKey;
        _chatHistory = new ChatHistory();
    }

    // TODO[1]: Initialize Kernel with Chat Completion Service
    public Kernel CreateKernel()
    {
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(
            serviceId: "chat",
            modelId: "gpt-4o-mini",
            apiKey: _apiKey);
        return builder.Build();
    }

    // TODO[2]: Manage Chat History
    public async Task<string> SendMessageAsync(Kernel kernel, string userMessage)
    {
        _chatHistory.AddUserMessage(userMessage);
        
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var response = new System.Text.StringBuilder();
        
        await foreach (var chunk in chatService.GetStreamingChatMessageContentsAsync(_chatHistory))
        {
            response.Append(chunk.Content);
        }
        
        var completeResponse = response.ToString();
        _chatHistory.AddAssistantMessage(completeResponse);
        
        return completeResponse;
    }

    // TODO[3]: Display Conversation History
    public void DisplayHistory()
    {
        foreach (var message in _chatHistory)
        {
            Console.WriteLine($"[{message.Role}]: {message.Content}");
            Console.WriteLine(new string('-', 50));
        }
    }
}
```

</details>
