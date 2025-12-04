# Quiz Answers: Memory & Context Management in Semantic Kernel

**Date:** December 3, 2025  
**Topic:** Memory & Context Management  
**User Score:** 90% (9/10 correct)

---

## Answer Key

**Question 1:** What is the primary purpose of ChatHistory in Semantic Kernel?  
**Correct Answer:** **B** - To maintain conversational context across multiple AI interactions  
**User Answer:** B ✅

**Explanation:** ChatHistory is designed to track and maintain the flow of conversation between users and AI. It preserves context across multiple turns, allowing the AI to reference earlier parts of the conversation. It stores messages (user/assistant/system) as a structured list, which gets included in subsequent prompts to maintain coherence.

---

**Question 2:** Which algorithm does HNSW (Hierarchical Navigable Small World) use for vector search?  
**Correct Answer:** **B** - O(log N) graph-based approximate search  
**User Answer:** B ✅

**Explanation:** HNSW builds a multi-layer graph structure where nodes represent vectors. It uses proximity graphs to navigate from a starting point to the nearest neighbors through graph traversals. This achieves O(log N) complexity for approximate nearest neighbor search, making it much faster than brute force O(N) while maintaining high accuracy (typically >95%).

---

**Question 3:** What is the main advantage of the RAG (Retrieval Augmented Generation) pattern?  
**Correct Answer:** **B** - It enables AI to generate responses based on retrieved contextual information from a knowledge base  
**User Answer:** B ✅

**Explanation:** RAG addresses the limitation of LLMs being frozen at their training cutoff date. By retrieving relevant documents from a vector store and injecting them into the prompt, RAG allows the AI to access up-to-date, domain-specific, or proprietary information. This grounds responses in retrieved facts rather than relying solely on parametric knowledge.

---

**Question 4:** (Multi-select) Which vector stores support native TTL (Time-To-Live) for automatic expiration?  
**Correct Answer:** **M:AB** - Redis, MongoDB Atlas  
**User Answer:** M:AQ ❌

**Explanation:** 
- **Redis (A):** Supports TTL via the `EXPIRE` command. Keys automatically expire after the specified duration.
- **MongoDB Atlas (B):** Supports TTL indexes. Documents with a date field can be automatically deleted after a specified time.
- **Qdrant (Q/C):** Does NOT have native TTL. You must manually delete records by filtering on timestamp.
- **Azure AI Search (D):** Does NOT support TTL. Requires manual cleanup jobs or indexer schedules.

TTL is critical for session data, temporary caches, and compliance with data retention policies.

---

**Question 5:** What is the correct syntax for context variables in Semantic Kernel prompt templates?  
**Correct Answer:** **C** - {{$variableName}}  
**User Answer:** C ✅

**Explanation:** Semantic Kernel uses the `{{$variableName}}` syntax to reference context variables in prompt templates. The `$` prefix distinguishes variables from function calls (which use `{{PluginName.FunctionName}}`). For example: `{{$userInput}}`, `{{$history}}`, `{{$topic}}`.

---

**Question 6:** Which vector store is best suited for development and testing with small datasets (< 10k vectors)?  
**Correct Answer:** **C** - VolatileMemoryStore  
**User Answer:** C ✅

**Explanation:** VolatileMemoryStore is an in-memory vector store that requires no external dependencies (databases, Docker, cloud services). It's perfect for:
- Local development and testing
- Proof-of-concept implementations
- Small datasets that fit in RAM
- Situations where persistence isn't required

It uses O(N) brute force search, which is acceptable for small datasets. For production or large-scale deployments, use Qdrant, Pinecone, or Azure AI Search.

---

**Question 7:** (Y/N) Can you mix orchestrator-driven RAG (pre-fetching context) with LLM-driven tool calls (letting the model call a SearchKnowledge function)?  
**Correct Answer:** **Y** (Yes)  
**User Answer:** Y ✅

**Explanation:** Hybrid RAG combines multiple retrieval strategies:
1. **Orchestrator-driven:** Your code pre-fetches context based on deterministic logic (e.g., always include company policy docs).
2. **LLM-driven:** The model decides when to invoke a `SearchKnowledge` function as a tool call during inference.

This provides flexibility: guaranteed context (orchestrator) + dynamic retrieval (LLM). It's especially useful when you want baseline context but also allow the model to explore additional sources as needed.

---

**Question 8:** What is the primary difference between keyword search (BM25) and vector search?  
**Correct Answer:** **B** - Keyword search matches exact terms, while vector search captures semantic meaning  
**User Answer:** B ✅

**Explanation:**
- **Keyword search (BM25/TF-IDF):** Matches documents containing specific words or phrases. Fast and precise for exact matches but misses synonyms, paraphrases, or conceptually related content.
- **Vector search:** Converts text into embeddings (dense vectors) that capture semantic meaning. Finds conceptually similar content even if wording differs. For example, "automobile" and "car" have high vector similarity despite different spellings.

Many systems use **hybrid search** (combining both) for optimal results.

---

**Question 9:** (Multi-select) Which are recommended best practices for managing chat history?  
**Correct Answer:** **M:ACD** - Token counting, sliding window, summarization  
**User Answer:** M:ACD ✅

**Explanation:**
- **A (Token counting):** Essential to avoid exceeding model context limits (e.g., GPT-4 has 8k/32k/128k token limits).
- **C (Sliding window):** Keep only the most recent N messages, discarding older ones. Simple and effective.
- **D (Summarization):** Use the LLM to summarize older messages into a compact form, preserving key information while reducing tokens.
- **B (Store indefinitely):** Incorrect. This leads to context overflow and degraded performance. Always implement a strategy to manage growing history.

Additional techniques: hierarchical summarization, importance filtering, external memory stores for long-term context.

---

**Question 10:** What is the time complexity of VolatileMemoryStore's vector search?  
**Correct Answer:** **C** - O(N) linear brute force  
**User Answer:** C ✅

**Explanation:** VolatileMemoryStore compares the query vector against every stored vector to compute similarity (cosine distance). This is O(N) time complexity. While inefficient for large datasets, it's:
- Simple to implement
- Acceptable for small datasets (< 10k vectors)
- Accurate (no approximation)

Production vector stores (Qdrant, Pinecone) use HNSW or other indexing structures to achieve O(log N) approximate search.

---

## Summary

**Excellent work!** You achieved **90%**, demonstrating strong mastery of:
- Conversational context management (ChatHistory)
- Vector search algorithms and complexity analysis
- RAG pattern principles and hybrid approaches
- Semantic Kernel syntax conventions
- Memory store selection criteria
- Best practices for production systems

**Area for review:**
- **TTL support across vector stores:** Focus on which databases have native time-based expiration (Redis, MongoDB) vs. those requiring manual cleanup (Qdrant, Azure AI Search). This is critical for data lifecycle management and compliance.

**Status:** **PASSED** - You're ready to proceed to the lab iteration!

