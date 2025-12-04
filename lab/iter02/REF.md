# Translation Plugin Lab - Reference & Hints

This document provides hints and guidance for completing the Translation Plugin Lab exercises.

---

## TODO 1 – Create Translation Semantic Function

<details>
<summary>💡 Hints</summary>

### Key Concepts
- Semantic functions are created using `kernel.CreateFunctionFromPrompt()`
- Template variables use the syntax `{{$variableName}}`
- Execution settings control AI behavior (temperature, max tokens, etc.)

### What You Need
1. A clear prompt template with instructions for the AI
2. Two template variables: `{{$text}}` and `{{$targetLanguage}}`
3. Execution settings with Temperature=0.3 and MaxTokens=500
4. Function metadata (name and description)

### Prompt Engineering Tips
- Be specific: Tell the AI exactly what to do
- Include context: Mention preserving meaning and tone
- Use clear variable names that indicate their purpose
- Low temperature (0.3) ensures consistent, accurate translations

### API Structure
```csharp
kernel.CreateFunctionFromPrompt(
    promptTemplate: "...",
    executionSettings: new OpenAIPromptExecutionSettings { ... },
    functionName: "...",
    description: "..."
)
```

### Common Pitfalls
- Forgetting to use `{{$variableName}}` syntax for template variables
- Setting temperature too high (causes inconsistent translations)
- Not providing clear instructions in the prompt
- Missing function name or description

</details>

---

## TODO 2 – Register as Plugin and Enable Auto Function Calling

<details>
<summary>💡 Hints</summary>

### Key Concepts
- Plugins are collections of functions that the AI can automatically call
- `ToolCallBehavior.AutoInvokeKernelFunctions` enables automatic function selection
- Chat completion service processes messages with function calling capabilities

### What You Need
1. Register the translation function as "TranslationPlugin"
2. Get the chat completion service from kernel
3. Configure execution settings with auto function calling
4. Create chat history and get response

### Registration Pattern
```csharp
kernel.Plugins.AddFromFunctions("PluginName", [function1, function2, ...])
```

### Auto Function Calling Setup
- Create `OpenAIPromptExecutionSettings` with `ToolCallBehavior.AutoInvokeKernelFunctions`
- Get chat completion service: `kernel.GetRequiredService<IChatCompletionService>()`
- Build chat history with user message
- Call `GetChatMessageContentAsync()` with history and settings

### How It Works
1. User sends natural language request (e.g., "Translate 'hello' to Spanish")
2. AI determines which function to call and with what arguments
3. Function executes automatically
4. AI incorporates the result into its response

### Common Pitfalls
- Not using the exact plugin name "TranslationPlugin"
- Forgetting to enable `AutoInvokeKernelFunctions`
- Not passing execution settings to the chat completion call
- Missing the chat history creation step

</details>

---

<details>
<summary>🔑 Reference Solution</summary>

```csharp
public KernelFunction CreateTranslationFunction()
{
    var promptTemplate = @"Translate the following text to {{$targetLanguage}}.
Preserve the original meaning, tone, and style in your translation.
Provide only the translated text without any additional explanation.

Text to translate: {{$text}}";

    return _kernel.CreateFunctionFromPrompt(
        promptTemplate: promptTemplate,
        executionSettings: new OpenAIPromptExecutionSettings
        {
            Temperature = 0.3,
            MaxTokens = 500
        },
        functionName: "Translate",
        description: "Translates text to the specified target language"
    );
}
```

### Key Elements:
- **Prompt Template**: Clear instructions with two template variables `{{$text}}` and `{{$targetLanguage}}`
- **Execution Settings**: Temperature set to 0.3 for consistent translations, MaxTokens at 500
- **Metadata**: Function name "Translate" with descriptive text
- **Instructions**: Tells AI to preserve meaning/tone and provide only the translation

</details>

---

## TODO 2 – Register as Plugin and Enable Auto Function Calling

<details>
<summary>💡 Hints</summary>

### Key Concepts
- Function chaining allows calling functions within prompt templates
- Syntax: `{{PluginName.FunctionName arg1=value1 arg2=value2}}`
- Multiple function calls can be combined in a single template

