# Quiz Answers: Prompt Engineering with Semantic Functions

**Date:** December 2, 2025  
**Student Score:** 90% (9/10 correct)

---

## Answer Key

**Question 1:** **B** - A prompt template that can be invoked like a function
- **Reasoning:** Semantic functions are prompts that Semantic Kernel treats as callable functions. They're created from natural language templates and can be invoked just like regular C# methods.

**Question 2:** **A** - 0.0-0.3
- **Reasoning:** Low temperature values (0.0-0.3) produce more deterministic, focused outputs, which is ideal for factual tasks like code generation, data extraction, and Q&A where accuracy is critical.

**Question 3:** **C** - {{$variableName}}
- **Reasoning:** In Semantic Kernel prompt templates, variables use the `{{$variableName}}` syntax. The `$` prefix indicates it's a variable.

**Question 4:** **A, B, D** - Automatic function calling by AI, Better organization of related functions, Discoverability for planners
- **Reasoning:** Registering as plugins enables AI planners to discover and automatically call functions, helps organize related functionality, and improves metadata/descriptions. It doesn't inherently make execution faster (C is incorrect).

**Question 5:** **B** - It allows AI to automatically select and execute registered functions
- **Reasoning:** `ToolCallBehavior.AutoInvokeKernelFunctions` enables the AI to automatically determine which functions to call based on the user's query and execute them without manual intervention.

**Question 6:** **B** - {{pluginName.functionName}}
- **Reasoning:** To call a plugin function from within a template, use `{{pluginName.functionName}}` syntax (no `$` prefix since it's a function call, not a variable).

**Question 7:** **Y** (Yes)
- **Reasoning:** Semantic Kernel allows you to mix native C# functions (decorated with [KernelFunction]) and semantic functions (created from prompts) in the same plugin, providing maximum flexibility.

**Question 8:** **B** - To limit the maximum length of the response and control costs
- **Reasoning:** `MaxTokens` sets the upper limit on how many tokens the AI can generate, which both controls response length and helps manage API costs (since you pay per token).

**Question 9:** **A, C, D** - Be specific and clear, Include examples (few-shot learning), Specify output format
- **Reasoning:** These are all best practices. Option B is incorrect - you should use LOW temperature for factual tasks, not high.

**Question 10:** **B** - It shows progress for long-running prompts
- **Reasoning:** Streaming allows you to display output incrementally as it's generated, providing real-time feedback for long-running prompts. It doesn't reduce costs (A), improve accuracy (C), or enable parallel execution (D - this was the incorrect answer selected).

---

## Student Answers

1: B ✅  
2: A ✅  
3: C ✅  
4: ABD ✅  
5: B ✅  
6: B ✅  
7: Y ✅  
8: B ✅  
9: ACD ✅  
10: BD ❌ (Correct: B)

---

## Feedback

Excellent performance! You demonstrated strong understanding of:
- Semantic function concepts and syntax
- Temperature settings for different use cases
- Plugin registration benefits
- Template variable and function call syntax
- Prompt engineering best practices

**Area for improvement:**
- Streaming is about progressive output display, not parallelization. It shows results as they're generated, giving users real-time feedback during long-running operations.
