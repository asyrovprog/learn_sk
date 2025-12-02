# Quiz 01 Answers: Microsoft Semantic Kernel

## Correct Answers

1. **B** - To serve as the central orchestrator for AI services, plugins, and prompts
   - *Reasoning*: The Kernel is the core component that coordinates all interactions between AI services, plugins, and prompts.

2. **B** - `Kernel.CreateBuilder().AddOpenAIChatCompletion().Build()`
   - *Reasoning*: This is the builder pattern used in Semantic Kernel to configure and create a Kernel instance.

3. **B** - A collection of reusable functions that extend the kernel's capabilities
   - *Reasoning*: Plugins encapsulate functionality that can be reused across different contexts within the kernel.

4. **M:ABC** - Using [KernelFunction] attribute on methods, Using prompt templates, Importing from OpenAPI specifications
   - *Reasoning*: All three are valid ways to create plugins. SQL queries are not a plugin creation method.

5. **B** - Sends a prompt to the AI model and returns the response
   - *Reasoning*: InvokePromptAsync is the method to execute a prompt and get AI-generated responses.

6. **B** - Maintain conversation context across multiple AI interactions
   - *Reasoning*: ChatHistory stores the conversation flow to maintain context in multi-turn conversations.

7. **C** - `[KernelFunction]`
   - *Reasoning*: This is the correct attribute to mark methods as kernel functions in Semantic Kernel.

8. **B** - Automatically creating and executing multi-step plans to achieve goals
   - *Reasoning*: The Planner creates execution plans by chaining functions to accomplish complex objectives.

9. **Y** - Yes, store securely
   - *Reasoning*: API keys should never be hardcoded; use secure storage mechanisms.

10. **Y** - Yes, it supports multiple AI service providers
    - *Reasoning*: Semantic Kernel is designed to work with various AI backends.

## Scoring
- Total Questions: 10
- Each Question: 10 points
- Passing Score: 70%
