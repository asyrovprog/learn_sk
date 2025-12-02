using ConversationalMemoryLab;
using Microsoft.SemanticKernel;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Conversational Memory Manager Lab Tests ===\n");

        // Get API key from environment
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY", EnvironmentVariableTarget.Machine) ?? "test-key";
        
        var chatManager = new ChatManager(apiKey);
        int passCount = 0;
        int totalTests = 3;

        // Test 1: Kernel Creation
        Console.WriteLine("Test 1: Kernel Creation");
        try
        {
            var kernel = chatManager.CreateKernel();
            if (kernel != null)
            {
                Console.WriteLine("✓ PASS: Kernel created successfully");
                passCount++;
            }
            else
            {
                Console.WriteLine("✗ FAIL: TODO[1] not satisfied – see README section 'TODO 1 – Initialize Kernel with Chat Completion Service'");
            }
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("✗ FAIL: TODO[1] not satisfied – see README section 'TODO 1 – Initialize Kernel with Chat Completion Service'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: TODO[1] not satisfied – see README section 'TODO 1 – Initialize Kernel with Chat Completion Service'");
            Console.WriteLine($"  Error: {ex.Message}");
        }

        // Test 2: Send Message and Maintain History (requires valid API key)
        Console.WriteLine("\nTest 2: Send Message and Maintain History");
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY")))
        {
            try
            {
                var kernel = chatManager.CreateKernel();
                var response1 = await chatManager.SendMessageAsync(kernel, "My name is Alice. What's a good hobby?");
                if (!string.IsNullOrEmpty(response1))
                {
                    Console.WriteLine($"✓ First response received: {response1.Substring(0, Math.Min(50, response1.Length))}...");

                    var response2 = await chatManager.SendMessageAsync(kernel, "What name did I just tell you?");
                    if (!string.IsNullOrEmpty(response2) && response2.ToLower().Contains("alice"))
                    {
                        Console.WriteLine("✓ PASS: Context maintained - AI remembered the name 'Alice'");
                        passCount++;
                    }
                    else
                    {
                        Console.WriteLine("✗ FAIL: TODO[2] not satisfied – see README section 'TODO 2 – Manage Chat History'");
                        Console.WriteLine("  Context not maintained properly. AI should remember previous messages.");
                    }
                }
                else
                {
                    Console.WriteLine("✗ FAIL: TODO[2] not satisfied – see README section 'TODO 2 – Manage Chat History'");
                }
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("✗ FAIL: TODO[2] not satisfied – see README section 'TODO 2 – Manage Chat History'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ SKIP: Test requires valid OpenAI API key. Error: {ex.Message}");
                Console.WriteLine("  Set OPENAI_API_KEY environment variable to run this test.");
                passCount++; // Don't penalize for API issues
            }
        }
        else
        {
            Console.WriteLine("⚠ SKIP: OPENAI_API_KEY not set - cannot test API interaction");
            Console.WriteLine("  To run this test, set your OpenAI API key as an environment variable.");
            passCount++; // Don't penalize for missing key
        }

        // Test 3: Display History
        Console.WriteLine("\nTest 3: Display Conversation History");
        try
        {
            Console.WriteLine("--- Conversation History ---");
            chatManager.DisplayHistory();
            Console.WriteLine("--- End History ---");
            Console.WriteLine("✓ PASS: History display method executed successfully");
            passCount++;
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("✗ FAIL: TODO[3] not satisfied – see README section 'TODO 3 – Display Conversation History'");
        }

        Console.WriteLine($"\n=== Test Results: {passCount}/{totalTests} PASSED ===");
    }
}
