using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ImageClassifierQA;

#pragma warning disable SKEXP0010 // Logprobs is for evaluation purposes only and is subject to change or removal in future updates
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0050 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
  

/// <summary>
/// Represents classification result with confidence scores
/// </summary>
public class ClassificationResult
{
    public Dictionary<string, double> CategoryProbabilities { get; set; } = new();
    public string TopCategory => CategoryProbabilities.OrderByDescending(x => x.Value).First().Key;
    public double TopProbability => CategoryProbabilities.OrderByDescending(x => x.Value).First().Value;
}

/// <summary>
/// Image classifier using GPT-4 Vision with logprobs for confidence scores
/// </summary>
public class ImageClassifier
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly string[] _categories = { "CAT", "DOG", "FISH", "OTHER" };

    public ImageClassifier(Kernel kernel)
    {
        _kernel = kernel;
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
    }

    /// <summary>
    /// TODO 1: Classify image and return probabilities for all categories
    /// </summary>
    public async Task<ClassificationResult> ClassifyImageAsync(string imageUrl)
    {
        // TODO[1]: Implement image classification with logprobs
        // [YOUR CODE GOES HERE]
        //
        // Instructions from README:
        // 1. Create a ChatHistory with system message instructing to respond with EXACTLY ONE token: CAT, DOG, FISH, or OTHER
        // 2. Add user message with the image (use ImageContent)
        // 3. Create OpenAIPromptExecutionSettings with:
        //    - Logprobs = true
        //    - TopLogprobs = 4 (to get all 4 categories)
        //    - MaxTokens = 1 (force single token response)
        //    - Temperature = 0 (deterministic)
        // 4. Call GetChatMessageContentAsync with the settings
        // 5. Extract logprobs from metadata:
        //    - Get response.Metadata?["ContentTokenLogProbabilities"]
        //    - Cast to the appropriate type to access token logprobs
        // 6. Find logprobs for each of the 4 categories (CAT, DOG, FISH, OTHER)
        //    - Check both the main token and top_logprobs list
        //    - Use a default low logprob (-10.0) if category not found
        // 7. Apply Softmax normalization to convert logprobs to probabilities
        // 8. Create and return ClassificationResult with the probability distribution
        //
        // Example:
        // var chatHistory = new ChatHistory();
        // chatHistory.AddSystemMessage("You are an image classifier. Respond with EXACTLY ONE of these tokens: CAT, DOG, FISH, OTHER");
        // chatHistory.AddUserMessage(new ChatMessageContentItemCollection {
        //     new TextContent("Classify this image."),
        //     new ImageContent(new Uri(imageUrl))
        // });
        //
        // Hint: Softmax formula: P(i) = exp(logprob_i) / sum(exp(logprob_j) for all j)

        var opts = new OpenAIPromptExecutionSettings()
        {
            Logprobs = true,
            TopLogprobs = 4, // log probability for all 4 categories
            MaxTokens = 100,
            Temperature = 0,
        };

        var history = new ChatHistory();

        history.AddSystemMessage(@"
        You need to classify user image provided to you by the image url in one of
        the following 4 categories: CAT, DOG, FISH, OTHER. You must output 0-based id of a
        category only. For instance: 0 for CAT or 1 for DOG. You must output a single word only.
        If you classify as cat or dog or fish that must be picture of real, alive cat or dog or fish.
        Do not output anything except 0 or 1 or 2 or 3.
        YOU MUST output a single digit only.
        For example if the image is of cat. Than just output: 0");

        history.AddUserMessage(imageUrl);
        
        var response = await _chatService.GetChatMessageContentAsync(
            history, 
            opts).ConfigureAwait(false);

        var defaultLogProb = -10.0;

        Console.WriteLine($"RESPONSE: {response.Content}");

        var result = new ClassificationResult();
        result.CategoryProbabilities.Add("CAT", defaultLogProb);
        result.CategoryProbabilities.Add("DOG", defaultLogProb);
        result.CategoryProbabilities.Add("FISH", defaultLogProb);
        result.CategoryProbabilities.Add("OTHER", defaultLogProb);

        var meta = response.Metadata?["ContentTokenLogProbabilities"];
        var lp = meta as List<ChatTokenLogProbabilityDetails>;
        if (lp == null || lp.Count != 1 || lp[0] == null)
        {
            throw new Exception("ContentTokenLogProbabilities could not be read");
        }
        var cats = lp[0]!;
        foreach (var p in cats.TopLogProbabilities)
        {
            var id = p.Token;
            var logProb = p.LogProbability;
            var name = "";
            switch (id)
            {
                case "0": name = "CAT"; break;
                case "1": name = "DOG"; break;
                case "2": name = "FISH"; break;
                case "3": name = "OTHER"; break;
                default:
                    Console.WriteLine($"Unexpected token: {id}");
                    break;
            }
            if (result.CategoryProbabilities.ContainsKey(name))
            {
                result.CategoryProbabilities[name] = logProb;
            }
        }

        var sum = 0.0;
        foreach (var kv in result.CategoryProbabilities)
        {
            var exp = Math.Exp(kv.Value);
            result.CategoryProbabilities[kv.Key] = exp;
            sum += exp;
        }

        foreach (var kv in result.CategoryProbabilities)
        {
            var norm = kv.Value / sum;
            result.CategoryProbabilities[kv.Key] = norm;
        }

        return result;
    }

    /// <summary>
    /// Helper: Apply softmax normalization to logprobs
    /// </summary>
    private double[] Softmax(double[] logprobs)
    {
        var exps = logprobs.Select(x => Math.Exp(x)).ToArray();
        var sum = exps.Sum();
        return exps.Select(x => x / sum).ToArray();
    }
}

