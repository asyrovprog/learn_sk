# Lab 03 Setup Summary

## Lab Details
- **Topic:** Memory & Context Management
- **Lab Name:** Multi-Turn Conversational Assistant with Semantic Memory
- **Directory:** `lab/iter03`
- **Status:** Ready for students (stubbed and tested)

## What Was Implemented

### 1. Real Semantic Memory with VolatileMemoryStore
- Uses **actual OpenAI embeddings** (text-embedding-ada-002)
- Implements **real vector search** with cosine similarity
- Students experience authentic RAG (Retrieval Augmented Generation) pattern
- Demonstrates semantic understanding (meaning-based search, not keywords)

### 2. Two TODO Challenges

#### TODO 1: Initialize Semantic Memory Store
- Create VolatileMemoryStore instance
- Build semantic memory with MemoryBuilder pattern
- Configure OpenAI text embedding generation
- Save user preferences as vector embeddings
- Return ISemanticTextMemory instance

#### TODO 2: Implement Conversation Loop with Semantic Search
- Create ChatHistory for multi-turn conversation
- Use `memory.SearchAsync()` for real vector similarity search
- Build context from search results with relevance scores
- Integrate retrieved context into conversational prompts
- Maintain conversation state across multiple turns

### 3. Package Configuration
```xml
<PackageReference Include="Microsoft.SemanticKernel" Version="1.28.0" />
<PackageReference Include="Microsoft.SemanticKernel.Plugins.Memory" Version="1.68.0-alpha" />
```

**Note:** The Memory plugin is in prerelease (alpha) but stable for educational use. It contains the "old" Memory API (VolatileMemoryStore) which aligns with learn03.md content.

## Files Created

### Core Files
- ✅ **Task.cs** - Stubbed with NotImplementedException for both TODOs
- ✅ **Program.cs** - Test harness that validates both TODO implementations
- ✅ **README.md** - Comprehensive instructions with detailed steps for each TODO
- ✅ **REF.md** - Hints, code snippets, and collapsed reference solution
- ✅ **ConversationalMemoryAssistant.csproj** - Project file with SK packages

### Integration
- ✅ Added to **learn_sk.sln**
- ✅ Added debug configuration to **.vscode/launch.json**
- ✅ Updated **learnlog.md** with lab 03 entry (status: started)

## Testing Verification

### Stub Behavior (Current State)
```bash
cd lab/iter03
OPENAI_API_KEY=dummy dotnet run
```

**Output:**
```
=== Conversational Memory Assistant Lab ===

Test 1: Initializing semantic memory...

❌ FAIL: TODO 1: Initialize Semantic Memory Store – see README.md section 'TODO 1 – Initialize Semantic Memory Store'
See README section 'TODO 1 – Initialize Semantic Memory Store'
```

✅ Properly throws NotImplementedException  
✅ References correct README section  
✅ Provides clear guidance to students

### Expected Behavior (After Completion)
1. Semantic memory initializes with real embeddings
2. At least 2 user preferences stored as vectors
3. Interactive conversation loop starts
4. Each user query triggers vector similarity search
5. Relevant context retrieved with relevance scores (0.0-1.0)
6. AI provides personalized responses based on semantic search results
7. Conversation maintains context across multiple turns
8. Exit with "exit" or "quit"

## Key Learning Objectives

### Students Will Learn:
1. **Vector Embeddings** - How text is converted to high-dimensional vectors
2. **Semantic Search** - Finding information by meaning, not keywords
3. **RAG Pattern** - Retrieval Augmented Generation in practice
4. **VolatileMemoryStore** - In-memory vector database for development
5. **MemoryBuilder Pattern** - Configuring semantic memory services
6. **ISemanticTextMemory** - Interface for memory operations
7. **Relevance Scoring** - Understanding cosine similarity scores
8. **Context Integration** - Combining chat history with semantic search results

### Practical Experience:
- Real API calls to OpenAI for embeddings
- Actual vector similarity computations
- Production-like RAG architecture (scaled down for learning)
- Multi-turn conversational AI with long-term memory

## Technical Notes

### Why VolatileMemoryStore?
- **Simple:** No external dependencies (Redis, Qdrant, etc.)
- **Authentic:** Real vector operations, not simulation
- **Educational:** Demonstrates core concepts without infrastructure complexity
- **Aligned:** Matches learn03.md which teaches the old Memory API

### Algorithm Complexity
- **Search:** O(N) brute force comparison
- **Limitation:** Suitable for < 10k vectors
- **Production:** Would use HNSW-based stores (Qdrant, Azure AI Search) for O(log N)

### .NET Framework
- **Target:** net10.0 (system has .NET 10 available)
- **Previous:** Changed from net8.0 to match environment

## Next Steps for Students

1. **Set OPENAI_API_KEY environment variable**
   ```bash
   export OPENAI_API_KEY=sk-...
   ```

2. **Open lab directory**
   ```bash
   cd lab/iter03
   ```

3. **Read README.md thoroughly**
   - Understand both TODO objectives
   - Review requirements and key concepts

4. **Implement TODO 1**
   - Edit Task.cs
   - Create VolatileMemoryStore
   - Configure MemoryBuilder
   - Save user preferences

5. **Test TODO 1**
   ```bash
   dotnet run
   ```

6. **Implement TODO 2**
   - Create ChatHistory
   - Implement conversation loop
   - Integrate semantic search
   - Display results with scores

7. **Test TODO 2 interactively**
   - Ask questions about learning preferences
   - Observe semantic matching (not keyword matching)
   - Verify relevance scores are displayed

8. **Experiment**
   - Try synonyms and related terms
   - See how semantic search finds relevant info
   - Test multi-turn conversation flow

## Reference Solution

The complete working solution is available in **REF.md** inside a collapsed `<details>` section. Students should attempt implementation before viewing.

## Success Criteria

### Lab Passes When:
- ✅ No compilation errors
- ✅ Semantic memory stores at least 2 preferences with embeddings
- ✅ Memory search returns results with relevance scores
- ✅ Conversation loop accepts user input
- ✅ Each turn performs semantic search
- ✅ Context is retrieved and displayed
- ✅ AI responses reference stored preferences
- ✅ ChatHistory maintains context across turns
- ✅ Can exit gracefully with "exit" or "quit"

### Additional Validation:
Students can verify semantic search works by querying with synonyms:
- Query: "What do I like?" → Should find "loves learning about AI"
- Query: "My work" → Should find "project involving semantic search"
- Query: "artificial intelligence" → Should match "AI and machine learning"

This proves semantic understanding, not just keyword matching!

---

**Lab Setup Completed:** December 3, 2025  
**Status:** Ready for students  
**Estimated Completion Time:** 45-60 minutes
