using Microsoft.SemanticKernel;
using CoTMathReasoner;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Chain-of-Thought Math Reasoner Lab ===\n");

        // Setup Kernel
        var builder = Kernel.CreateBuilder();
        
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("ERROR: OPENAI_API_KEY environment variable not set");
            return;
        }

        builder.AddOpenAIChatCompletion("gpt-4o-mini", apiKey);
        var kernel = builder.Build();

        var reasoner = new CoTReasoner(kernel);

        // Run tests
        await RunTests(reasoner);
    }

    static async Task RunTests(CoTReasoner reasoner)
    {
        var testCases = new Dictionary<string, string>
        {
            ["Simple Addition"] = "Emma has 15 apples. Her friend gives her 12 more apples. How many apples does Emma have now?",
            ["Multi-step Problem"] = "A store has 8 boxes of pencils with 12 pencils in each box. If they sell 25 pencils, how many pencils are left?",
            ["Division Problem"] = "John has 48 cookies and wants to share them equally among 6 friends. How many cookies does each friend get?"
        };

        int passedTests = 0;
        int totalTests = testCases.Count;

        foreach (var test in testCases)
        {
            Console.WriteLine($"\n{'=', 60}");
            Console.WriteLine($"Test: {test.Key}");
            Console.WriteLine($"{'=', 60}");
            Console.WriteLine($"Problem: {test.Value}\n");

            try
            {
                // Test TODO 1: Create CoT function
                var cotFunction = reasoner.CreateCoTPrompt();
                if (cotFunction == null)
                {
                    Console.WriteLine("❌ FAIL: TODO 1 not implemented - CreateCoTPrompt() returned null");
                    Console.WriteLine("   See README section 'TODO 1 – Create CoT Prompt Function'\n");
                    continue;
                }

                // Test TODO 2: Generate multiple paths
                Console.WriteLine("Generating 5 reasoning paths...\n");
                var samples = await reasoner.GenerateMultiplePaths(cotFunction, test.Value, 5);
                
                if (samples == null || samples.Count == 0)
                {
                    Console.WriteLine("❌ FAIL: TODO 2 not implemented - GenerateMultiplePaths() returned no results");
                    Console.WriteLine("   See README section 'TODO 2 – Implement Self-Consistency Sampling'\n");
                    continue;
                }

                // Display sample reasoning paths (first 2)
                for (int i = 0; i < Math.Min(2, samples.Count); i++)
                {
                    Console.WriteLine($"--- Path {i + 1} ---");
                    Console.WriteLine(samples[i].FullReasoning.Substring(0, Math.Min(200, samples[i].FullReasoning.Length)) + "...");
                    Console.WriteLine($"Extracted Answer: {samples[i].ExtractedAnswer}\n");
                }

                // Test TODO 3: Majority voting
                var result = reasoner.SelectByMajorityVote(samples);
                
                if (string.IsNullOrEmpty(result.WinningAnswer) || result.WinningAnswer == "No results" || result.WinningAnswer == "No valid answers")
                {
                    Console.WriteLine("❌ FAIL: TODO 3 not implemented - SelectByMajorityVote() returned invalid result");
                    Console.WriteLine("   See README section 'TODO 3 – Implement Majority Voting'\n");
                    continue;
                }

                // Display voting results
                Console.WriteLine("Vote Distribution:");
                foreach (var vote in result.VoteDistribution.OrderByDescending(kvp => kvp.Value))
                {
                    var marker = vote.Key == result.WinningAnswer ? "✓" : "";
                    Console.WriteLine($"  - {vote.Key}: {vote.Value} vote(s) {marker}");
                }

                Console.WriteLine($"\n✅ Final Answer: {result.WinningAnswer} (with {result.ConsensusPercentage:F0}% consensus)");
                
                passedTests++;
            }
            catch (NotImplementedException ex)
            {
                Console.WriteLine($"❌ FAIL: {ex.Message}");
                var todoMatch = System.Text.RegularExpressions.Regex.Match(ex.Message, @"TODO\[(\d+)\]");
                if (todoMatch.Success)
                {
                    var todoNum = todoMatch.Groups[1].Value;
                    Console.WriteLine($"   See README section 'TODO {todoNum}'\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAIL: Unexpected error - {ex.Message}\n");
            }
        }

        // Summary
        Console.WriteLine($"\n{'=', 60}");
        Console.WriteLine($"Test Results: {passedTests}/{totalTests} passed");
        Console.WriteLine($"{'=', 60}");

        if (passedTests == totalTests)
        {
            Console.WriteLine("🎉 All tests passed! Great work!");
        }
        else
        {
            Console.WriteLine($"⚠️  {totalTests - passedTests} test(s) failed. Review the TODOs above.");
        }
    }
}
