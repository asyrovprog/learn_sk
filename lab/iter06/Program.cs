using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ImageClassifierQA;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Smart Image Classifier & Q&A Assistant - Lab 06 ===\n");

        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("❌ ERROR: OPENAI_API_KEY environment variable not set");
            Console.WriteLine("Set it with: export OPENAI_API_KEY=your-key-here");
            return;
        }

        // Setup Kernel
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-5.2", apiKey);

        // Add text embedding for semantic memory
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        _ = builder.AddOpenAITextEmbeddingGeneration("text-embedding-3-small", apiKey);
        var kernel = builder.Build();

        // Setup semantic memory
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var memoryBuilder = new MemoryBuilder();
        memoryBuilder.WithOpenAITextEmbeddingGeneration("text-embedding-3-small", apiKey);
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        _ = memoryBuilder.WithMemoryStore(new VolatileMemoryStore());
        var memory = memoryBuilder.Build();

        // Create instances
        var classifier = new ImageClassifier(kernel);
        var assistant = new ImageQAAssistant(kernel, memory);

        int passedTests = 0;
        int totalTests = 3;

        // Test 1: Image Classification with Logprobs
        Console.WriteLine("Test 1: Image Classification with Logprobs");
        try
        {
            // Using a publicly accessible cat image
            var catImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/3a/Cat03.jpg/481px-Cat03.jpg";
            
            Console.WriteLine("  Classifying test image of a cat.");
            var result = await classifier.ClassifyImageAsync(catImageUrl);

            Console.WriteLine("  Classification Results:");
            foreach (var kvp in result.CategoryProbabilities.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value:F4} ({kvp.Value * 100:F2}%)");
            }

            var dogImageUrl = "https://upload.wikimedia.org/wikipedia/commons/c/c5/Basenji_Profile_%28loosercrop%29.jpg";
            Console.WriteLine("  Classifying test image of a dog.");
            result = await classifier.ClassifyImageAsync(dogImageUrl);

            Console.WriteLine("  Classification Results:");
            foreach (var kvp in result.CategoryProbabilities.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value:F4} ({kvp.Value * 100:F2}%)");
            }

            var fishImageUrl = "https://upload.wikimedia.org/wikipedia/commons/a/a5/Pomacentridae_-_Amphiprion_nigripes.jpg";
            Console.WriteLine("  Classifying test image of a fish.");
            result = await classifier.ClassifyImageAsync(fishImageUrl);

            Console.WriteLine("  Classification Results:");
            foreach (var kvp in result.CategoryProbabilities.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value:F4} ({kvp.Value * 100:F2}%)");
            }

            var carImageUrl = "https://en.wikipedia.org/wiki/Category:Images_of_cars#/media/File:Cheever_GP_Masters.JPG";
            Console.WriteLine("  Classifying test image of a car.");
            result = await classifier.ClassifyImageAsync(carImageUrl);

            Console.WriteLine("  Classification Results:");
            foreach (var kvp in result.CategoryProbabilities.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value:F4} ({kvp.Value * 100:F2}%)");
            }


            var hardImageUrl = "https://i.insider.com/5e600587a9f40c6f39701245?width=700&format=jpeg&auto=webp";
            Console.WriteLine("  Classifying test image of a dog.");
            result = await classifier.ClassifyImageAsync(hardImageUrl);

            Console.WriteLine("  Classification Results:");
            foreach (var kvp in result.CategoryProbabilities.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value:F4} ({kvp.Value * 100:F2}%)");
            }

            var robotImageUrl = "https://techcrunch.com/wp-content/uploads/2020/01/MarsCat-6.jpg?resize=2048,1365";
            Console.WriteLine("  Classifying test image of a robot cat.");
            result = await classifier.ClassifyImageAsync(robotImageUrl);

            Console.WriteLine("  Classification Results:");
            foreach (var kvp in result.CategoryProbabilities.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value:F4} ({kvp.Value * 100:F2}%)");
            }

            // Validate
            var totalProb = result.CategoryProbabilities.Values.Sum();
            if (Math.Abs(totalProb - 1.0) > 0.01)
            {
                throw new Exception($"Probabilities don't sum to 1.0 (got {totalProb:F4})");
            }

            if (result.CategoryProbabilities.Count != 4)
            {
                throw new Exception($"Expected 4 categories, got {result.CategoryProbabilities.Count}");
            }

            if (!result.CategoryProbabilities.ContainsKey("CAT") ||
                !result.CategoryProbabilities.ContainsKey("DOG") ||
                !result.CategoryProbabilities.ContainsKey("FISH") ||
                !result.CategoryProbabilities.ContainsKey("OTHER"))
            {
                throw new Exception("Missing required categories (CAT, DOG, FISH, OTHER)");
            }

            Console.WriteLine("  ✅ PASS\n");
            passedTests++;
        }
        catch (NotImplementedException ex)
        {
            Console.WriteLine($"  ❌ FAIL: {ex.Message}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ FAIL: {ex.Message}\n");
        }

        // Test 2: Semantic Memory Storage
        Console.WriteLine("Test 2: Semantic Memory Storage");
        try
        {
            Console.WriteLine("  Storing test image analysis in memory...");
            
            var testResult = new ClassificationResult
            {
                CategoryProbabilities = new()
                {
                    ["CAT"] = 0.85,
                    ["DOG"] = 0.10,
                    ["FISH"] = 0.03,
                    ["OTHER"] = 0.02
                }
            };

            await assistant.StoreImageAnalysisAsync(
                "test_cat.jpg",
                "A fluffy orange tabby cat sitting on a windowsill",
                testResult
            );

            Console.WriteLine("  Searching memory for 'cat'...");

            var memories = new System.Collections.Generic.List<MemoryQueryResult>();
            await foreach (var mem in memory.SearchAsync("image_analyses", "orange cat", limit: 3))
            {
                memories.Add(mem);
            }

            if (memories.Count == 0)
            {
                throw new Exception("No memories found - storage might have failed");
            }

            Console.WriteLine($"  Found {memories.Count} relevant memory/memories");
            foreach (var mem in memories)
            {
                Console.WriteLine($"    - Relevance: {mem.Relevance:F2} | {mem.Metadata.Text.Substring(0, Math.Min(60, mem.Metadata.Text.Length))}...");
            }

            Console.WriteLine("  ✅ PASS\n");
            passedTests++;
        }
        catch (NotImplementedException ex)
        {
            Console.WriteLine($"  ❌ FAIL: {ex.Message}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ FAIL: {ex.Message}\n");
        }

        // Test 3: Q&A Conversation
        Console.WriteLine("Test 3: Q&A Conversation");
        try
        {
            var dogImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/2/2d/Golde33443.jpg/440px-Golde33443.jpg";
            
            Console.WriteLine("  Question 1: What animal is in this image?");
            var answer1 = await assistant.AskQuestionAsync(
                "What animal is in this image?",
                dogImageUrl
            );
            
            if (string.IsNullOrEmpty(answer1))
            {
                throw new Exception("Q&A returned empty response");
            }
            
            Console.WriteLine($"  Answer: {answer1.Substring(0, Math.Min(100, answer1.Length))}...");

            Console.WriteLine("\n  Question 2: Have we seen any cats before? (should find memory)");
            var answer2 = await assistant.AskQuestionAsync(
                "Have we analyzed any cats before? What do you remember?"
            );
            
            if (string.IsNullOrEmpty(answer2))
            {
                throw new Exception("Q&A returned empty response for memory question");
            }
            
            Console.WriteLine($"  Answer: {answer2.Substring(0, Math.Min(100, answer2.Length))}...");

            Console.WriteLine("  ✅ PASS\n");
            passedTests++;
        }
        catch (NotImplementedException ex)
        {
            Console.WriteLine($"  ❌ FAIL: {ex.Message}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ FAIL: {ex.Message}\n");
        }

        // Summary
        Console.WriteLine("=".PadRight(60, '='));
        Console.WriteLine($"Test Results: {passedTests}/{totalTests} passed");
        Console.WriteLine("=".PadRight(60, '='));

        if (passedTests == totalTests)
        {
            Console.WriteLine("🎉 All tests passed! Lab complete!");
        }
        else
        {
            Console.WriteLine($"⚠️  {totalTests - passedTests} test(s) failed. Review the TODOs above.");
        }
    }
}
