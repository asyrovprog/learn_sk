# Quiz: Prompt Engineering with Semantic Functions

**Date:** December 2, 2025  
**Topic:** Prompt Engineering with Semantic Functions  
**Total Questions:** 10

---

**Question 1:** What is a Semantic Function in Semantic Kernel?
- A) A C# method decorated with [KernelFunction]
- B) A prompt template that can be invoked like a function
- C) A database query function
- D) A mathematical calculation function

**Question 2:** Which temperature setting should you use for factual, deterministic tasks like code generation?
- A) 0.0-0.3
- B) 0.4-0.7
- C) 0.8-1.0
- D) Always use 1.0

**Question 3:** What is the correct syntax for a template variable in Semantic Kernel prompts?
- A) ${variableName}
- B) {{variableName}}
- C) {{$variableName}}
- D) {variableName}

**Question 4:** (Multi-select) Which of the following are benefits of registering semantic functions as plugins? (Select all that apply)
- A) Automatic function calling by AI
- B) Better organization of related functions
- C) Faster execution speed
- D) Discoverability for planners

**Question 5:** What does the `ToolCallBehavior.AutoInvokeKernelFunctions` setting do?
- A) It prevents the AI from calling any functions
- B) It allows AI to automatically select and execute registered functions
- C) It only shows which functions the AI wants to call without executing them
- D) It requires manual approval for each function call

**Question 6:** How do you call a plugin function from within a prompt template?
- A) {{$pluginName.functionName}}
- B) {{pluginName.functionName}}
- C) [pluginName.functionName]
- D) ${pluginName.functionName}

**Question 7:** (Y/N) Semantic functions can be mixed with native C# functions in the same plugin.

**Question 8:** What is the purpose of the `MaxTokens` setting in OpenAIPromptExecutionSettings?
- A) To control how creative the response is
- B) To limit the maximum length of the response and control costs
- C) To set the number of retries
- D) To define the number of examples in few-shot learning

**Question 9:** (Multi-select) Which are valid prompt engineering best practices? (Select all that apply)
- A) Be specific and clear in your instructions
- B) Use high temperature for factual tasks
- C) Include examples (few-shot learning)
- D) Specify the output format explicitly

**Question 10:** What is the benefit of streaming responses with `InvokeStreamingAsync`?
- A) It reduces token costs
- B) It shows progress for long-running prompts
- C) It improves accuracy
- D) It allows parallel execution
