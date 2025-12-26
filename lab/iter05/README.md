# Chain-of-Thought Math Reasoner Lab

**Topic:** Advanced Prompt Engineering Patterns  
**Estimated Time:** ~25 minutes  
**Difficulty:** Intermediate

## Overview

In this lab, you'll build a math word problem solver using advanced prompting techniques:
- **Chain-of-Thought (CoT)** prompting for step-by-step reasoning
- **Self-Consistency** with multiple reasoning paths
- **Majority voting** to select the most reliable answer

You'll learn how to make AI reasoning more reliable by generating multiple independent solutions and selecting the most consistent result.

## Prerequisites

- Semantic Kernel basics
- Understanding of CoT prompting
- Knowledge of Self-Consistency pattern

## Lab Structure

This lab has 3 TODOs that build progressively:

---

## TODO 1 – Create CoT Prompt Function

**Objective:** Create a semantic function that uses Chain-of-Thought prompting to solve math word problems with explicit step-by-step reasoning.

**Instructions:**
1. Create a semantic function using `kernel.CreateFunctionFromPrompt()`
2. Design a prompt template that:
   - Takes a math word problem as `{{$problem}}`
   - Explicitly instructs the AI to think step by step
   - Requests the final answer in a specific format: `ANSWER: [number]`
3. Set execution settings:
   - Temperature: 0.7 (allows some variation for self-consistency)
   - MaxTokens: 500 (enough for reasoning steps)
4. Return the created function

**Requirements:**
- Use the phrase "Let's think step by step" or similar to trigger CoT
- Ensure the prompt asks for reasoning before the answer
- Final answer must be clearly marked with "ANSWER:" prefix
- Function name: "SolveWithCoT"

---

## TODO 2 – Implement Self-Consistency Sampling

**Objective:** Generate multiple independent reasoning paths for the same problem to improve reliability through diversity.

**Instructions:**
1. Take the CoT function from TODO 1
2. Invoke it multiple times (parameter: `sampleCount`) with the same problem
3. Each invocation should be independent (don't reuse results)
4. Collect all reasoning paths and answers in a list
5. Return the list of reasoning results

**Requirements:**
- Parameter `sampleCount` should control how many samples to generate
- Each sample should be a complete independent reasoning attempt
- Store both the full reasoning text and extracted answer for each sample
- Use proper async/await patterns

**Hint:** You'll need to parse each response to extract the answer marked with "ANSWER:"

---

## TODO 3 – Implement Majority Voting

**Objective:** Select the most consistent answer from multiple reasoning paths using majority voting.

**Instructions:**
1. Take the list of reasoning results from TODO 2
2. Extract the final answer from each result (look for "ANSWER: [number]")
3. Count how many times each unique answer appears
4. Return the answer that appears most frequently (the "majority vote")
5. If there's a tie, return the first occurrence

**Requirements:**
- Handle variations in number format (e.g., "42", "42.0", "42.00" should be treated as same)
- Return both the winning answer and its vote count
- Handle edge cases (empty list, no valid answers found)

**Hint:** Use a dictionary/map to count occurrences of each answer

---

## Expected Behavior

When complete, your program should:

1. Accept a math word problem
2. Generate multiple reasoning paths using CoT
3. Vote on the most consistent answer
4. Display:
   - Each reasoning path (optional, for debugging)
   - Vote distribution
   - Final selected answer

**Example Output:**
```
Problem: Sarah has 3 boxes with 12 apples each. She gives away 8 apples. How many apples does she have left?

Generating 5 reasoning paths...

Vote Distribution:
- 28: 4 votes ✓
- 27: 1 vote

Final Answer: 28 (with 80% consensus)
```

## Testing

Run the program with `dotnet run` to test various math problems. All test cases should pass.

## Success Criteria

- ✓ CoT prompt generates step-by-step reasoning
- ✓ Self-consistency generates multiple independent samples
- ✓ Majority voting correctly identifies most common answer
- ✓ All test cases pass
- ✓ Code is clean and well-commented

Good luck! 🚀
