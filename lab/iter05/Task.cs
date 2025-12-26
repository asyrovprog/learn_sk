using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.RegularExpressions;

namespace CoTMathReasoner;

/// <summary>
/// Chain-of-Thought Math Reasoner using Self-Consistency pattern
/// </summary>
public class CoTReasoner
{
    private readonly Kernel _kernel;

    public CoTReasoner(Kernel kernel)
    {
        _kernel = kernel;
    }

    /// <summary>
    /// TODO 1: Create a semantic function that uses Chain-of-Thought prompting
    /// </summary>
    public KernelFunction CreateCoTPrompt()
    {
        // TODO[1]: Create a CoT prompt function
        // [YOUR CODE GOES HERE]
        //
        // Instructions from README:
        // 1. Create a semantic function using `kernel.CreateFunctionFromPrompt()`
        // 2. Design a prompt template that:
        //    - Takes a math word problem as `{{$problem}}`
        //    - Explicitly instructs the AI to think step by step
        //    - Requests the final answer in a specific format: `ANSWER: [number]`
        // 3. Set execution settings:
        //    - Temperature: 0.7 (allows some variation for self-consistency)
        //    - MaxTokens: 500 (enough for reasoning steps)
        // 4. Return the created function
        //
        // Requirements:
        // - Use the phrase "Let's think step by step" or similar to trigger CoT
        // - Ensure the prompt asks for reasoning before the answer
        // - Final answer must be clearly marked with "ANSWER:" prefix
        // - Function name: "SolveWithCoT"

        string promptTemplate = @"
        Let's think step by step to solve the following problem:
        {{$problem}}
        Your final response should be in the following format:
        ANSWER: [number]";
        
        var prompt = _kernel.CreateFunctionFromPrompt(
            promptTemplate: promptTemplate,
            executionSettings: new OpenAIPromptExecutionSettings()
            {
                Temperature = 0.7,
                MaxTokens = 500,
            },
            functionName: "SolveWithCoT",
            description: "Solve math problem"
        );

        return prompt;
    }

    /// <summary>
    /// TODO 2: Generate multiple independent reasoning paths
    /// </summary>
    public async Task<List<ReasoningResult>> GenerateMultiplePaths(
        KernelFunction cotFunction, 
        string problem, 
        int sampleCount)
    {
        // TODO[2]: Implement Self-Consistency Sampling
        // [YOUR CODE GOES HERE]
        //
        // Instructions from README:
        // 1. Take the CoT function from TODO 1
        // 2. Invoke it multiple times (parameter: `sampleCount`) with the same problem
        // 3. Each invocation should be independent (don't reuse results)
        // 4. Collect all reasoning paths and answers in a list
        // 5. Return the list of reasoning results
        //
        // Requirements:
        // - Parameter `sampleCount` should control how many samples to generate
        // - Each sample should be a complete independent reasoning attempt
        // - Store both the full reasoning text and extracted answer for each sample
        // - Use proper async/await patterns
        //
        // Hint: You'll need to parse each response to extract the answer marked with "ANSWER:"

        var list = new List<Task<FunctionResult>>();
        var arguments = new KernelArguments();
        arguments.Add("problem", problem);

        for (int i = 0; i < sampleCount; i++)
        {
            list.Add(cotFunction.InvokeAsync(_kernel, arguments));
        }
        await Task.WhenAll(list).ConfigureAwait(false);
        var results = new List<ReasoningResult>();
        foreach (var task in list)
        {
            var functionResult = await task.ConfigureAwait(false);
            var fullReasoning = functionResult.GetValue<string>() ?? "";
            var reasoningResult = new ReasoningResult()
            {
                ExtractedAnswer = ExtractAnswer(fullReasoning),
                FullReasoning = fullReasoning
            };
            results.Add(reasoningResult);
        }

        return results;
    }

    /// <summary>
    /// TODO 3: Implement majority voting to select most consistent answer
    /// </summary>
    public VotingResult SelectByMajorityVote(List<ReasoningResult> results)
    {
        // TODO[3]: Implement Majority Voting
        // [YOUR CODE GOES HERE]
        //
        // Instructions from README:
        // 1. Take the list of reasoning results from TODO 2
        // 2. Extract the final answer from each result (look for "ANSWER: [number]")
        // 3. Count how many times each unique answer appears
        // 4. Return the answer that appears most frequently (the "majority vote")
        // 5. If there's a tie, return the first occurrence
        //
        // Requirements:
        // - Handle variations in number format (e.g., "42", "42.0", "42.00" should be treated as same)
        // - Return both the winning answer and its vote count
        // - Handle edge cases (empty list, no valid answers found)
        //
        // Hint: Use a dictionary/map to count occurrences of each answer

        var dict = new Dictionary<string, int>();
        foreach (var val in results)
        {
            int.TryParse(val.ExtractedAnswer, out var value);
            dict.TryGetValue(value.ToString(), out var count);
            dict[value.ToString()] = count + 1;
        }
        
        var winner = "";
        var winnerCount = 0;
        foreach (var kv in dict)
        {
            if (kv.Value > winnerCount)
            {
                winner = kv.Key;
                winnerCount = kv.Value;
            }
        }

        return new VotingResult()
        {
            TotalVotes = results.Count,
            VoteCount = winnerCount,
            WinningAnswer = winner.ToString(),
            VoteDistribution = dict
        };

    }

    /// <summary>
    /// Helper: Extract answer from CoT response
    /// </summary>
    private string ExtractAnswer(string text)
    {
        var match = Regex.Match(text, @"ANSWER:\s*([0-9]+\.?[0-9]*)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : "";
    }

    /// <summary>
    /// Helper: Normalize numbers to handle format variations
    /// </summary>
    private string NormalizeNumber(string number)
    {
        if (double.TryParse(number, out double value))
        {
            // Remove trailing zeros and decimal point if integer
            return value.ToString("0.####");
        }
        return number;
    }
}

/// <summary>
/// Represents a single reasoning attempt
/// </summary>
public class ReasoningResult
{
    public string FullReasoning { get; set; } = "";
    public string ExtractedAnswer { get; set; } = "";
}

/// <summary>
/// Represents the result of majority voting
/// </summary>
public class VotingResult
{
    public string WinningAnswer { get; set; } = "";
    public int VoteCount { get; set; }
    public int TotalVotes { get; set; }
    public Dictionary<string, int> VoteDistribution { get; set; } = new();
    
    public double ConsensusPercentage => TotalVotes > 0 ? (VoteCount * 100.0 / TotalVotes) : 0;
}