### What You Need
1. A semantic function that takes `{{$text}}` as input
2. Three calls to `TranslationPlugin.Translate` within the template
3. Target languages: Spanish, French, and German
4. Proper formatting to display all translations clearly

### Function Call Syntax in Templates
```
{{TranslationPlugin.Translate text=$text targetLanguage="Spanish"}}
```

### Template Structure
Your prompt template should:
- Accept `{{$text}}` as the input variable
- Call `TranslationPlugin.Translate` three times (once per language)
- Format output with language labels
- Preserve the structure across all languages

### Execution Settings
- Temperature: 0.3 (for consistency)
- MaxTokens: 1000 (enough for three translations)

### Common Pitfalls
- Incorrect function call syntax (missing plugin name, wrong argument format)
- Not using the registered plugin name "TranslationPlugin"
- Forgetting to pass template variables correctly
- Not ensuring the translation function is registered before creating this function

### Example Output Format
```
Spanish: [translation]
French: [translation]
German: [translation]
```

</details>

---

<details>
<summary>🔑 Reference Solution</summary>

```csharp
public KernelFunction CreateMultiLanguageFunction()
{
    var promptTemplate = @"Translate the following text to multiple languages and format the output clearly:

Original text: {{$text}}

Spanish: {{TranslationPlugin.Translate text=$text targetLanguage=""Spanish""}}

French: {{TranslationPlugin.Translate text=$text targetLanguage=""French""}}

German: {{TranslationPlugin.Translate text=$text targetLanguage=""German""}}";

    return _kernel.CreateFunctionFromPrompt(
        promptTemplate: promptTemplate,
        executionSettings: new OpenAIPromptExecutionSettings
        {
            Temperature = 0.3,
            MaxTokens = 1000
        },
        functionName: "TranslateToMultiple"
    );
}
```

### Key Elements:
- **Function Chaining Syntax**: `{{TranslationPlugin.Translate text=$text targetLanguage="Spanish"}}`
- **Template Variable**: `{{$text}}` passed to each function call
- **Three Languages**: Spanish, French, and German as required
- **Formatted Output**: Labels for each language in the prompt template
- **Execution Settings**: Temperature 0.3 and MaxTokens 1000

### How Function Chaining Works:
1. Template contains three calls to `TranslationPlugin.Translate`
2. Each call specifies the same `$text` variable with different target languages
3. Functions execute during template rendering
4. Results are inserted into the template at the call location
5. Final output contains all three translations formatted with labels

### Note:
The TranslationPlugin must already be registered in the kernel before this function is created. The test harness handles this by calling `CreateTranslationFunction()` and registering it before calling `CreateMultiLanguageFunction()`.

</details>

---

## Additional Resources

### Semantic Kernel Documentation
- [Creating Semantic Functions](https://learn.microsoft.com/semantic-kernel/prompts/your-first-prompt)
- [Template Variables](https://learn.microsoft.com/semantic-kernel/prompts/templatizing-prompts)
- [Plugin Registration](https://learn.microsoft.com/semantic-kernel/agents/plugins/)
- [Function Calling](https://learn.microsoft.com/semantic-kernel/concepts/ai-services/chat-completion/function-calling/)

### Debugging Tips
1. **Test incrementally**: Complete and test each TODO before moving to the next
2. **Check variable names**: Ensure template variables match exactly (`{{$text}}`, not `{{text}}`)
3. **Verify plugin registration**: Plugin must be registered before function chaining works
4. **Review error messages**: Test failures reference specific README sections for guidance
5. **Use console output**: Add temporary `Console.WriteLine()` statements to debug

### Testing Your Implementation
Run the test harness with:
```bash
dotnet run
```

Expected output when all TODOs are complete:
```
Test 1: Translation Function Creation
PASS - Translation: [Spanish translation]

Test 2: Plugin Registration and Auto Function Calling
PASS - Auto translation response: [AI response with translation]

Test 3: Multi-Language Output with Function Chaining
PASS - Multi-language output:
[Formatted translations in Spanish, French, and German]

Results: 3/3 PASSED
```

</details>
