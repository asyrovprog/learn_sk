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
    public static async System.Threading.Tasks.Task<ISemanticTextMemory> InitializeMemoryAsync(string apiKey)
    {
        // [YOUR CODE GOES HERE]
        // 1. Create VolatileMemoryStore instance
        // 2. Create MemoryBuilder with memory store and OpenAI embedding generation
        // 3. Build the semantic memory instance
        // 4. Save at least 2 user preferences to the "UserPreferences" collection
        // 5. Return the configured semantic memory
        
        throw new NotImplementedException("TODO 1: Initialize Semantic Memory Store – see README.md section 'TODO 1 – Initialize Semantic Memory Store'");
    }

    /// <summary>
    /// TODO 2: Implement Conversation Loop with Semantic Search
    /// See README.md section "TODO 2 – Implement Conversation Loop with Semantic Search" for detailed instructions
    /// </summary>
    public static async System.Threading.Tasks.Task RunConversationLoopAsync(
        Kernel kernel,
        ISemanticTextMemory memory,
        IChatCompletionService chatService)
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
        
        throw new NotImplementedException("TODO 2: Implement Conversation Loop with Semantic Search – see README.md section 'TODO 2 – Implement Conversation Loop with Semantic Search'");
    }
}
