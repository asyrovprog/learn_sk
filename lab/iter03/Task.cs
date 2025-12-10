#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0050

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

namespace ConversationalMemoryAssistant;

public static class LabTask
{
    /// <summary>
    /// TODO 1: Initialize Semantic Memory Store
    /// See README.md section "TODO 1 – Initialize Semantic Memory Store" for detailed instructions
    /// </summary>
    public static async Task<ISemanticTextMemory> InitializeMemoryAsync(string apiKey)
    {
        // [YOUR CODE GOES HERE]
        // 1. Create VolatileMemoryStore instance
        // 2. Create MemoryBuilder with memory store and OpenAI embedding generation
        // 3. Build the semantic memory instance
        // 4. Save at least 2 user preferences to the "UserPreferences" collection
        // 5. Return the configured semantic memory
        var memoryStore = new VolatileMemoryStore();
        var memoryBuilder = new MemoryBuilder();
        memoryBuilder.WithMemoryStore(memoryStore);
        memoryBuilder.WithOpenAITextEmbeddingGeneration(modelId: "text-embedding-ada-002", apiKey: apiKey);
        
        var memory = memoryBuilder.Build();
        await memory.SaveInformationAsync(
            collection: "UserPreferences",
            text: "User loves learning about AI and machine learning. Prefers detailed technical explanations.",
            id: "pref_learning_style",
            description: "User's learning preferences"
        ).ConfigureAwait(false);

        await memory.SaveInformationAsync(
            collection: "UserPreferences",
            text: "User is working on a project involving semantic search and vector databases.",
            description: "User's current project context",
            id: "pref_current_project"
        ).ConfigureAwait(false);

        return memory;
    }

    /// <summary>
    /// TODO 2: Implement Conversation Loop with Semantic Search
    /// See README.md section "TODO 2 – Implement Conversation Loop with Semantic Search" for detailed instructions
    /// </summary>
    public static async Task<string[]> RunConversationLoopAsync(
        Kernel kernel,
        ISemanticTextMemory memory,
        IChatCompletionService chatService,
        string[] inputs)
    {
        // [YOUR CODE GOES HERE]
        // 1. Create ChatHistory instance
        // 2. Add system message instructing AI to use user context
        // 3. Start infinite conversation loop:
        //    - Prompt user for input
        //    - Exit if "exit" or "quit"
        //    - Search semantic memory with memory.SearchAsync()
        //    - Build context string from search results with relevance scores
        //    - Add user message with context to ChatHistory
        //    - Get AI response with ToolCallBehavior.AutoInvokeKernelFunctions
        //    - Add assistant response to ChatHistory
        //    - Display response to console
        // 4. Loop until user exits
        
        var history = new ChatHistory();
        var responses = new List<string>();

        history.AddSystemMessage(@"
            - Act as a helpful learning assistant.
            - Use provided context about user preferences.
            - Provide personalized responses based on user's background.
        ");

        if (inputs == null || inputs.Length == 0)
        {
            throw new ArgumentException("Inputs array cannot be null or empty", nameof(inputs));
        }
        
        foreach (var input in inputs)
        {
            Console.WriteLine($"User: {input}");
            
            // Search semantic memory for relevant context
            var contextBuilder = new System.Text.StringBuilder();
            await foreach(var info in memory.SearchAsync(
                collection: "UserPreferences",
                query: input,
                limit: 2,
                minRelevanceScore: 0.7).ConfigureAwait(false))
            {
                contextBuilder.AppendLine($"- {info.Metadata.Text} (relevance: {info.Relevance:F2})");
            }
            
            // Build user message with context
            var context = contextBuilder.ToString();
            var userMessage = string.IsNullOrEmpty(context)
                ? $"User Query: {input}"
                : $"User Query: {input}\n\nRelevant User Context:\n{context}";
                
            history.AddUserMessage(userMessage);

            // Get AI response
            var response = await chatService.GetChatMessageContentAsync(
                chatHistory: history,
                kernel: kernel,
                executionSettings: new OpenAIPromptExecutionSettings()
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
                }).ConfigureAwait(false);

            var responseText = response.Content ?? string.Empty;
            history.AddAssistantMessage(responseText);
            responses.Add(responseText);
            
            Console.WriteLine($"Assistant: {responseText}\n");
        }
        
        return responses.ToArray();
    }
}
