# Quiz: Memory & Context Management in Semantic Kernel

**Date:** December 3, 2025  
**Topic:** Memory & Context Management  
**Total Questions:** 10

---

**Question 1:** What is the primary purpose of ChatHistory in Semantic Kernel?
- A) To store semantic memory embeddings
- B) To maintain conversational context across multiple AI interactions
- C) To manage vector database connections
- D) To cache compiled kernel functions

**Question 2:** Which algorithm does HNSW (Hierarchical Navigable Small World) use for vector search?
- A) O(N) brute force linear search
- B) O(log N) graph-based approximate search
- C) O(1) hash-based exact search
- D) O(N²) exhaustive comparison

**Question 3:** What is the main advantage of the RAG (Retrieval Augmented Generation) pattern?
- A) It reduces API costs by caching responses
- B) It enables AI to generate responses based on retrieved contextual information from a knowledge base
- C) It speeds up model training
- D) It eliminates the need for embeddings

**Question 4:** (Multi-select) Which vector stores support native TTL (Time-To-Live) for automatic expiration? (Select all that apply)
- A) Redis
- B) MongoDB Atlas
- C) Qdrant
- D) Azure AI Search

**Question 5:** What is the correct syntax for context variables in Semantic Kernel prompt templates?
- A) ${variableName}
- B) {{variableName}}
- C) {{$variableName}}
- D) {$variableName}

**Question 6:** Which vector store is best suited for development and testing with small datasets (< 10k vectors)?
- A) Azure AI Search
- B) Pinecone
- C) VolatileMemoryStore
- D) Qdrant

**Question 7:** (Y/N) Can you mix orchestrator-driven RAG (pre-fetching context) with LLM-driven tool calls (letting the model call a SearchKnowledge function)?

**Question 8:** What is the primary difference between keyword search (BM25) and vector search?
- A) Keyword search is slower but more accurate
- B) Keyword search matches exact terms, while vector search captures semantic meaning
- C) Vector search only works with English text
- D) Keyword search requires embeddings

**Question 9:** (Multi-select) Which are recommended best practices for managing chat history? (Select all that apply)
- A) Implement token counting to stay within context limits
- B) Store all messages indefinitely for perfect memory
- C) Use sliding window or truncation for long conversations
- D) Summarize older messages to compress context

**Question 10:** What is the time complexity of VolatileMemoryStore's vector search?
- A) O(1) - constant time
- B) O(log N) - logarithmic using HNSW
- C) O(N) - linear brute force
- D) O(N log N) - sorted search

---

**Submit your answers in this format:** `1:B,2:A,3:Y,4:ABC,5:N` (use M: prefix for multi-select, e.g., `4:M:AC`)
