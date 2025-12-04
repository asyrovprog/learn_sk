using TranslationPluginLab;
using Microsoft.SemanticKernel;

class Program
{
    static async Task Main(string[] args)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("ERROR: OPENAI_API_KEY environment variable not set");
            return;
        }

        var manager = new TranslationManager(apiKey);
        int passed = 0;
        int total = 3;

        // Test 1: Translation function creation with proper template variables and settings
        Console.WriteLine("Test 1: Translation Function Creation");
        try
        {
            var translateFunc = manager.CreateTranslationFunction();
            
            if (translateFunc == null)
            {
                Console.WriteLine("FAIL - TODO[1] not satisfied – see README section 'TODO 1 – Create Translation Semantic Function'");
            }
            else if (translateFunc.Name != "Translate")
            {
                Console.WriteLine("FAIL - Function name should be 'Translate'");
            }
            else if (string.IsNullOrEmpty(translateFunc.Description))
            {
                Console.WriteLine("FAIL - Function must have a description");
            }
            else
            {
                // Test basic translation
                var result = await translateFunc.InvokeAsync(manager.GetKernel(), new KernelArguments
                {
                    ["text"] = "Hello, how are you?",
                    ["targetLanguage"] = "Spanish"
                });

                var translation = result.ToString();
                if (string.IsNullOrWhiteSpace(translation))
                {
                    Console.WriteLine("FAIL - Translation returned empty result");
                }
                else if (translation.Contains("Hello") && !translation.Contains("Hola"))
                {
                    Console.WriteLine("FAIL - Translation doesn't appear to be working (still contains English)");
                }
                else
                {
                    Console.WriteLine($"PASS - Translation: {translation}");
                    passed++;
                }
            }
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("FAIL - TODO[1] not satisfied – see README section 'TODO 1 – Create Translation Semantic Function'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL - {ex.Message}");
        }

        Console.WriteLine();

        // Test 2: Plugin registration and auto function calling
        Console.WriteLine("Test 2: Plugin Registration and Auto Function Calling");
        try
        {
            var translateFunc = manager.CreateTranslationFunction();
            var response = await manager.TranslateWithAutoFunctionAsync(
                translateFunc, 
                "Translate 'Good morning' to French"
            );

            if (string.IsNullOrWhiteSpace(response))
            {
                Console.WriteLine("FAIL - TODO[2] not satisfied – see README section 'TODO 2 – Register as Plugin and Enable Auto Function Calling'");
            }
            else if (!response.Contains("Bonjour") && !response.Contains("bonjour"))
            {
                Console.WriteLine("FAIL - Auto function calling doesn't appear to be working correctly");
            }
            else
            {
                Console.WriteLine($"PASS - Auto translation response: {response}");
                passed++;
            }
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("FAIL - TODO[2] not satisfied – see README section 'TODO 2 – Register as Plugin and Enable Auto Function Calling'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL - {ex.Message}");
        }

        Console.WriteLine();

        // Test 3: Multi-language function with function chaining
        Console.WriteLine("Test 3: Multi-Language Output with Function Chaining");
        try
        {
            // Create a fresh kernel for this test to avoid plugin conflicts
            var testManager = new TranslationManager(apiKey);
            
            // First create and register the translation function
            var translateFunc = testManager.CreateTranslationFunction();
            testManager.GetKernel().Plugins.AddFromFunctions("TranslationPlugin", [translateFunc]);

            // Now create the multi-language function
            var multiLangFunc = testManager.CreateMultiLanguageFunction();

            if (multiLangFunc == null)
            {
                Console.WriteLine("FAIL - TODO[3] not satisfied – see README section 'TODO 3 – Multi-Language Output with Function Chaining'");
            }
            else if (multiLangFunc.Name != "TranslateToMultiple")
            {
                Console.WriteLine("FAIL - Function name should be 'TranslateToMultiple'");
            }
            else
            {
                var result = await multiLangFunc.InvokeAsync(testManager.GetKernel(), new KernelArguments
                {
                    ["text"] = "Thank you very much"
                });

                var output = result.ToString();
                if (string.IsNullOrWhiteSpace(output))
                {
                    Console.WriteLine("FAIL - Multi-language translation returned empty result");
                }
                else if (!output.ToLower().Contains("spanish") || 
                         !output.ToLower().Contains("french") || 
                         !output.ToLower().Contains("german"))
                {
                    Console.WriteLine("FAIL - Output should contain all three languages: Spanish, French, and German");
                }
                else if (output.Contains("Thank you") && 
                        (!output.Contains("Gracias") && !output.Contains("gracias")) &&
                        (!output.Contains("Merci") && !output.Contains("merci")))
                {
                    Console.WriteLine("FAIL - Translations don't appear to be working (still shows English)");
                }
                else
                {
                    Console.WriteLine($"PASS - Multi-language output:\n{output}");
                    passed++;
                }
            }
        }
        catch (NotImplementedException)
        {
            Console.WriteLine("FAIL - TODO[3] not satisfied – see README section 'TODO 3 – Multi-Language Output with Function Chaining'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL - {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine($"Results: {passed}/{total} PASSED");
    }
}
