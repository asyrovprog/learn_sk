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
    // Objective: Set up a Semantic Kernel instance configured with OpenAI chat completion.
    // [YOUR CODE GOES HERE]
    // Instructions:
    // 1. Create a Kernel builder
    // 2. Add the OpenAI chat completion service using the provided API key and model name
    // 3. Build and return the configured kernel
    // Requirements:
    // - Use the AddOpenAIChatCompletion method
    // - Service ID should be "chat"
    // - Model should be "gpt-4o-mini"
    public Kernel CreateKernel()
    {
        var kernel = Kernel
            .CreateBuilder()
            .AddOpenAIChatCompletion(
                modelId: "gpt-5-mini",
                apiKey: _apiKey)
            .Build();

        return kernel;
    }

    // TODO[2]: Manage Chat History
    // Objective: Implement a function that adds user and assistant messages to chat history and maintains conversation context.
    // [YOUR CODE GOES HERE]
    // Instructions:
    // 1. Add the user's message to the ChatHistory
    // 2. Get a streaming response from the chat completion service
    // 3. Collect the streamed response chunks into a complete message
    // 4. Add the assistant's response to ChatHistory
    // 5. Return the complete assistant response
    // Requirements:
    // - Use GetRequiredService<IChatCompletionService>() to get the chat service
    // - Use GetStreamingChatMessageContentsAsync() for streaming
    // - Properly iterate through all message chunks
    // - Add both user and assistant messages to history
    public async Task<string> SendMessageAsync(Kernel kernel, string userMessage)
    {
        _chatHistory.AddUserMessage(userMessage);

        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        var contents = await chatService.GetChatMessageContentsAsync(
            chatHistory: _chatHistory,
            kernel: kernel);

        var result = string.Empty;
        _chatHistory.AddRange(contents);

        foreach (var content in contents)
        {
            result += content?.Content ?? "";
        }

        return result;
    }    
    
    // TODO[3]: Display Conversation History
    // Objective: Create a method to display the entire conversation history in a readable format.
    // [YOUR CODE GOES HERE]
    // Instructions:
    // 1. Iterate through all messages in ChatHistory
    // 2. For each message, print the role (User/Assistant) and the content
    // 3. Add visual separators for readability
    // Requirements:
    // - Handle both User and Assistant roles
    // - Format output clearly with role labels
    // - Add separators between messages
    public void DisplayHistory()
    {
        foreach (var item in _chatHistory)
        {
            Console.WriteLine($"{item.Role}: {item.InnerContent}");
            Console.WriteLine("---");
        }
    }
}
