# Multi-Turn Conversational Assistant with Semantic Memory Lab

## Overview
Build a conversational AI assistant that uses **real semantic memory** with embeddings and vector search. This lab demonstrates practical implementation of ChatHistory management and semantic memory integration using VolatileMemoryStore - the same pattern used in production RAG applications.

## Learning Objectives
- Implement semantic memory with VolatileMemoryStore and embeddings
- Store and retrieve user preferences using vector similarity search
- Experience how semantic search finds relevant information by meaning, not keywords
- Combine conversational context (ChatHistory) with long-term knowledge (Semantic Memory)
- Handle personalized responses based on remembered information

## TODO 1 – Initialize Semantic Memory Store

**Objective:** Set up VolatileMemoryStore with real OpenAI embeddings and save user preferences.

**Instructions:**
1. Create an instance of `VolatileMemoryStore` (in-memory vector database)
2. Create a `MemoryBuilder` and configure it with:
   - The volatile memory store using `.WithMemoryStore(memoryStore)`
   - OpenAI text embedding generation using `.WithOpenAITextEmbeddingGeneration("text-embedding-ada-002", apiKey)`
3. Build the semantic memory instance using `.Build()`
4. Define a collection name: `"UserPreferences"`
5. Save a user preference to semantic memory with:
   - Collection: `"UserPreferences"`
   - Text: `"User loves learning about AI and machine learning. Prefers detailed technical explanations."`
   - Id: `"pref_learning_style"`
   - Description: `"User's learning preferences"`
6. Save another user preference:
   - Collection: `"UserPreferences"`
   - Text: `"User is working on a project involving semantic search and vector databases."`
   - Id: `"pref_current_project"`
   - Description: `"User's current project context"`
7. Return the configured semantic memory instance

**Requirements:**
- Use OpenAI embedding model "text-embedding-ada-002"
- Use VolatileMemoryStore (real in-memory vector database)
- Save at least 2 user preferences that will be converted to embeddings
- Each memory entry must have a unique ID
- Return type: `ISemanticTextMemory`

**Key Concepts:**
- When you call `SaveInformationAsync()`, the text is automatically converted to a vector embedding
- These embeddings capture the **semantic meaning** of the text
- Later searches use cosine similarity to find relevant information

---

## TODO 2 – Implement Conversation Loop with Semantic Search

**Objective:** Create a conversation loop that uses real vector search to find relevant context.

**Instructions:**
1. Create a new `ChatHistory` instance
2. Add a system message that instructs the AI to:
   - Act as a helpful learning assistant
   - Use provided context about user preferences
   - Provide personalized responses based on user's background
3. Start an infinite conversation loop that:
   - Prompts user for input (use `Console.ReadLine()`)
   - Exits if user types "exit" or "quit" (case-insensitive)
   - For each user message:
     a. Search semantic memory using `memory.SearchAsync()`:
        - Collection: `"UserPreferences"`
        - Query: the user's input (will be embedded automatically)
        - Limit: 2 results
        - Minimum relevance score: 0.7
     b. Build a context string from search results
        - Include the text and relevance score for each result
        - Format: `"- {text} (relevance: {score})"`
     c. Add user message to ChatHistory with format:
        ```
        User Query: {user input}
        
        Relevant User Context:
        {context from memory search with scores}
        ```
     d. Get AI response using chat completion service with:
        - The ChatHistory
        - Execution settings with `ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions`
     e. Add assistant's response to ChatHistory
     f. Display the assistant's response to console
4. Handle the loop until user exits

**Requirements:**
- Maintain ChatHistory across all conversation turns
- Use `memory.SearchAsync()` for real semantic search (not keyword matching)
- Include relevance scores in the context
- Use relevance threshold of 0.7 to filter results
- Continue conversation until user types "exit" or "quit"

**Key Concepts:**
- `SearchAsync()` converts your query to an embedding and finds similar vectors
- Relevance scores range from 0.0 to 1.0 (higher = more similar)
- This is **semantic search** - "ML" will match "machine learning" even without exact words

---

## Running the Lab

```bash
cd lab/iter03
dotnet run
```

## Expected Behavior

When you run the program successfully:
1. The assistant initializes semantic memory with embeddings
2. You can have a multi-turn conversation
3. The assistant performs **vector similarity search** for each query
4. Queries find relevant information based on **meaning**, not keywords
5. You'll see relevance scores showing how well each memory matches
6. ChatHistory maintains context across turns
7. Responses are personalized based on semantic search results

Example conversation:
```
You: What topics should I focus on?
[Memory search finds "AI and machine learning" preference with high relevance]
Assistant: Based on your interest in AI and machine learning, and that you prefer detailed technical explanations, I recommend focusing on...

You: Can you help with my current work?
[Memory search finds "semantic search and vector databases" preference]
Assistant: I see you're working on semantic search and vector databases. Let me provide detailed technical guidance on...

You: Tell me about neural networks
[Memory searches but finds relevant AI/ML preference]
Assistant: Given your background in AI and machine learning...
```

## Testing

Run the program and verify:
- ✅ No compilation errors
- ✅ Semantic memory stores preferences as embeddings
- ✅ Conversation maintains context across multiple turns
- ✅ Semantic search finds relevant preferences (with scores shown)
- ✅ Assistant references stored context in responses
- ✅ Search works semantically (finds "AI" when you ask about "machine learning")
- ✅ Can exit gracefully with "exit" or "quit"

## What Makes This "Semantic"?

Try asking questions with different words but similar meaning:
- "What am I studying?" → finds learning preferences
- "My project" → finds current work
- "artificial intelligence" → finds AI preference

The vector search understands **meaning**, not just matching words!
