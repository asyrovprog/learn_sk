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
        // TODO[1]: Create SentimentAnalyzer prompt function
        // [YOUR CODE GOES HERE]
        //
        // Instructions from README:
        // 1. Create a `PromptTemplateConfig` with:
        //    - Template: Handlebars format (use {{text}} variable)
        //    - TemplateFormat: "handlebars"
        //    - Name: "AnalyzeSentiment"
        //    - Description: Clear description for LLM to understand when to use this function
        //    - InputVariables: Define "text" parameter
        // 2. The prompt should:
        //    - Analyze sentiment (positive/negative/neutral)
        //    - Return confidence score (0.0 to 1.0)
        //    - Provide brief reasoning
        //    - Output as JSON
        // 3. Return `kernel.CreateFunctionFromPrompt(config, new HandlebarsPromptTemplateFactory())`
        //
        // Hint: Use multiline string with @"..." for template
        
        var template = @"
Analyze the sentiment of the following text. Be nuanced and consider:
- Strong/clear sentiment should have HIGH confidence (0.8-1.0)
- Weak/subtle sentiment should have MEDIUM confidence (0.5-0.7)
- Mixed/ambiguous sentiment should have LOW confidence (0.2-0.5)

{{!-- Need to provide moderation guidelines --}}
{{ModerationPlugin-GetModerationGuidelines}}

Return ONLY a JSON object:
{
  ""sentiment"": ""positive"" or ""negative"" or ""neutral"",
  ""confidence"": 0.0 to 1.0 (be realistic and varied based on sentiment clarity),
  ""reasoning"": ""brief explanation of why this confidence level""
}

Text to analyze:
{{text}}";

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object",
            Temperature = 0.3,  // Some variance to get different confidence levels
        };
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


        var promptConfig = new PromptTemplateConfig
        {
            Template = template,
            TemplateFormat = "handlebars",
            Name = "AnalyzeSentiment",
            Description = "Analyzes the sentiment of text and returns sentiment classification with confidence score",
            InputVariables = new List<InputVariable>
            {
                new InputVariable
                {
                    Name = "text",
                    Description = "The text to analyze for sentiment",
                    IsRequired = true
                }
            }
        };

        promptConfig.ExecutionSettings.Add(
            OpenAIPromptExecutionSettings.DefaultServiceId, executionSettings);

        var prompt = kernel.CreateFunctionFromPrompt(
            promptConfig: promptConfig,
            promptTemplateFactory: new HandlebarsPromptTemplateFactory()
        );

        return prompt;
    }

    // TODO 2: Create ToxicityDetector prompt function (Handlebars)
    public static KernelFunction CreateToxicityDetector(Kernel kernel)
    {
        // TODO[2]: Create ToxicityDetector prompt function
        // [YOUR CODE GOES HERE]
        //
        // Instructions from README:
        // 1. Create a `PromptTemplateConfig` with:
        //    - Template: Handlebars format (use {{text}} variable)
        //    - TemplateFormat: "handlebars"
        //    - Name: "DetectToxicity"
        //    - Description: Clear description for detecting harmful content
        //    - InputVariables: Define "text" parameter
        // 2. The prompt should detect:
        //    - Hate speech, harassment, violence, adult content, spam
        //    - Return JSON with: isToxic, categories, severity, recommendation
        // 3. Return `kernel.CreateFunctionFromPrompt(config, new HandlebarsPromptTemplateFactory())`

        var template = @"
Detects harmful, toxic, or policy-violating content in the the following text and return ONLY a JSON object with this exact structure:
{
    ""isToxic"": true or false,
    ""categories"": [""list of violations one or more: hate speech, harassment, violence, adult content, spam""],
    ""severity"": ""low"" or ""medium"" or ""high"" or ""critical"",
    ""recommendation"": ""allow"" or ""flag"" or ""block""
}

Text to analyze:
{{text}}";

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "json_object", // Forces JSON output
            Temperature = 0.3,
        };
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


        var promptConfig = new PromptTemplateConfig
        {
            Template = template,
            TemplateFormat = "handlebars",
            Name = "DetectToxicity",
            Description = "Detects harmful, toxic, or policy-violating content in the text category, severity and recommendation",
            InputVariables = new List<InputVariable>
            {
                new InputVariable
                {
                    Name = "text",
                    Description = "The text to analyze for violation",
                    IsRequired = true
                }
            }
        };

        promptConfig.ExecutionSettings.Add(
            OpenAIPromptExecutionSettings.DefaultServiceId, executionSettings);

        var prompt = kernel.CreateFunctionFromPrompt(
            promptConfig: promptConfig,
            promptTemplateFactory: new HandlebarsPromptTemplateFactory()
        );

        return prompt;
    }

    // TODO 3: Create LanguageDetector prompt function (Handlebars)
    public static KernelFunction CreateLanguageDetector(Kernel kernel)
    {
        // TODO[3]: Create LanguageDetector prompt function
        // [YOUR CODE GOES HERE]
        //
        // Instructions from README:
        // 1. Create a `PromptTemplateConfig` with:
        //    - Template: Handlebars format (use {{text}} variable)
        //    - TemplateFormat: "handlebars"
        //    - Name: "DetectLanguage"
        //    - Description: Identifies the language of text
        //    - InputVariables: Define "text" parameter
        // 2. The prompt should:
        //    - Detect language
        //    - Return ISO 639-1 code (en, es, fr, de, etc.)
        //    - Return ONLY the 2-letter code
        // 3. Return `kernel.CreateFunctionFromPrompt(config, new HandlebarsPromptTemplateFactory())`
        var template = @"
Identify language of the following text and return ONLY 2-letter ISO 639-1 code for it (en, es, fr, de, etc.):

Text to analyze:
{{text}}";

#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ResponseFormat = "string",
            Temperature = 0.3,
        };
