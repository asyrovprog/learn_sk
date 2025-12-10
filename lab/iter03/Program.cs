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

            // Test 3: Automated conversation test with semantic search
            Console.WriteLine("Test 3: Testing conversation with semantic search...");
            
            var testInputs = new[]
            {
                "What am I learning about?",
                "Tell me about my current project"
            };
            
            var responses = await LabTask.RunConversationLoopAsync(kernel, memory, chatService, testInputs);
            
            if (responses.Length != testInputs.Length)
            {
                Console.WriteLine($"❌ FAIL: TODO 2 - Expected {testInputs.Length} responses, got {responses.Length}");
                Console.WriteLine("See README section 'TODO 2 – Implement Conversation Loop with Semantic Search'");
                return;
            }
            
            // Verify responses contain relevant information from memory
            bool hasLearningReference = responses[0].Contains("AI", StringComparison.OrdinalIgnoreCase) || 
                                       responses[0].Contains("machine learning", StringComparison.OrdinalIgnoreCase) ||
                                       responses[0].Contains("learning", StringComparison.OrdinalIgnoreCase);
                                       
            bool hasProjectReference = responses[1].Contains("semantic", StringComparison.OrdinalIgnoreCase) || 
                                      responses[1].Contains("vector", StringComparison.OrdinalIgnoreCase) ||
                                      responses[1].Contains("database", StringComparison.OrdinalIgnoreCase) ||
                                      responses[1].Contains("project", StringComparison.OrdinalIgnoreCase);
            
            if (!hasLearningReference)
            {
                Console.WriteLine("❌ FAIL: TODO 2 - Response doesn't reference stored learning preferences");
                Console.WriteLine($"Response: {responses[0]}");
                Console.WriteLine("See README section 'TODO 2 – Implement Conversation Loop with Semantic Search'");
                return;
            }
            
            if (!hasProjectReference)
            {
                Console.WriteLine("❌ FAIL: TODO 2 - Response doesn't reference stored project information");
                Console.WriteLine($"Response: {responses[1]}");
                Console.WriteLine("See README section 'TODO 2 – Implement Conversation Loop with Semantic Search'");
                return;
            }
            
            Console.WriteLine("✅ PASS: Conversation responses use semantic memory context\n");
            
            Console.WriteLine("=== All Tests Passed ===");
            Console.WriteLine("\n🎓 Lab complete! Your implementation:");
            Console.WriteLine("  ✓ Creates semantic memory with real embeddings");
            Console.WriteLine("  ✓ Stores user preferences as vectors");
            Console.WriteLine("  ✓ Performs vector similarity search");
            Console.WriteLine("  ✓ Integrates memory context into conversations");
            Console.WriteLine("  ✓ Returns personalized responses based on stored information");
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
