using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace TranslationPluginLab;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.


public class TranslationManager
{
    private readonly Kernel _kernel;

    public TranslationManager(string apiKey)
    {
        _kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4o-mini", apiKey)
            .Build();
    }

    // TODO[1]: Create Translation Semantic Function
    // Objective: Create a semantic function that translates text to a specified language using proper prompt engineering techniques.
    // [YOUR CODE GOES HERE]
    // Instructions:
    // 1. Create a semantic function using kernel.CreateFunctionFromPrompt()
    // 2. Use a clear, specific prompt template that:
    //    - Takes two template variables: {{$text}} and {{$targetLanguage}}
    //    - Instructs the AI to translate accurately
    //    - Specifies to preserve meaning and tone
    // 3. Set appropriate execution settings:
    //    - Temperature: 0.3 (low for accuracy)
    //    - MaxTokens: 500
    // 4. Provide function name: "Translate"
    // 5. Provide description: "Translates text to the specified target language"
    // 6. Return the created function
    // Requirements:
    // - Use {{$text}} for the input text variable
    // - Use {{$targetLanguage}} for the target language variable
    // - Set temperature to 0.3 for deterministic translation
    // - Include clear instructions in the prompt about preserving meaning
    public KernelFunction CreateTranslationFunction()
    {
        // [YOUR CODE GOES HERE]
        var template = @"
        Translate the following to {{$targetLanguage}} language, be accurate, preserve meaning and tone:
        {{$text}}";

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0.3,
            MaxTokens = 500
        };

        var name = "Translate";
        var description = "Translates text to the specified target language";

        var function = _kernel.CreateFunctionFromPrompt(
            promptTemplate: template, 
            functionName: name, 
            description: description, 
            executionSettings: executionSettings);

        return function;
    }

    // TODO[2]: Register Functions as Plugin and Enable Auto Function Calling
    // Objective: Register semantic functions as a plugin and configure automatic function calling so the AI can automatically select and invoke the translation function.
    // [YOUR CODE GOES HERE]
    // Instructions:
    // 1. Register the translation function as a plugin:
    //    - Plugin name: "TranslationPlugin"
    //    - Use kernel.Plugins.AddFromFunctions()
    // 2. Get the chat completion service from the kernel
    // 3. Create execution settings with ToolCallBehavior.AutoInvokeKernelFunctions to enable automatic function calling
    // 4. Create a ChatHistory and add the user's message
    // 5. Get the chat response with automatic function calling enabled
    // 6. Return the response content as a string
    // Requirements:
    // - Plugin must be named "TranslationPlugin"
    // - Must use ToolCallBehavior.AutoInvokeKernelFunctions
    // - Must properly configure chat completion settings
    // - Return the final AI response after function execution
    public async Task<string> TranslateWithAutoFunctionAsync(KernelFunction translateFunction, string userMessage)
    {
        // [YOUR CODE GOES HERE]
        _kernel.Plugins.AddFromFunctions("TranslationPlugin", [translateFunction]);
        var chat = _kernel.GetRequiredService<IChatCompletionService>();

        var history = new ChatHistory();
        history.AddUserMessage(userMessage);

        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var result = await chat.GetChatMessageContentAsync(history, settings, _kernel).ConfigureAwait(false);

        return result?.Content ?? "";
    }

    // TODO[3]: Create Multi-Language Output Function
    // Objective: Create a semantic function that translates input text to multiple languages simultaneously, 
    // demonstrating function chaining and output formatting.
    // [YOUR CODE GOES HERE]
    // Instructions:
    // 1. Create a semantic function that:
    //    - Takes {{$text}} as input
    //    - Translates to Spanish, French, and German
    //    - Formats output as a structured list with language names
    // 2. Use the translation function created in TODO 1 within the template:
    //    - Syntax: {{TranslationPlugin.Translate text=<value> targetLanguage=<value>}}
    // 3. Set appropriate execution settings:
    //    - Temperature: 0.3
    //    - MaxTokens: 1000
    // 4. Provide function name: "TranslateToMultiple"
    // 5. Return the created function
    // Requirements:
    // - Must translate to exactly: Spanish, French, and German
    // - Must use the TranslationPlugin.Translate function calls in the template
    // - Output must be formatted with language labels
    // - Temperature must be 0.3
    public KernelFunction CreateMultiLanguageFunction()
    {
        // [YOUR CODE GOES HERE]
        var template = @"
        - Spanish: {{TranslationPlugin.Translate text=$text targetLanguage=Spanish}}
        - French: {{TranslationPlugin.Translate text=$text targetLanguage=French}}
        - German: {{TranslationPlugin.Translate text=$text targetLanguage=German}}";

        var executionSettings = new OpenAIPromptExecutionSettings
        {
            Temperature = 0.3,
            MaxTokens = 1000
        };

        var name = "TranslateToMultiple";
        var description = "Translates text to Spanish, French, and German";

        var function = _kernel.CreateFunctionFromPrompt(
            promptTemplate: template, 
            functionName: name, 
            description: description, 
            executionSettings: executionSettings);

        return function;
    }

    public Kernel GetKernel() => _kernel;
}
