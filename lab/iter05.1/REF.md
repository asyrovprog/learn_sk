# Reference Guide - Lab 06

## TODO 1 Hints – Create SentimentAnalyzer

**Key Concepts:**
- PromptTemplateConfig is used for structured prompt configuration
- Handlebars uses `{{variableName}}` syntax
- InputVariables list helps LLM understand parameters

**Helpful Code Snippets:**

```csharp
var config = new PromptTemplateConfig
{
    Template = @"Your multi-line prompt here with {{text}}",
    TemplateFormat = "handlebars",
    Name = "FunctionName",
    Description = "What this function does",
    InputVariables = new List<InputVariable>
    {
        new() { Name = "text", Description = "The text to analyze", IsRequired = true }
    }
};

return kernel.CreateFunctionFromPrompt(config, new HandlebarsPromptTemplateFactory());
```

**Common Mistakes:**
- Forgetting to use `@"..."` for multi-line strings
- Missing `{{text}}` in template
- Not importing `Microsoft.SemanticKernel.PromptTemplates.Handlebars`

---

## TODO 2 Hints – Create ToxicityDetector

**Key Concepts:**
- Function description is crucial - LLM uses it to decide when to call this
- JSON output format helps with structured responses
- List specific content types to check

**Template Structure:**
```
1. State what to detect
2. List categories to check
3. Specify output format
4. Use {{text}} variable
```

**Pro Tip:** Ask for multiple fields in JSON (isToxic, categories, severity, recommendation) to get comprehensive analysis.

---

## TODO 3 Hints – Create LanguageDetector

**Key Concepts:**
- ISO 639-1 codes are 2-letter language codes
- "Return ONLY the code" prevents extra text
- Simple prompts work best for classification tasks

**Common Languages:**
- en (English)
- es (Spanish)
- fr (French)
- de (German)
- ja (Japanese)
- zh (Chinese)

---

## TODO 4 Hints – Create ModerationPlugin

**Key Concepts:**
- `[KernelFunction]` makes method callable by LLM
- `[Description("...")]` helps LLM understand when to use it
- Static functions provide hardcoded data/rules

**Method Structure:**
```csharp
[KernelFunction]
[Description("What this function returns")]
public string MethodName()
{
    return @"Multi-line content here...";
}
```

**Why Static?** The LLM can reference policy rules when making decisions, combining AI analysis with fixed guidelines.

---

## TODO 5 Hints – RunModerationAgent

**Key Concepts:**
- Register prompt functions as one plugin, static plugin separately
- ChatHistory maintains conversation context
- `ToolCallBehavior.AutoInvokeKernelFunctions` enables automatic function calling
- Temperature 0.0 makes function selection deterministic

**Step-by-Step:**
1. Create functions (call TODO 1-3 methods)
2. Register as plugins
3. Get chat service
4. Create history + add messages
5. Configure settings with ToolCallBehavior
6. Call GetChatMessageContentAsync
7. Return response

**Common Mistake:** Forgetting to pass `kernel` parameter to GetChatMessageContentAsync.

---

## Additional Tips

1. **Test incrementally:** Implement and test TODO 1, then 2, then 3, etc.
2. **Check descriptions:** Clear function descriptions are critical for LLM selection
3. **Use multiline strings:** `@"..."` makes prompts readable
4. **Debug output:** Add Console.WriteLine to see which functions are called

---

## Common Issues

- **"Handlebars not recognized"**: Add `using Microsoft.SemanticKernel.PromptTemplates.Handlebars;`
- **"Function not called"**: Check that description clearly explains when to use it
- **"Template error"**: Verify {{text}} syntax (double braces)
- **Compilation errors**: Ensure all using statements are present

---

<details>
<summary>Reference Solution (open after completion)</summary>

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using System.ComponentModel;
using System.Text.Json;

namespace ContentModerationAgent;

public class Task
{
    // TODO 1: Create SentimentAnalyzer prompt function (Handlebars)
    public static KernelFunction CreateSentimentAnalyzer(Kernel kernel)
    {
        var config = new PromptTemplateConfig
        {
            Template = @"
Analyze the sentiment of this text.
Return a JSON object with:
- sentiment: positive, negative, or neutral
- confidence: 0.0 to 1.0
- reason: brief explanation

Text: {{text}}

Output JSON only.",
            TemplateFormat = "handlebars",
            Name = "AnalyzeSentiment",
            Description = "Analyzes emotional sentiment of text content",
            InputVariables = new List<InputVariable>
            {
                new() { Name = "text", Description = "The text to analyze", IsRequired = true }
            }
        };

        return kernel.CreateFunctionFromPrompt(config, new HandlebarsPromptTemplateFactory());
    }

