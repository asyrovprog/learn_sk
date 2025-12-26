# Reference and Hints

## Hint for TODO 1 – Image Classification with Logprobs

**Objective:** Get GPT-4 Vision to classify an image and extract confidence scores using logprobs.

**Key Steps:**

1. **Create Chat History with System Message**
   ```csharp
   var chatHistory = new ChatHistory();
   chatHistory.AddSystemMessage("You are an image classifier. Respond with EXACTLY ONE of these tokens: CAT, DOG, FISH, OTHER");
   ```

2. **Add User Message with Image**
   ```csharp
   chatHistory.AddUserMessage(new ChatMessageContentItemCollection {
       new TextContent("Classify this image."),
       new ImageContent(new Uri(imageUrl))
   });
   ```

3. **Configure Execution Settings for Logprobs**
   ```csharp
   var settings = new OpenAIPromptExecutionSettings
   {
       Logprobs = true,           // Enable logprob return
       TopLogprobs = 4,           // Get top 4 alternatives
       MaxTokens = 1,             // Force single token
       Temperature = 0.0          // Deterministic
   };
   ```

4. **Extract Logprobs from Response Metadata**
   
   The logprobs are in: `response.Metadata?["ContentTokenLogProbabilities"]`
   
   You'll need to cast this to the appropriate type. Look at the Semantic Kernel source or use reflection to inspect the type.

5. **Apply Softmax to Convert Logprobs to Probabilities**
   
   Use the provided `Softmax()` helper method.

**Common Issues:**

- **Missing categories**: If a category isn't in top_logprobs, assign it a low default logprob (-10.0)
- **Probabilities don't sum to 1.0**: Make sure you're normalizing across all 4 categories, not just the ones returned
- **Metadata is null**: Check if the API call succeeded and settings were applied correctly

---

## Hint for TODO 2 – Semantic Memory Storage

**Objective:** Store image analysis results in semantic memory for later retrieval.

**Key Steps:**

1. **Check Memory is Available**
   ```csharp
   if (_memory == null) return;
   ```

2. **Create Text Summary**
   ```csharp
   var summary = $"Image: {imageName}. Description: {description}. " +
                 $"Classification: {classification.TopCategory} " +
                 $"(confidence: {classification.TopProbability:P0})";
   ```

3. **Save to Memory**
   ```csharp
   await _memory.SaveInformationAsync(
       collection: MemoryCollectionName,
       text: summary,
       id: imageName
   );
   ```

**What Happens:**
- Memory system automatically creates embeddings for the text
- Stored in the "image_analyses" collection
- Can be searched semantically later

---

## Hint for TODO 3 – Q&A with Memory Context

**Objective:** Answer questions about images while incorporating relevant past analyses from memory.

**Key Steps:**

1. **Search Semantic Memory**
   ```csharp
   var memories = _memory != null
       ? await _memory.SearchAsync(MemoryCollectionName, question, limit: 3).ToListAsync()
       : new List<MemoryQueryResult>();
   ```

2. **Build Context String**
   ```csharp
   var context = memories.Any()
       ? "Relevant past analyses:\n" + string.Join("\n", 
           memories.Select(m => $"- {m.Metadata.Text}"))
       : "";
   ```

3. **Create Multi-Modal Message**
   ```csharp
   var messageItems = new ChatMessageContentItemCollection();
   
   if (!string.IsNullOrEmpty(context))
       messageItems.Add(new TextContent(context + "\n\n"));
   
   messageItems.Add(new TextContent(question));
   
   if (!string.IsNullOrEmpty(imageUrl))
       messageItems.Add(new ImageContent(new Uri(imageUrl)));
   ```

4. **Add to Chat History and Get Response**
   ```csharp
   _chatHistory.AddUserMessage(messageItems);
   var response = await _chatService.GetChatMessageContentAsync(_chatHistory);
   _chatHistory.AddAssistantMessage(response.Content ?? "");
   return response.Content ?? "";
   ```

**Flow:**
1. User asks question → Search memory for relevant context
2. Combine context + question + optional image
3. Send to GPT-4 Vision
4. Add exchange to chat history (maintains conversation context)
5. Return answer

---

## Advanced Tips

### Logprobs Extraction

The `ContentTokenLogProbabilities` metadata contains a collection of token logprob information. Here's a more detailed extraction pattern:

```csharp
// Extract logprob data from metadata
var logprobsData = response.Metadata?["ContentTokenLogProbabilities"];

// This will be a collection - iterate through it
// Look for properties like: Token, Logprob, TopLogprobs
// TopLogprobs contains alternatives with their logprobs
```

### Numerical Stability in Softmax

For very negative logprobs (< -20), you might encounter numerical issues. Here's a stable version:

```csharp
private double[] StableSoftmax(double[] logprobs)
{
    // Subtract max for numerical stability
    var maxLogprob = logprobs.Max();
    var exps = logprobs.Select(x => Math.Exp(x - maxLogprob)).ToArray();
    var sum = exps.Sum();
    return exps.Select(x => x / sum).ToArray();
}
```

### Memory Search Relevance Threshold

You might want to filter out low-relevance memories:

```csharp
var relevantMemories = memories.Where(m => m.Relevance > 0.7).ToList();
```

