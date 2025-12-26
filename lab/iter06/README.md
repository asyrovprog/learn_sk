# Lab 06: Smart Image Classifier & Q&A Assistant

**Topic:** Multi-Modal AI Applications  
**Estimated Time:** ~25 minutes  
**Difficulty:** Intermediate

## Overview

Build an intelligent image analysis system that combines **image classification with confidence scores** and **conversational Q&A**. You'll use GPT-4 Vision's **logprobs** (log probabilities) to get calibrated confidence scores for classification, and implement semantic memory to remember previous image analyses.

## Learning Objectives

1. Use **GPT-4 Vision** for image understanding
2. Extract and process **token logprobs** for confidence estimation
3. Convert log probabilities to normalized probabilities
4. Implement **semantic memory** for storing image analyses
5. Build multi-turn conversations about images

## Prerequisites

- Completed Lab 03 (Semantic Memory)
- Understanding of GPT-4 Vision from learn06.md
- Understanding of probability and softmax normalization
- OpenAI API key with GPT-4 Vision access

## Lab Structure

You'll implement three key components:

### TODO 1 – Image Classification with Logprobs

**Objective:** Classify images into CAT, DOG, FISH, or OTHER categories with confidence scores.

**What You'll Do:**
- Configure GPT-4 Vision to return logprobs for tokens
- Prompt the model to respond with ONLY one of the four category tokens
- Extract logprobs for CAT, DOG, FISH, and OTHER tokens
- Convert logprobs to probabilities using softmax normalization
- Return a `ClassificationResult` with probabilities for all 4 categories

**Key Concepts:**
- **Logprobs**: Natural logarithm of token probabilities returned by the model
- **Softmax**: Converts logprobs to normalized probabilities that sum to 1.0
- **Top Logprobs**: OpenAI returns top-k most likely tokens at each position

**Expected Behavior:**
```csharp
var result = await classifier.ClassifyImageAsync("path/to/cat.jpg");
// Example output:
// CAT: 0.91
// DOG: 0.05
// FISH: 0.02
// OTHER: 0.02
```

### TODO 2 – Semantic Memory Integration

**Objective:** Store image analyses in semantic memory for later retrieval.

**What You'll Do:**
- Initialize a `VolatileMemoryStore` for in-memory storage
- Create a semantic memory collection for image analyses
- Store classification results with image descriptions
- Enable semantic search to find similar previous analyses

**Key Concepts:**
- **Semantic Memory**: Vector-based storage for contextual information
- **Embeddings**: Vector representations of text for similarity search
- **Memory Collections**: Organized groups of related memories

**Expected Behavior:**
```csharp
await assistant.StoreImageAnalysisAsync("cat.jpg", "A fluffy orange cat", classificationResult);
// Later...
var memories = await assistant.SearchMemoryAsync("orange pets");
// Returns: stored cat analysis with relevance score
```

### TODO 3 – Conversational Q&A Loop

**Objective:** Answer questions about images using GPT-4 Vision and semantic memory.

**What You'll Do:**
- Create a conversation loop that accepts user questions
- Retrieve relevant past analyses from semantic memory
- Use GPT-4 Vision to answer questions about current or past images
- Maintain chat history for multi-turn conversations

**Key Concepts:**
- **Multi-Modal Chat**: Combining text and images in conversations
- **Context Retrieval**: Finding relevant information from memory
- **Chat History**: Maintaining conversational context

**Expected Behavior:**
```
> Ask: What animal is in the image?
AI: This image shows a cat. Based on the classification, I'm 91% confident it's a cat.

> Ask: What color is it?
AI: The cat in the image has orange/ginger colored fur.

> Ask: Have I shown you any other cats before?
AI: [Searches memory] Yes, you showed me a fluffy orange cat earlier.
```

## Implementation Guide

### Step 1: Understanding Logprobs

When you request logprobs from OpenAI, you get:

```json
{
  "content": "CAT",
  "logprobs": {
    "content": [
      {
        "token": "CAT",
        "logprob": -0.09431,  // ln(0.91) ≈ -0.09431
        "top_logprobs": [
          { "token": "CAT", "logprob": -0.09431 },
          { "token": "DOG", "logprob": -2.9957 },
          { "token": "FISH", "logprob": -3.9120 },
          { "token": "OTHER", "logprob": -3.9120 }
        ]
      }
    ]
  }
}
```

