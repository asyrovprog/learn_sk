using Microsoft.SemanticKernel;
using ContentModerationAgent;

class Program
{
    static async System.Threading.Tasks.Task Main(string[] args)
    {
        Console.WriteLine("=== Content Moderation Agent Lab ===\n");

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("⚠️  Warning: OPENAI_API_KEY not set. Some tests will be skipped.");
        }

        int passed = 0;
        int total = 5;

        // Test 1: Create SentimentAnalyzer
        Console.WriteLine("Test 1: Create SentimentAnalyzer function");
        try
        {
            var kernel1 = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("gpt-4o-mini", apiKey ?? "test")
                .Build();
            var func = ContentModerationAgent.Task.CreateSentimentAnalyzer(kernel1);
            if (func != null && func.Name == "AnalyzeSentiment")
            {
                Console.WriteLine("✓ PASS: SentimentAnalyzer created successfully");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ FAIL: TODO[1] not satisfied – see README section 'TODO 1 – Create SentimentAnalyzer'");
            }
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("✗ FAIL: TODO[1] not satisfied – see README section 'TODO 1 – Create SentimentAnalyzer'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: TODO[1] error – {ex.Message}");
        }

        // Test 2: Create ToxicityDetector
        Console.WriteLine("\nTest 2: Create ToxicityDetector function");
        try
        {
            var kernel2 = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("gpt-4o-mini", apiKey ?? "test")
                .Build();
            var func = ContentModerationAgent.Task.CreateToxicityDetector(kernel2);
            if (func != null && func.Name == "DetectToxicity")
            {
                Console.WriteLine("✓ PASS: ToxicityDetector created successfully");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ FAIL: TODO[2] not satisfied – see README section 'TODO 2 – Create ToxicityDetector'");
            }
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("✗ FAIL: TODO[2] not satisfied – see README section 'TODO 2 – Create ToxicityDetector'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: TODO[2] error – {ex.Message}");
        }

        // Test 3: Create LanguageDetector
        Console.WriteLine("\nTest 3: Create LanguageDetector function");
        try
        {
            var kernel3 = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion("gpt-4o-mini", apiKey ?? "test")
                .Build();
            var func = ContentModerationAgent.Task.CreateLanguageDetector(kernel3);
            if (func != null && func.Name == "DetectLanguage")
            {
                Console.WriteLine("✓ PASS: LanguageDetector created successfully");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ FAIL: TODO[3] not satisfied – see README section 'TODO 3 – Create LanguageDetector'");
            }
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("✗ FAIL: TODO[3] not satisfied – see README section 'TODO 3 – Create LanguageDetector'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: TODO[3] error – {ex.Message}");
        }

        // Test 4: Create ModerationPlugin
        Console.WriteLine("\nTest 4: Create ModerationPlugin with static function");
        try
        {
            var plugin = new ContentModerationAgent.Task.ModerationPlugin();
            var guidelines = plugin.GetModerationGuidelines();
            if (!string.IsNullOrEmpty(guidelines) && guidelines.Contains("BLOCK") && guidelines.Contains("ALLOW"))
            {
                Console.WriteLine("✓ PASS: ModerationPlugin created with guidelines");
                passed++;
            }
            else
            {
                Console.WriteLine("✗ FAIL: TODO[4] not satisfied – see README section 'TODO 4 – Create ModerationPlugin'");
            }
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("✗ FAIL: TODO[4] not satisfied – see README section 'TODO 4 – Create ModerationPlugin'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ FAIL: TODO[4] error – {ex.Message}");
        }