---

<details>
<summary>Reference Solution (open after completion)</summary>

```csharp
// TODO 1: Image Classification with Logprobs
public async Task<ClassificationResult> ClassifyImageAsync(string imageUrl)
{
    var chatHistory = new ChatHistory();
    chatHistory.AddSystemMessage(
        "You are an image classifier. Respond with EXACTLY ONE of these tokens: CAT, DOG, FISH, OTHER"
    );
    
    chatHistory.AddUserMessage(new ChatMessageContentItemCollection
    {
        new TextContent("Classify this image."),
        new ImageContent(new Uri(imageUrl))
    });
    
    var settings = new OpenAIPromptExecutionSettings
    {
        Logprobs = true,
        TopLogprobs = 4,
        MaxTokens = 1,
        Temperature = 0.0
    };
    
    var response = await _chatService.GetChatMessageContentAsync(
        chatHistory,
        settings
    );
    
    // Extract logprobs from metadata
    var logprobsData = response.Metadata?["ContentTokenLogProbabilities"] as IEnumerable<object>;
    
    if (logprobsData == null)
        throw new Exception("Failed to get logprobs from response");
    
    // Find logprobs for each category
    var categoryLogprobs = new Dictionary<string, double>();
    
    foreach (var item in logprobsData)
    {
        // Use reflection to access properties
        var topLogprobsProp = item.GetType().GetProperty("TopLogprobs");
        var topLogprobs = topLogprobsProp?.GetValue(item) as IEnumerable<object>;
        
        if (topLogprobs != null)
        {
            foreach (var logprobItem in topLogprobs)
            {
                var tokenProp = logprobItem.GetType().GetProperty("Token");
                var logprobProp = logprobItem.GetType().GetProperty("Logprob");
                
                var token = tokenProp?.GetValue(logprobItem)?.ToString();
                var logprob = logprobProp?.GetValue(logprobItem);
                
                if (token != null && logprob != null && _categories.Contains(token))
                {
                    categoryLogprobs[token] = Convert.ToDouble(logprob);
                }
            }
        }
    }
    
    // Fill in missing categories with low default
    foreach (var category in _categories)
    {
        if (!categoryLogprobs.ContainsKey(category))
        {
            categoryLogprobs[category] = -10.0;
        }
    }
    
    // Convert to probabilities using softmax
    var logprobArray = _categories.Select(c => categoryLogprobs[c]).ToArray();
    var probabilities = Softmax(logprobArray);
    
    var result = new ClassificationResult();
    for (int i = 0; i < _categories.Length; i++)
    {
        result.CategoryProbabilities[_categories[i]] = probabilities[i];
    }
    
    return result;
}

// TODO 2: Store Image Analysis
public async Task StoreImageAnalysisAsync(
    string imageName,
    string description,
    ClassificationResult classification)
{
    if (_memory == null)
        return;
    
    var summary = $"Image: {imageName}. Description: {description}. " +
                  $"Classification: {classification.TopCategory} " +
                  $"(confidence: {classification.TopProbability:P0}). " +
                  $"All scores - CAT: {classification.CategoryProbabilities["CAT"]:P0}, " +
                  $"DOG: {classification.CategoryProbabilities["DOG"]:P0}, " +
                  $"FISH: {classification.CategoryProbabilities["FISH"]:P0}, " +
                  $"OTHER: {classification.CategoryProbabilities["OTHER"]:P0}";
    
    await _memory.SaveInformationAsync(
        collection: MemoryCollectionName,
        text: summary,
        id: imageName,
        description: description
    );
}

// TODO 3: Q&A with Memory Context
public async Task<string> AskQuestionAsync(string question, string? imageUrl = null)
{
    // Search semantic memory for context
    var memories = _memory != null
        ? await _memory.SearchAsync(MemoryCollectionName, question, limit: 3).ToListAsync()
        : new List<MemoryQueryResult>();
    
    // Build context from memories
    var context = memories.Any()
        ? "Relevant past image analyses:\n" + string.Join("\n",
            memories.Select(m => $"- {m.Metadata.Text} (relevance: {m.Relevance:F2})"))
        : "";
    
    // Create message with context + question + optional image
    var messageItems = new ChatMessageContentItemCollection();
    
    if (!string.IsNullOrEmpty(context))
    {
        messageItems.Add(new TextContent(context + "\n\nUser question: "));
    }
    
    messageItems.Add(new TextContent(question));
    
    if (!string.IsNullOrEmpty(imageUrl))
    {
        messageItems.Add(new ImageContent(new Uri(imageUrl)));
    }
    
    _chatHistory.AddUserMessage(messageItems);
    
    // Get response
    var response = await _chatService.GetChatMessageContentAsync(_chatHistory);
    
    // Add to history
    _chatHistory.AddAssistantMessage(response.Content ?? "");
    
    return response.Content ?? "";
}
```

</details>

---

**Good luck with your implementation!** 🚀

Remember:
1. Start with TODO 1 and verify it works before moving on
2. Read compiler errors carefully - they often point to the solution
3. Use `Console.WriteLine()` to debug intermediate values
4. Check the test output to understand what's expected
5. Refer to learn06.md and learn06_realtime.md for multimodal concepts