### Step 2: Converting Logprobs to Probabilities

Use **softmax** normalization:

```csharp
// Logprobs to probabilities
double[] logprobs = { -0.09431, -2.9957, -3.9120, -3.9120 };

// Apply softmax
double[] probs = Softmax(logprobs);
// Result: [0.91, 0.05, 0.02, 0.02]

double[] Softmax(double[] logprobs)
{
    // exp(logprob) converts back to raw probabilities
    var exps = logprobs.Select(x => Math.Exp(x)).ToArray();
    var sum = exps.Sum();
    return exps.Select(x => x / sum).ToArray();
}
```

### Step 3: Prompt Engineering for Classification

To get reliable logprobs, your prompt must:
1. Be extremely specific about the four valid responses
2. Instruct the model to output ONLY the category token
3. Use system message to enforce the constraint

Example prompt:
```
System: You are an image classifier. Respond with EXACTLY ONE of these tokens: CAT, DOG, FISH, OTHER
User: [Image] Classify this image.
```

## Testing

Run the tests:

```bash
cd lab/iter06
dotnet run
```

### Expected Test Output

```
=== Smart Image Classifier & Q&A Assistant - Lab 06 ===

Test 1: Image Classification with Logprobs
  Classifying test image...
  Classification Results:
    CAT: 0.9100 (91.00%)
    DOG: 0.0500 (5.00%)
    FISH: 0.0200 (2.00%)
    OTHER: 0.0200 (2.00%)
  ✅ PASS

Test 2: Semantic Memory Storage
  Storing image analysis in memory...
  Searching memory for 'cat'...
  Found 1 relevant memory with score 0.85
  ✅ PASS

Test 3: Q&A Conversation
  Question: What animal is this?
  Answer: This is a cat...
  Question: What are its features?
  Answer: The cat has...
  ✅ PASS

=== Test Results: 3/3 PASSED ===
🎉 All tests passed! Lab complete!
```

## Success Criteria

- ✅ Classification returns probabilities for all 4 categories
- ✅ Probabilities sum to approximately 1.0 (within 0.01)
- ✅ Logprobs are correctly extracted from API response
- ✅ Softmax normalization is properly implemented
- ✅ Semantic memory stores and retrieves image analyses
- ✅ Q&A answers questions about current and past images
- ✅ Chat history maintains conversational context

## Key Concepts Review

### Logprobs vs Regular Classification

**Without Logprobs (traditional):**
```
Response: "This is a cat"
Confidence: Unknown (you only get the answer)
```

**With Logprobs:**
```
Response: "CAT"
Logprobs: { CAT: -0.094, DOG: -2.996, FISH: -3.912, OTHER: -3.912 }
Probabilities: { CAT: 0.91, DOG: 0.05, FISH: 0.02, OTHER: 0.02 }
Confidence: 91% (you get the distribution)
```

### Why Use Logprobs?

1. **Confidence Scores**: Know how certain the model is
2. **Risk Assessment**: Make better decisions based on confidence
3. **Debugging**: Understand what other options the model considered
4. **Calibration**: Tune thresholds for classification
5. **Multiple Predictions**: Get top-k predictions with probabilities

### Softmax Mathematics

Softmax converts arbitrary real numbers (logprobs) to probabilities:

```
P(class_i) = exp(logprob_i) / Σ exp(logprob_j)
```

Properties:
- All probabilities are between 0 and 1
- Sum of all probabilities equals 1
- Preserves relative ordering

## Common Pitfalls

1. **Forgetting to normalize**: Raw logprobs aren't probabilities
2. **Wrong token extraction**: Must extract logprobs for ALL 4 categories
3. **Prompt too flexible**: Model might output something other than CAT/DOG/FISH/OTHER
4. **Numerical stability**: Very negative logprobs can cause overflow in exp()
5. **Missing logprobs parameter**: Must set `logprobs: true` in API request

## Resources

- [OpenAI Logprobs Documentation](https://platform.openai.com/docs/api-reference/chat/create#chat-create-logprobs)
- [GPT-4 Vision Guide](https://platform.openai.com/docs/guides/vision)
- [Softmax Function](https://en.wikipedia.org/wiki/Softmax_function)
- [Semantic Kernel Memory](https://learn.microsoft.com/en-us/semantic-kernel/memories/)

---

Good luck! 🚀 Remember: Start with TODO 1, verify it works, then move to TODO 2 and TODO 3.
