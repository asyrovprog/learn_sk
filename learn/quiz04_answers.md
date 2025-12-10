# Quiz Answers: Planners & AI Orchestration

**Date:** December 8, 2025  
**Topic:** Planners & AI Orchestration

---

## Answer Key

**Question 1:** **B** - To automatically break down complex goals into executable steps
- **Reasoning:** Planners are the orchestration component in Semantic Kernel that take a user's goal and automatically decompose it into a series of function calls to accomplish the task.

**Question 2:** **C** - FunctionCallingStepwisePlanner
- **Reasoning:** FunctionCallingStepwisePlanner is the recommended planner for modern Semantic Kernel implementations. SequentialPlanner is being deprecated in favor of function calling approaches.

**Question 3:** **B** - Automatic function discovery and execution by the AI
- **Reasoning:** This setting enables the AI model to automatically discover available functions in the kernel and invoke them based on the user's request without explicit manual invocation.

**Question 4:** **Y** - Yes
- **Reasoning:** HandlebarsPlanner generates reusable Handlebars templates that can be saved, versioned, and executed multiple times with different inputs.

**Question 5:** **A, C, D** - Well-described functions, Kernel with registered plugins, OpenAI API credentials
- **Reasoning:** Planners need: (A) clear function descriptions so the AI knows when to use them, (C) a kernel with plugins registered, and (D) API credentials to communicate with the LLM. A database connection (B) is not required for planners to work.

**Question 6:** **B** - It can adapt and make decisions at each step based on previous results
- **Reasoning:** FunctionCallingStepwisePlanner works iteratively, allowing it to observe the result of each step and adjust its strategy, making it more flexible than SequentialPlanner which creates the entire plan upfront.

**Question 7:** **B** - The AI's ability to select and invoke registered functions based on user queries
- **Reasoning:** Function calling is a capability where the LLM can decide which functions to call from the available toolkit based on understanding the user's intent and the function descriptions.

**Question 8:** **A, B, D** - Token limits, Ambiguous function descriptions, Hallucinated function calls
- **Reasoning:** Common challenges include: (A) hitting token limits with complex multi-step plans, (B) poorly described functions leading to wrong selections, and (D) the AI hallucinating functions that don't exist. Internet connection (C) is needed for API calls but isn't specifically a planner challenge.

**Question 9:** **B** - To control cost and prevent excessively long responses
- **Reasoning:** MaxTokens limits the maximum length of the response, which helps control API costs (since pricing is per token) and prevents runaway token usage.

**Question 10:** **Y** - Yes
- **Reasoning:** This is the core capability of planners - they can orchestrate multiple function calls in sequence or based on conditional logic to accomplish complex tasks that require multiple steps.

---

## Scoring Guide
- Questions 1-3, 6-7, 9: 1 point each (6 points total)
- Questions 4, 10 (Y/N): 1 point each (2 points total)  
- Question 5 (Multi-select): 1 point (all correct required)
- Question 8 (Multi-select): 1 point (all correct required)

**Total Possible: 10 points**
**Passing Score: 7+ points (70%)**