    // TODO 2: Create ToxicityDetector prompt function (Handlebars)
    public static KernelFunction CreateToxicityDetector(Kernel kernel)
    {
        var config = new PromptTemplateConfig
        {
            Template = @"
Detect if this content contains:
- Hate speech
- Harassment
- Violence
- Adult content
- Spam

Text: {{text}}

Return JSON:
{
  ""isToxic"": true/false,
  ""categories"": [""list of violations""],
  ""severity"": ""low/medium/high/critical"",
  ""recommendation"": ""allow/flag/block""
}",
            TemplateFormat = "handlebars",
            Name = "DetectToxicity",
            Description = "Detects harmful, toxic, or policy-violating content",
            InputVariables = new List<InputVariable>
            {
                new() { Name = "text", Description = "The text to check for toxicity", IsRequired = true }
            }
        };

        return kernel.CreateFunctionFromPrompt(config, new HandlebarsPromptTemplateFactory());
    }

    // TODO 3: Create LanguageDetector prompt function (Handlebars)
    public static KernelFunction CreateLanguageDetector(Kernel kernel)
    {
        var config = new PromptTemplateConfig
        {
            Template = @"
Detect the language of this text.
Return ISO 639-1 code (e.g., en, es, fr, de, ja, zh).

Text: {{text}}

Return ONLY the 2-letter code.",
            TemplateFormat = "handlebars",
            Name = "DetectLanguage",
            Description = "Identifies the language of text content",
            InputVariables = new List<InputVariable>
            {
                new() { Name = "text", Description = "The text to detect language for", IsRequired = true }
            }
        };

        return kernel.CreateFunctionFromPrompt(config, new HandlebarsPromptTemplateFactory());
    }

    // TODO 4: Create static ModerationPlugin with GetModerationGuidelines
    public class ModerationPlugin
    {
        [KernelFunction]
        [Description("Returns the content moderation policy guidelines")]
        public string GetModerationGuidelines()
        {
            return @"
CONTENT MODERATION POLICY:

BLOCK immediately:
- Hate speech, harassment, threats
- Explicit violence or gore
- Adult/sexual content
- Spam or phishing attempts

FLAG for review:
- Borderline offensive language
- Political or controversial topics
- Unverified claims or misinformation

ALLOW:
- Constructive criticism
- Educational content
- Personal opinions (non-hateful)
- General discussions

When uncertain, err on side of safety and flag for human review.";
        }
    }

    // TODO 5: Register all functions and demonstrate usage
    public static async Task<string> RunModerationAgent(
        Kernel kernel,
        string userQuery,
        string? contentToModerate = null)
    {
        // Register all prompt functions
        var sentimentFunc = CreateSentimentAnalyzer(kernel);
        var toxicityFunc = CreateToxicityDetector(kernel);
        var languageFunc = CreateLanguageDetector(kernel);

        kernel.Plugins.AddFromFunctions("ModerationTools", new[]
        {
            sentimentFunc,
            toxicityFunc,
            languageFunc
        });

        // Register static plugin
        kernel.Plugins.AddFromType<ModerationPlugin>();

        // Create ChatHistory
        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var history = new ChatHistory();
        history.AddSystemMessage("You are a content moderation assistant. Use available tools to analyze content and make moderation decisions.");

        // Add user query
        if (!string.IsNullOrEmpty(contentToModerate))
        {
            history.AddUserMessage($"{userQuery}\n\nContent: {contentToModerate}");
        }
        else
        {
            history.AddUserMessage(userQuery);
        }

        // Enable automatic function calling
        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.0
        };

        // Get response
        var response = await chatService.GetChatMessageContentAsync(history, settings, kernel);

        return response.Content ?? "";
    }
}
```

</details>

---

**Lab Complete!** ��

This lab taught you:
- ✅ Creating prompt functions with Handlebars
- ✅ Mixing prompts with static C# functions
- ✅ Using GetChatMessageContentAsync with automatic function calling
- ✅ How LLM selects functions based on descriptions
- ✅ The powerful "Prompts as Functions" pattern for AI agents

Next steps: Try the bonus challenges or move to the next learning module!