        // Test 5: RunModerationAgent (requires API key)
        Console.WriteLine("\nTest 5: RunModerationAgent with function calling");
        if (!string.IsNullOrEmpty(apiKey))
        {
            try
            {
                var kernel5 = Kernel.CreateBuilder()
                    .AddOpenAIChatCompletion("gpt-4o-mini", apiKey)
                    .Build();

                var result = await ContentModerationAgent.Task.RunModerationAgent(
                    kernel5,
                    "Analyze the sentiment of this review",
                    "I absolutely love this product! Best purchase ever!"
                );

                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine($"✓ PASS: Agent responded: {result.Substring(0, Math.Min(80, result.Length))}...");
                    passed++;
                }
                else
                {
                    Console.WriteLine("✗ FAIL: TODO[5] not satisfied – see README section 'TODO 5 – RunModerationAgent'");
                }
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("✗ FAIL: TODO[5] not satisfied – see README section 'TODO 5 – RunModerationAgent'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ FAIL: TODO[5] error – {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("⊘ SKIP: Test requires OPENAI_API_KEY");
        }

        Console.WriteLine($"\n=== Test Results: {passed}/{total} PASSED ===");

        if (passed == total)
        {
            Console.WriteLine("🎉 All tests passed! Lab complete!");
        }
        else
        {
            Console.WriteLine($"❌ {total - passed} test(s) failed. Review the TODOs above.");
        }

        // Additional tests with toxic content and function call tracking
        if (!string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("\n\n=== Additional Tests: Toxic Content Detection & Function Call Tracking ===\n");

            var testCases = new[]
            {
                new { Name = "Guidelines Query", Content = "What are your moderation guidelines? What should be blocked?" },
                new { Name = "Hate Speech", Content = "I hate those people, they should all be banned from here!" },
                new { Name = "Harassment", Content = "You're so stupid, nobody likes you. Just quit already." },
                new { Name = "Violence", Content = "I'm going to find you and make you pay for this." },
                new { Name = "Spam", Content = "CLICK HERE NOW!!! FREE MONEY!!! BUY CHEAP PILLS!!!" },
                new { Name = "Clean Content", Content = "This is a great tutorial, thank you for sharing!" },
                new { Name = "Borderline", Content = "Your opinion is completely wrong and shows ignorance." },
                new { Name = "Foreign Language", Content = "Ce produit est vraiment mauvais, je ne le recommande pas." }
            };

            foreach (var testCase in testCases)
            {
                Console.WriteLine($"\n--- Test: {testCase.Name} ---");
                Console.WriteLine($"Content: {testCase.Content}");
                
                // Create a fresh kernel for each test to avoid plugin conflicts
                var kernel = Kernel.CreateBuilder()
                    .AddOpenAIChatCompletion("gpt-4o-mini", apiKey)
                    .Build();

                var functionCallLog = new List<string>();
                kernel.FunctionInvocationFilters.Add(new FunctionCallTracker(functionCallLog));

                try
                {
                    // For guidelines query, don't pass content separately
                    var query = testCase.Name == "Guidelines Query" 
                        ? testCase.Content 
                        : $"Moderate this content: {testCase.Content}";
                    var content = testCase.Name == "Guidelines Query" 
                        ? null 
                        : testCase.Content;
                    
                    var result = await ContentModerationAgent.Task.RunModerationAgent(
                        kernel,
                        query,
                        content
                    );

                    Console.WriteLine($"Result: {result}");
                    
                    if (functionCallLog.Count > 0)
                    {
                        Console.WriteLine($"\n✓ Functions called ({functionCallLog.Count}):");
                        foreach (var call in functionCallLog)
                        {
                            Console.WriteLine($"  - {call}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠ No functions were called");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"✗ Error: {ex.Message}");
                }
            }
        }
    }
}

// Function call tracking filter
public class FunctionCallTracker : IFunctionInvocationFilter
{
    private readonly List<string> _log;

    public FunctionCallTracker(List<string> log)
    {
        _log = log;
    }

    public async System.Threading.Tasks.Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, System.Threading.Tasks.Task> next)
    {
        var functionName = $"{context.Function.PluginName}.{context.Function.Name}";
        var argStr = context.Arguments.FirstOrDefault().Value?.ToString();
        var truncated = argStr != null && argStr.Length > 30 ? argStr.Substring(0, 30) + "..." : argStr ?? "";
        
        _log.Add($"{functionName}({truncated})");
        Console.WriteLine($"[FUNCTION CALL] {functionName}");
        
        await next(context);
    }
}
