# Reference Guide - Lab 05: Chain-of-Thought Math Reasoner

This guide provides hints and reference solutions for each TODO in the lab.

---

## TODO 1 Hints – Create CoT Prompt Function

**Key Concepts:**
- Chain-of-Thought prompting requires explicit instruction to think step-by-step
- The prompt should guide the AI through a structured reasoning process
- Output format matters - use a clear marker like "ANSWER:" for parsing

**Hints:**
1. Use `kernel.CreateFunctionFromPrompt()` to create the function
2. Your prompt template should:
   - Include the phrase "Let's think step by step" or similar
   - Ask the AI to break down the problem into steps
   - Request the final answer in format "ANSWER: [number]"
3. Set Temperature to 0.7 (allows variation for self-consistency)
4. Set MaxTokens to 500 (enough for reasoning + answer)

**Example Prompt Structure:**
```
You are a helpful math tutor. Solve this problem step by step.

Problem: {{$problem}}

Let's think step by step:
1. [instruction for first step]
2. [instruction for second step]
...

Provide final answer as: ANSWER: [number]
```

---

## TODO 2 Hints – Implement Self-Consistency Sampling

**Key Concepts:**
- Self-Consistency generates multiple independent reasoning paths
- Each invocation should be completely independent
- Store both the full reasoning text and extracted answer

**Hints:**
1. Use a loop to invoke the CoT function `sampleCount` times
2. Each invocation should use `await kernel.InvokeAsync(cotFunction, arguments)`
3. Extract the answer from each response using regex or string parsing
4. Store results in a `List<ReasoningResult>`
5. You'll need a helper method to extract "ANSWER: X" from the text

**Pseudocode:**
```
results = []
for i in range(sampleCount):
    response = await invoke_function(problem)
    answer = extract_answer(response)
    results.append(ReasoningResult(response, answer))
return results
```

---

## TODO 3 Hints – Implement Majority Voting

**Key Concepts:**
- Count occurrences of each unique answer
- Select the most frequent answer as the winner
- Handle number format variations (42, 42.0, 42.00 are same)

**Hints:**
1. Use a `Dictionary<string, int>` to count votes
2. Normalize numbers before counting (handle decimal variations)
3. Find the answer with maximum vote count
4. Return both the winning answer and vote statistics
5. Handle edge cases: empty list, no valid answers

**Pseudocode:**
```
vote_counts = {}
for result in results:
    answer = normalize(result.answer)
    vote_counts[answer] = vote_counts.get(answer, 0) + 1

winner = max(vote_counts, key=vote_counts.get)
return VotingResult(winner, vote_counts[winner], total_votes)
```

---

## Common Issues & Solutions

### Issue: Tests fail with NotImplementedException
**Solution:** Make sure to replace the stub code with your implementation and remove the `throw new NotImplementedException()` line.

### Issue: No answer extracted from CoT response
**Solution:** Check your prompt format. The AI response must contain "ANSWER: [number]" exactly. Use regex: `@"ANSWER:\s*([0-9]+\.?[0-9]*)"`

### Issue: Majority voting doesn't handle ties
**Solution:** If there's a tie, return the first answer that achieved the maximum vote count.

### Issue: Different number formats cause vote splitting
**Solution:** Normalize numbers before counting. Convert "42", "42.0", "42.00" to same format (e.g., "42").

---

<details>
<summary>Reference Solution (open after attempting all TODOs)</summary>

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.RegularExpressions;

namespace CoTMathReasoner;

public class CoTReasoner
{
    private readonly Kernel _kernel;

    public CoTReasoner(Kernel kernel)
    {
        _kernel = kernel;
    }

    public KernelFunction CreateCoTPrompt()
    {
        var cotPrompt = @"
You are a helpful math tutor. Solve the following math word problem step by step.

Problem: {{$problem}}

Let's think step by step to solve this problem:
1. First, identify what we know
2. Then, determine what we need to find
3. Break down the problem into smaller steps
4. Solve each step
5. Provide the final answer

After your reasoning, provide the final numerical answer in this exact format:
ANSWER: [your numerical answer]
";

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0.7,
            MaxTokens = 500
        };

        var function = _kernel.CreateFunctionFromPrompt(
            cotPrompt,
            executionSettings,
            functionName: "SolveWithCoT",
            description: "Solves math word problems using Chain-of-Thought reasoning"
        );

        return function;
    }

    public async Task<List<ReasoningResult>> GenerateMultiplePaths(
        KernelFunction cotFunction, 
        string problem, 
        int sampleCount)
    {
        var results = new List<ReasoningResult>();

        for (int i = 0; i < sampleCount; i++)
        {
            var response = await _kernel.InvokeAsync(
                cotFunction,
                new KernelArguments { ["problem"] = problem }
            );

            var fullText = response.ToString();
            var answer = ExtractAnswer(fullText);

            results.Add(new ReasoningResult
            {
                FullReasoning = fullText,
                ExtractedAnswer = answer
            });
        }

        return results;
    }

    public VotingResult SelectByMajorityVote(List<ReasoningResult> results)
    {
        if (results == null || results.Count == 0)
        {
            return new VotingResult { WinningAnswer = "No results", VoteCount = 0, TotalVotes = 0 };
        }

        var voteCounts = new Dictionary<string, int>();

        foreach (var result in results)
        {
            if (!string.IsNullOrEmpty(result.ExtractedAnswer))
            {
                var normalized = NormalizeNumber(result.ExtractedAnswer);
                
                if (voteCounts.ContainsKey(normalized))
                {
                    voteCounts[normalized]++;
                }
                else
                {
                    voteCounts[normalized] = 1;
                }
            }
        }

        if (voteCounts.Count == 0)
        {
            return new VotingResult { WinningAnswer = "No valid answers", VoteCount = 0, TotalVotes = results.Count };
        }

        var winner = voteCounts.OrderByDescending(kvp => kvp.Value).First();

        return new VotingResult
        {
            WinningAnswer = winner.Key,
            VoteCount = winner.Value,
            TotalVotes = results.Count,
            VoteDistribution = voteCounts
        };
    }

    private string ExtractAnswer(string text)
    {
        var match = Regex.Match(text, @"ANSWER:\s*([0-9]+\.?[0-9]*)", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : "";
    }

    private string NormalizeNumber(string number)
    {
        if (double.TryParse(number, out double value))
        {
            return value.ToString("0.####");
        }
        return number;
    }
}

public class ReasoningResult
{
    public string FullReasoning { get; set; } = "";
    public string ExtractedAnswer { get; set; } = "";
}

public class VotingResult
{
    public string WinningAnswer { get; set; } = "";
    public int VoteCount { get; set; }
    public int TotalVotes { get; set; }
    public Dictionary<string, int> VoteDistribution { get; set; } = new();
    
    public double ConsensusPercentage => TotalVotes > 0 ? (VoteCount * 100.0 / TotalVotes) : 0;
}
```

</details>

---

## Additional Resources

- [Chain-of-Thought Prompting Paper](https://arxiv.org/abs/2201.11903)
- [Self-Consistency Paper](https://arxiv.org/abs/2203.11171)
- [Semantic Kernel Documentation](https://learn.microsoft.com/en-us/semantic-kernel/)
