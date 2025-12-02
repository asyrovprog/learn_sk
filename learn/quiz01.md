# Quiz 01: Microsoft Semantic Kernel - .NET C# with OpenAI APIs

## Question 1
What is the primary purpose of the Kernel in Semantic Kernel?
- A) To manage API keys and authentication
- B) To serve as the central orchestrator for AI services, plugins, and prompts
- C) To store chat history
- D) To compile C# code

## Question 2
Which method is used to build a Kernel instance with OpenAI services?
- A) `new Kernel()`
- B) `Kernel.CreateBuilder().AddOpenAIChatCompletion().Build()`
- C) `OpenAI.CreateKernel()`
- D) `SemanticKernel.Initialize()`

## Question 3
A Plugin in Semantic Kernel is:
- A) Only a C# class with special attributes
- B) A collection of reusable functions that extend the kernel's capabilities
- C) A type of neural network
- D) A database connector

## Question 4 (Multi-select: M:)
Which of the following are valid ways to create plugins? (Select all that apply)
- A) Using [KernelFunction] attribute on methods
- B) Using prompt templates (semantic functions)
- C) Importing from OpenAPI specifications
- D) Writing raw SQL queries

## Question 5
What does the InvokePromptAsync method do?
- A) Compiles the kernel
- B) Sends a prompt to the AI model and returns the response
- C) Creates a new plugin
- D) Deletes chat history

## Question 6
ChatHistory in Semantic Kernel is used to:
- A) Debug application errors
- B) Maintain conversation context across multiple AI interactions
- C) Store user credentials
- D) Cache compiled code

## Question 7
Which attribute is used to mark a method as a kernel function?
- A) `[Function]`
- B) `[SemanticFunction]`
- C) `[KernelFunction]`
- D) `[AIMethod]`

## Question 8
What is the Planner component used for?
- A) Scheduling tasks in the operating system
- B) Automatically creating and executing multi-step plans to achieve goals
- C) Planning database migrations
- D) Organizing file structures

## Question 9
When using OpenAI with Semantic Kernel, the API key should be:
- Y) Stored securely (e.g., environment variables, Azure Key Vault)
- N) Hardcoded in the source code

## Question 10
Can Semantic Kernel integrate with multiple AI services (OpenAI, Azure OpenAI, Hugging Face)?
- Y) Yes, it supports multiple AI service providers
- N) No, it only works with OpenAI

---

**Submit your answers in this format**: `1:B,2:A,3:Y,4:ABC,5:N` (use M: prefix for multi-select, e.g., `4:M:AC`)
