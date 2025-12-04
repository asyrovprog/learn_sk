# Multi-Language Translator Plugin Lab

## Overview
Build a translation plugin using Semantic Kernel that demonstrates prompt engineering with semantic functions. You'll create a plugin with multiple semantic functions, use template variables, configure execution settings, and practice automatic function calling.

## TODO 1 – Create Translation Semantic Function

**Objective:** Create a semantic function that translates text to a specified language using proper prompt engineering techniques.

**Instructions:**
1. Create a semantic function using `kernel.CreateFunctionFromPrompt()`
2. Use a clear, specific prompt template that:
   - Takes two template variables: `{{$text}}` and `{{$targetLanguage}}`
   - Instructs the AI to translate accurately
   - Specifies to preserve meaning and tone
3. Set appropriate execution settings:
   - Temperature: 0.3 (low for accuracy)
   - MaxTokens: 500
4. Provide function name: "Translate"
5. Provide description: "Translates text to the specified target language"
6. Return the created function

**Requirements:**
- Use `{{$text}}` for the input text variable
- Use `{{$targetLanguage}}` for the target language variable
- Set temperature to 0.3 for deterministic translation
- Include clear instructions in the prompt about preserving meaning

---

## TODO 2 – Register Functions as Plugin and Enable Auto Function Calling

**Objective:** Register semantic functions as a plugin and configure automatic function calling so the AI can automatically select and invoke the translation function.

**Instructions:**
1. Register the translation function as a plugin:
   - Plugin name: "TranslationPlugin"
   - Use `kernel.Plugins.AddFromFunctions()`
2. Get the chat completion service from the kernel
3. Create execution settings with `ToolCallBehavior.AutoInvokeKernelFunctions` to enable automatic function calling
4. Create a ChatHistory and add the user's message
5. Get the chat response with automatic function calling enabled
6. Return the response content as a string

**Requirements:**
- Plugin must be named "TranslationPlugin"
- Must use `ToolCallBehavior.AutoInvokeKernelFunctions`
- Must properly configure chat completion settings
- Return the final AI response after function execution

---

## TODO 3 – Create Multi-Language Output Function

**Objective:** Create a semantic function that translates input text to multiple languages simultaneously, demonstrating function chaining and output formatting.

**Instructions:**
1. Create a semantic function that:
   - Takes `{{$text}}` as input
   - Translates to Spanish, French, and German
   - Formats output as a structured list with language names
2. Use the translation function created in TODO 1 within the template:
   - Syntax: `{{TranslationPlugin.Translate text=<value> targetLanguage=<value>}}`
3. Set appropriate execution settings:
   - Temperature: 0.3
   - MaxTokens: 1000
4. Provide function name: "TranslateToMultiple"
5. Return the created function

**Requirements:**
- Must translate to exactly: Spanish, French, and German
- Must use the TranslationPlugin.Translate function calls in the template
- Output must be formatted with language labels
- Temperature must be 0.3