/// <summary>
/// Q&A Assistant with semantic memory for image analyses
/// </summary>
public class ImageQAAssistant
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;
    private readonly ISemanticTextMemory? _memory;
    private readonly ChatHistory _chatHistory;
    private const string MemoryCollectionName = "image_analyses";

    public ImageQAAssistant(Kernel kernel, ISemanticTextMemory? memory = null)
{
        _kernel = kernel;
        _chatService = kernel.GetRequiredService<IChatCompletionService>();
        _memory = memory;
        _chatHistory = new ChatHistory();
        _chatHistory.AddSystemMessage("You are a helpful image analysis assistant. Answer questions about images based on what you see and any relevant context provided.");
    }

    /// <summary>
    /// TODO 2: Store image analysis in semantic memory
    /// </summary>
    public async Task StoreImageAnalysisAsync(string imageName, string description, ClassificationResult classification)
    {
        // TODO[2]: Store image analysis in semantic memory
        // [YOUR CODE GOES HERE]
        //
        // Instructions from README:
        // 1. Check if _memory is not null
        // 2. Create a text summary combining:
        //    - Image name
        //    - Description
        //    - Classification results (category and probability)
        // 3. Use _memory.SaveInformationAsync() to store:
        //    - collection: MemoryCollectionName
        //    - text: the summary you created
        //    - id: imageName (or generate unique ID)
        //    - description: short description
        // 4. The memory system will automatically create embeddings
        //
        // Example:
        // if (_memory != null)
        // {
        //     var summary = $"Image: {imageName}. {description}. Classification: {classification.TopCategory} ({classification.TopProbability:P0} confidence)";
        //     await _memory.SaveInformationAsync(MemoryCollectionName, summary, imageName);
        // }
        
        throw new NotImplementedException("TODO[2]: Store image analysis in semantic memory - See README section 'TODO 2 – Semantic Memory Integration'");
    }

    /// <summary>
    /// TODO 3: Answer question about image with memory context
    /// </summary>
    public async Task<string> AskQuestionAsync(string question, string? imageUrl = null)
    {
        // TODO[3]: Implement Q&A with semantic memory and chat history
        // [YOUR CODE GOES HERE]
        //
        // Instructions from README:
        // 1. Search semantic memory for relevant context (if _memory is not null):
        //    - Use _memory.SearchAsync(MemoryCollectionName, question, limit: 3)
        //    - Extract relevant past analyses
        // 2. Build context string from memory results
        // 3. Create user message with:
        //    - Context from memory (if any)
        //    - The user's question
        //    - Image (if imageUrl provided) using ImageContent
        // 4. Add user message to _chatHistory
        // 5. Call _chatService.GetChatMessageContentAsync(_chatHistory)
        // 6. Add AI response to _chatHistory
        // 7. Return the response content
        //
        // Example structure:
        // // Search memory
        // var memories = _memory != null 
        //     ? await _memory.SearchAsync(MemoryCollectionName, question, limit: 3).ToListAsync()
        //     : new List<MemoryQueryResult>();
        //
        // // Build context
        // var context = memories.Any() 
        //     ? "Relevant past analyses:\n" + string.Join("\n", memories.Select(m => $"- {m.Metadata.Text}"))
        //     : "";
        //
        // // Create message with context + question + optional image
        // var messageItems = new ChatMessageContentItemCollection();
        // if (!string.IsNullOrEmpty(context))
        //     messageItems.Add(new TextContent(context + "\n\n"));
        // messageItems.Add(new TextContent(question));
        // if (!string.IsNullOrEmpty(imageUrl))
        //     messageItems.Add(new ImageContent(new Uri(imageUrl)));
        //
        // _chatHistory.AddUserMessage(messageItems);
        
        throw new NotImplementedException("TODO[3]: Implement Q&A with memory context - See README section 'TODO 3 – Conversational Q&A Loop'");
    }

    public ChatHistory GetChatHistory() => _chatHistory;
}
