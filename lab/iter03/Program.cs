using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace ConversationalMemoryAssistant;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Conversational Memory Assistant Lab ===\n");

        // Get API key from environment
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("❌ FAIL: OPENAI_API_KEY environment variable not set");
            return;
        }

        try
        {
            // Test 1: Initialize Memory
            Console.WriteLine("Test 1: Initializing semantic memory...");
            var memory = await LabTask.InitializeMemoryAsync(apiKey);
            
            // Verify memory has stored preferences by searching
            var testSearch = memory.SearchAsync("UserPreferences", "learning", limit: 10);
            int memoryCount = 0;
            await foreach (var item in testSearch)
            {
                memoryCount++;
            }
            
            if (memoryCount < 2)
            {
                Console.WriteLine($"❌ FAIL: TODO 1 not satisfied - Expected at least 2 user preferences in memory, found {memoryCount}");
                Console.WriteLine("See README section 'TODO 1 – Initialize Semantic Memory Store'");
                return;
            }
            Console.WriteLine($"✅ PASS: Memory initialized with {memoryCount} preferences\n");

            // Test 2: Kernel and Chat Service Setup
            Console.WriteLine("Test 2: Setting up kernel and chat service...");
            var kernel = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("gpt-4o-mini", apiKey)
                .Build();

            var chatService = kernel.GetRequiredService<IChatCompletionService>();
            Console.WriteLine("✅ PASS: Kernel and chat service ready\n");

            // Test 3: Run Conversation Loop
            Console.WriteLine("Test 3: Starting conversation loop...");
            Console.WriteLine("(This is interactive - type some questions and then 'exit' to finish)\n");
            
            await LabTask.RunConversationLoopAsync(kernel, memory, chatService);
            
            Console.WriteLine("\n✅ PASS: Conversation completed successfully");
            Console.WriteLine("\n=== All Tests Passed ===");
        }
        catch (NotImplementedException ex)
        {
            Console.WriteLine($"\n❌ FAIL: {ex.Message}");
            if (ex.Message.Contains("TODO 1"))
            {
                Console.WriteLine("See README section 'TODO 1 – Initialize Semantic Memory Store'");
            }
            else if (ex.Message.Contains("TODO 2"))
            {
                Console.WriteLine("See README section 'TODO 2 – Implement Conversation Loop with Memory Integration'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ FAIL: Unexpected error - {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}