#pragma warning restore SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


        var promptConfig = new PromptTemplateConfig
        {
            Template = template,
            TemplateFormat = "handlebars",
            Name = "DetectLanguage",
            Description = "Identifies the language of text",
            InputVariables = new List<InputVariable>
            {
                new InputVariable
                {
                    Name = "text",
                    Description = "The text to identify language",
                    IsRequired = true
                }
            }
        };

        promptConfig.ExecutionSettings.Add(
            OpenAIPromptExecutionSettings.DefaultServiceId, executionSettings);

        var prompt = kernel.CreateFunctionFromPrompt(
            promptConfig: promptConfig,
            promptTemplateFactory: new HandlebarsPromptTemplateFactory()
        );

        return prompt;

    }

    // TODO 4: Create static ModerationPlugin with GetModerationGuidelines
    public class ModerationPlugin
    {
        // TODO[4]: Implement GetModerationGuidelines static function
        // [YOUR CODE GOES HERE]
        //
        // Instructions from README:
        // 1. Add `[KernelFunction]` attribute
        // 2. Add `[Description("...")]` attribute explaining what this function returns
        // 3. Implement method that returns hardcoded moderation policy string
        // 4. Policy should include:
        //    - BLOCK immediately: hate speech, harassment, violence, adult content, spam
        //    - FLAG for review: borderline offensive, political topics, misinformation
        //    - ALLOW: constructive criticism, educational content, personal opinions
        // 5. Return type: string

        [KernelFunction]
        [Description("Returns the content moderation policy guidelines")]
        public string GetModerationGuidelines()
        {
            var policy = @"
Moderation policy guidelines:

**BLOCK immediately:**
- Hate speech, harassment, threats
- Explicit violence or gore
- Adult/sexual content
- Spam or phishing attempts

**FLAG for review:**
- Borderline offensive language
- Political or controversial topics
- Unverified claims or misinformation

**ALLOW:**
- Constructive criticism
- Educational content
- Personal opinions (non-hateful)
- General discussions";            
            
            return policy;
        }
    }

    // TODO 5: Register all functions and demonstrate usage
    public static async Task<string> RunModerationAgent(
        Kernel kernel,
        string userQuery,
        string? contentToModerate = null)
    {
        // TODO[5]: Implement RunModerationAgent
        // [YOUR CODE GOES HERE]
        //
        // Instructions from README:
        // 1. Create all prompt functions using TODO 1-3 methods
        // 2. Register prompt functions as "ModerationTools" plugin using:
        //    kernel.Plugins.AddFromFunctions("ModerationTools", new[] { func1, func2, func3 })
        // 3. Register static ModerationPlugin using:
        //    kernel.Plugins.AddFromType<ModerationPlugin>()
        // 4. Get IChatCompletionService from kernel
        // 5. Create ChatHistory and add system message
        // 6. Add user message (combine userQuery and contentToModerate if provided)
        // 7. Create OpenAIPromptExecutionSettings with:
        //    - ToolCallBehavior.AutoInvokeKernelFunctions
        //    - Temperature = 0.0 (deterministic function selection)
        // 8. Call GetChatMessageContentAsync with history, settings, kernel
        // 9. Return response.Content

        var sentinel = CreateSentimentAnalyzer(kernel);
        var toxicity = CreateToxicityDetector(kernel);
        var language = CreateLanguageDetector(kernel);

        // Remove existing plugins if they exist to avoid duplicate key errors
        if (kernel.Plugins.Contains("ModerationTools"))
        {
            kernel.Plugins.Remove(kernel.Plugins["ModerationTools"]);
        }
        if (kernel.Plugins.Contains("ModerationPlugin"))
        {
            kernel.Plugins.Remove(kernel.Plugins["ModerationPlugin"]);
        }

        kernel.Plugins.AddFromFunctions("ModerationTools", [sentinel, toxicity, language]);
        kernel.Plugins.AddFromType<ModerationPlugin>();

        var history = new ChatHistory();
        history.AddSystemMessage(@"You are a content moderation assistant. Use available tools to analyze content and make moderation decisions.

Available tools:
- GetModerationGuidelines: Use when user asks about moderation policy, rules, or guidelines
- AnalyzeSentiment: Analyze emotional tone of content
- DetectToxicity: Check for harmful or policy-violating content
- DetectLanguage: Identify the language of content

When asked about guidelines or policy, ALWAYS call GetModerationGuidelines first.");

        if (!string.IsNullOrEmpty(contentToModerate))
        {
            history.AddUserMessage($"{userQuery}\n\nContent: {contentToModerate}");
        }
        else
        {
            history.AddUserMessage(userQuery);
        }

        var settings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.0  // Deterministic function selection
        };

        var chatService = kernel.GetRequiredService<IChatCompletionService>();
        var response = await chatService.GetChatMessageContentAsync(history, settings, kernel);
        return response.Content ?? "";    
    }
}
