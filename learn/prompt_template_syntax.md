# Semantic Kernel Prompt Template Syntax - Complete Reference

**Last Updated**: December 7, 2025

---

## 📚 Overview

Semantic Kernel supports multiple templating systems for creating dynamic, powerful prompts. This document covers all template syntax options, variables, control flow, function calling, and state management.

---

## 🎯 Template Systems in Semantic Kernel

### 1. Basic Template Syntax (Simple Variable Substitution)

The simplest form - just variable replacement:

```csharp
var template = "Translate {{$text}} to {{$language}}";
var function = kernel.CreateFunctionFromPrompt(template);

await kernel.InvokeAsync(function, new() {
    ["text"] = "Hello",
    ["language"] = "Spanish"
});
```

**Syntax:**
- `{{$variableName}}` - Input variable
- Variables are passed as `KernelArguments` at invocation time
- No control flow, no internal state

---

### 2. Handlebars Template Syntax (Advanced)

Full-featured templating with control flow, loops, conditionals, and internal variables.

```csharp
var template = @"
{{#each cities}}
  {{set 'weather' (WeatherPlugin-GetWeather city=this)}}
  City: {{this}}, Weather: {{weather}}
{{/each}}
";
```

**Key Features:**
- ✅ Internal variables with `{{set}}`
- ✅ Conditionals: `{{#if}}`, `{{#unless}}`, `{{else}}`
- ✅ Loops: `{{#each}}`, `{{#with}}`
- ✅ Function calls
- ✅ Helpers and custom logic

---

### 3. Liquid Template Syntax (Alternative)

Another templating option (less common in SK):

```liquid
{% for city in cities %}
  Weather in {{ city }}: {{ WeatherPlugin.GetWeather city: city }}
{% endfor %}
```

---

## 🔤 Variable Types and Scoping

### External Variables (Input Parameters)

Passed from outside when invoking the function:

```csharp
// In template
"Process {{$input}} with settings {{$mode}}"

// At invocation
await kernel.InvokeAsync(function, new KernelArguments {
    ["input"] = "user data",
    ["mode"] = "fast"
});
```

**Key Points:**
- Prefixed with `$`: `{{$varName}}`
- **Scope**: Single invocation only
- **Persistence**: NOT persisted across requests
- **Source**: Provided by caller

### Internal Variables (Handlebars Only)

Created and managed within the template:

```handlebars
{{set "result" (MyPlugin-ProcessData input=$input)}}
{{set "summary" (TextPlugin-Summarize text=result maxLength=100)}}

Final: {{summary}}
```

**Key Points:**
- Created with `{{set "name" value}}`
- **Scope**: Within the current template execution
- **Persistence**: NOT persisted across requests
- **Source**: Created during execution

### Context Variables (ChatHistory)

For conversational context that persists:

```csharp
var chatHistory = new ChatHistory();
chatHistory.AddUserMessage("Remember my name is Alice");
chatHistory.AddAssistantMessage("Got it, Alice!");
chatHistory.AddUserMessage("What's my name?");

// ChatHistory persists across multiple turns
var result = await chatService.GetChatMessageContentAsync(
    chatHistory,
    settings,
    kernel
);
```

**Key Points:**
- Managed via `ChatHistory` object
- **Scope**: Entire conversation
- **Persistence**: YES - across multiple AI requests
- **Source**: Accumulated during conversation

---

## 🔧 Handlebars Template Syntax Deep Dive

### Variable Assignment

```handlebars
{{!-- Set from function result --}}
{{set "weather" (WeatherPlugin-GetWeather city="Seattle")}}

{{!-- Set from input variable --}}
{{set "userCity" $location}}

{{!-- Set from literal --}}
{{set "count" 5}}

{{!-- Set from another variable --}}
{{set "backup" weather}}
```

### Conditionals

```handlebars
{{#if $includeDetails}}
  Detailed information:
  {{set "details" (GetDetails id=$itemId)}}
  {{details}}
{{else}}
  Summary only
{{/if}}

{{!-- Check variable existence --}}
{{#if weather}}
  Weather: {{weather}}
{{else}}
  No weather data available
{{/if}}

{{!-- Unless (inverse if) --}}
{{#unless $skipValidation}}
  {{set "valid" (Validate data=$input)}}
{{/unless}}
```

### Loops

```handlebars
{{!-- Loop over array --}}
{{#each $cities}}
  Processing: {{this}}
  {{set "info" (GetCityInfo city=this)}}
  Result: {{info}}
{{/each}}

{{!-- With index --}}
{{#each $items}}
  Item {{@index}}: {{this}}
{{/each}}

{{!-- Loop over object properties --}}
{{#each $userData}}
  {{@key}}: {{this}}
{{/each}}
```

### Function Calls

```handlebars
{{!-- Basic function call --}}
{{MyPlugin-MyFunction param1="value" param2=$variable}}

{{!-- Store result --}}
{{set "result" (MyPlugin-MyFunction param="value")}}

{{!-- Chain functions --}}
{{set "step1" (Plugin1-Process input=$data)}}
{{set "step2" (Plugin2-Transform data=step1)}}
{{set "step3" (Plugin3-Finalize result=step2)}}

{{!-- Pass variables as parameters --}}
{{set "output" (Process input=step1 mode=$mode threshold=step2)}}
```

### Comments

```handlebars
{{!-- This is a comment --}}
{{! Also a comment }}

{{!-- 
Multi-line
comment
--}}
```

### Built-in Helpers

```handlebars
{{!-- JSON helpers --}}
{{json variable}}  <!-- Serialize to JSON -->
{{json variable.property}}  <!-- Access nested property -->

{{!-- String helpers --}}
{{concat "Hello" " " $name}}

{{!-- Comparison --}}
{{#if (eq status "active")}}
  Status is active
{{/if}}

{{#if (gt count 10)}}
  Count is greater than 10
{{/if}}
```

---

## 🔄 Variable Persistence: What Survives?

### ❌ NOT Persisted Across Requests

1. **Template internal variables** (`{{set}}`)
   ```handlebars
   {{set "temp" "value"}}  <!-- Lost after template execution -->
   ```

2. **Input parameters** (`{{$var}}`)
   ```csharp
   new KernelArguments { ["var"] = "value" }  // Single use
   ```

### ✅ Persisted Across Requests

1. **ChatHistory** (Conversation memory)
   ```csharp
   var chatHistory = new ChatHistory();
   chatHistory.AddUserMessage("My favorite color is blue");
   // ... later ...
   chatHistory.AddUserMessage("What's my favorite color?");
   // AI can reference previous messages
   ```

2. **Semantic Memory** (Vector embeddings)
   ```csharp
   var memoryStore = new VolatileMemoryStore();
   await memoryStore.SaveInformationAsync(
       "facts",
       "User prefers Python",
       "user_001"
   );
   // ... later, even different session ...
   var results = await memoryStore.SearchAsync("facts", "programming preference");
   ```

3. **Kernel State** (Custom plugins with state)
   ```csharp
   public class StatefulPlugin
   {
       private Dictionary<string, string> _storage = new();
       
       [KernelFunction]
       public void Remember(string key, string value)
       {
           _storage[key] = value;  // Persists as long as kernel lives
       }
       
       [KernelFunction]
       public string Recall(string key)
       {
           return _storage.GetValueOrDefault(key, "Not found");
       }
   }
   ```

---

## 📝 Complete Template Examples

### Example 1: Simple Substitution (Basic Syntax)

```csharp
var template = @"
You are a {{$role}}.
Task: {{$task}}
Context: {{$context}}
";

var function = kernel.CreateFunctionFromPrompt(template);

await kernel.InvokeAsync(function, new KernelArguments {
    ["role"] = "helpful assistant",
    ["task"] = "summarize the article",
    ["context"] = "scientific paper"
});
```

### Example 2: Conditional Logic (Handlebars)

```handlebars
{{#if $verbose}}
  Detailed analysis:
  {{set "analysis" (AnalyzePlugin-DeepAnalysis data=$input)}}
  
  Summary: {{json analysis.summary}}
  Details: {{json analysis.details}}
  Confidence: {{json analysis.confidence}}
{{else}}
  Quick summary:
  {{set "summary" (AnalyzePlugin-QuickSummary data=$input)}}
  {{summary}}
{{/if}}
```

### Example 3: Multi-Step Pipeline (Handlebars)

```handlebars
{{!-- Step 1: Fetch data --}}
{{set "rawData" (DataPlugin-Fetch source=$dataSource)}}

{{!-- Step 2: Validate --}}
{{#if rawData}}
  {{set "validated" (ValidatePlugin-Check data=rawData)}}
  
  {{#if validated.isValid}}
    {{!-- Step 3: Process --}}
    {{set "processed" (ProcessPlugin-Transform data=rawData)}}
    
    {{!-- Step 4: Format output --}}
    {{set "output" (FormatPlugin-ToJSON data=processed)}}
    
    Result:
    {{output}}
  {{else}}
    Error: Data validation failed
    Reason: {{validated.error}}
  {{/if}}
{{else}}
  Error: Could not fetch data from {{$dataSource}}
{{/if}}
```

### Example 4: Loop with Aggregation (Handlebars)

```handlebars
Processing {{$count}} items:

{{#each $items}}
  Item {{@index}}:
  {{set "result" (ProcessPlugin-Handle item=this mode=$processingMode)}}
  - Status: {{result.status}}
  - Output: {{result.data}}
  
{{/each}}

{{!-- Generate summary --}}
{{set "summary" (SummaryPlugin-Aggregate items=$items)}}

Final Summary:
{{summary}}
```

### Example 5: Conversational with Memory (ChatHistory)

```csharp
// Persistent conversation
var chatHistory = new ChatHistory("You are a helpful assistant with memory");

// Turn 1
chatHistory.AddUserMessage("My name is Alice and I love Python");
var response1 = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
chatHistory.AddAssistantMessage(response1.Content);

// Turn 2 (references Turn 1)
chatHistory.AddUserMessage("What programming language do I prefer?");
var response2 = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
// AI responds: "You love Python!"
chatHistory.AddAssistantMessage(response2.Content);

// Turn 3 (references Turn 1)
chatHistory.AddUserMessage("What's my name?");
var response3 = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
// AI responds: "Your name is Alice!"
```

---

## 🎨 Best Practices

### ✅ DO

1. **Use descriptive variable names**
   ```handlebars
   {{set "userProfileData" (...)}}  <!-- Good -->
   {{set "x" (...)}}  <!-- Bad -->
   ```

2. **Handle missing data gracefully**
   ```handlebars
   {{#if weatherData}}
     Temperature: {{weatherData.temp}}
   {{else}}
     Weather data unavailable
   {{/if}}
   ```

3. **Use ChatHistory for conversational state**
   ```csharp
   // Persist conversation context
   var chatHistory = new ChatHistory();
   // Keep adding to it across turns
   ```

4. **Comment complex templates**
   ```handlebars
   {{!-- Fetch and validate user data --}}
   {{set "user" (UserPlugin-Get id=$userId)}}
   ```

5. **Break complex logic into smaller functions**
   ```csharp
   // Instead of huge template, create focused functions
   [KernelFunction]
   public string ProcessStep1(...) { }
   
   [KernelFunction]
   public string ProcessStep2(...) { }
   ```

### ❌ DON'T

1. **Don't expect template variables to persist**
   ```handlebars
   {{set "temp" "value"}}
   <!-- This is gone after template execution! -->
   ```

2. **Don't mix template systems**
   ```handlebars
   <!-- Don't do this -->
   {{$variable}} mixed with {% liquid syntax %}
   ```

3. **Don't create deeply nested conditionals**
   ```handlebars
   <!-- Too complex - refactor into functions -->
   {{#if a}}
     {{#if b}}
       {{#if c}}
         {{#if d}}
   ```

4. **Don't forget to handle errors**
   ```handlebars
   <!-- Always check function results -->
   {{set "data" (FetchPlugin-Get url=$url)}}
   {{#if data}}
     <!-- Use data -->
   {{else}}
     <!-- Handle failure -->
   {{/if}}
   ```

---

## 🔗 Official Resources

### Documentation

- **Semantic Kernel Prompts**: https://learn.microsoft.com/en-us/semantic-kernel/prompts/
- **Template Syntax**: https://learn.microsoft.com/en-us/semantic-kernel/prompts/configure-prompts
- **Handlebars Planner**: https://learn.microsoft.com/en-us/semantic-kernel/ai-orchestration/planners/handlebars-planner
- **Function Calling**: https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/

### Handlebars Language

- **Handlebars.js Official Guide**: https://handlebarsjs.com/guide/
- **Built-in Helpers**: https://handlebarsjs.com/guide/builtin-helpers.html
- **Expressions**: https://handlebarsjs.com/guide/expressions.html

### GitHub Examples

- **SK Prompt Examples**: https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/Concepts/PromptTemplates
- **Handlebars Planning Samples**: https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/Concepts/Planning
- **ChatCompletion Examples**: https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/Concepts/ChatCompletion

### Video Tutorials

- **Semantic Kernel Prompts Playlist**: https://www.youtube.com/results?search_query=semantic+kernel+prompt+templates
- **Handlebars Tutorial**: https://www.youtube.com/results?search_query=handlebars+template+tutorial

### Articles & Blogs

- **Microsoft DevBlogs - Semantic Kernel**: https://devblogs.microsoft.com/semantic-kernel/
- **Prompt Engineering Guide**: https://www.promptingguide.ai/
- **OpenAI Function Calling**: https://platform.openai.com/docs/guides/function-calling

---

## 🧪 Testing Template Syntax

### Quick Test: Basic Substitution

```csharp
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-4", apiKey)
    .Build();

var template = "Hello {{$name}}, you are {{$age}} years old";
var function = kernel.CreateFunctionFromPrompt(template);

var result = await kernel.InvokeAsync(function, new KernelArguments {
    ["name"] = "Alice",
    ["age"] = "30"
});

Console.WriteLine(result);  // "Hello Alice, you are 30 years old"
```

### Quick Test: Handlebars with Set

```csharp
kernel.ImportPluginFromType<MathPlugin>();

var template = @"
{{set 'sum' (MathPlugin-Add a=5 b=3)}}
{{set 'double' (MathPlugin-Multiply a=sum b=2)}}
Result: {{double}}
";

var function = kernel.CreateFunctionFromPrompt(
    template,
    new HandlebarsPromptTemplateFactory()
);

var result = await kernel.InvokeAsync(function);
Console.WriteLine(result);  // "Result: 16"
```

### Quick Test: ChatHistory Persistence

```csharp
var chatHistory = new ChatHistory();

// Turn 1
chatHistory.AddUserMessage("Remember: my favorite color is blue");
var r1 = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
chatHistory.AddAssistantMessage(r1.Content);

// Turn 2 - References Turn 1
chatHistory.AddUserMessage("What's my favorite color?");
var r2 = await chatService.GetChatMessageContentAsync(chatHistory, settings, kernel);
// AI can recall: "Your favorite color is blue"
```

---

## 📊 Quick Reference Table

| Feature | Basic Syntax | Handlebars | Persistence |
|---------|-------------|------------|-------------|
| **Input Variables** | `{{$var}}` | `{{$var}}` | ❌ No |
| **Internal Variables** | ❌ No | `{{set "var" value}}` | ❌ No |
| **Conditionals** | ❌ No | `{{#if}}` | N/A |
| **Loops** | ❌ No | `{{#each}}` | N/A |
| **Function Calls** | Via `{{plugin.func}}` | `{{Plugin-Function param=value}}` | N/A |
| **Comments** | ❌ No | `{{!-- comment --}}` | N/A |
| **ChatHistory** | ✅ Via API | ✅ Via API | ✅ Yes |
| **Semantic Memory** | ✅ Via API | ✅ Via API | ✅ Yes |

---

## 🔌 Plugin Function Metadata & Advanced Configuration

### Function Descriptions Are Prompts Too!

When you add descriptions to kernel functions, you're essentially creating **prompts for the AI** to understand what each function does. This metadata is crucial for:
- **Planners** - AI uses descriptions to select appropriate functions
- **Function Calling** - AI decides which function to invoke
- **Auto-completion** - IDEs and tools can provide better suggestions

### Basic Inline Metadata

```csharp
public class MyPlugin
{
    [KernelFunction]
    [Description("Calculates the square root of a number with high precision")]
    public double SquareRoot(
        [Description("The number to find the square root of")] double number)
    {
        return Math.Sqrt(number);
    }
}
```

**What AI sees:**
```
Function: MyPlugin-SquareRoot
Description: Calculates the square root of a number with high precision
Parameters:
  - number (double): The number to find the square root of
```

### Loading Functions from Configuration Files

#### Option 1: Prompt YAML/JSON Configuration

Store function definitions in configuration files:

**config/plugins/MathPlugin/SquareRoot/config.json**
```json
{
  "schema": 1,
  "description": "Calculates the square root of a number",
  "execution_settings": {
    "max_tokens": 100,
    "temperature": 0.0
  },
  "input_variables": [
    {
      "name": "number",
      "description": "The number to calculate square root of",
      "default": "0"
    }
  ]
}
```

**config/plugins/MathPlugin/SquareRoot/skprompt.txt**
```
Calculate the square root of {{$number}}
```

**Load from file:**
```csharp
// Load plugin from directory structure
var pluginDirectory = Path.Combine(Directory.GetCurrentDirectory(), "config", "plugins");
var mathPlugin = kernel.ImportPluginFromPromptDirectory(
    Path.Combine(pluginDirectory, "MathPlugin")
);

// Use it
var result = await kernel.InvokeAsync(mathPlugin["SquareRoot"], 
    new() { ["number"] = "16" });
```

#### Option 2: YAML Plugin Definitions

**plugins/weather-plugin.yaml**
```yaml
name: WeatherPlugin
description: Provides weather information for cities
functions:
  - name: GetWeather
    description: Gets current weather for a specified city
    parameters:
      - name: city
        description: The city name
        type: string
        required: true
      - name: units
        description: Temperature units (celsius or fahrenheit)
        type: string
        default: celsius
        required: false
```

**Load from YAML:**
```csharp
using YamlDotNet.Serialization;

var yaml = File.ReadAllText("plugins/weather-plugin.yaml");
var deserializer = new DeserializerBuilder().Build();
var config = deserializer.Deserialize<PluginConfig>(yaml);

// Create plugin from config
var plugin = CreatePluginFromConfig(config);
kernel.ImportPlugin(plugin);
```

### Dynamic Function Registration

Register functions with dynamic metadata at runtime:

```csharp
// Create function with custom metadata
var function = KernelFunctionFactory.CreateFromMethod(
    method: (string city, string units = "celsius") => 
    {
        return $"Weather in {city}: 72°{(units == "celsius" ? "C" : "F")}";
    },
    functionName: "GetWeather",
    description: "Gets current weather for a city. Use this when user asks about weather conditions."
);

// Add to kernel with metadata
kernel.ImportPluginFromFunctions("WeatherPlugin", new[] { function });
```

### Advanced Metadata Customization

#### Rich Parameter Descriptions

```csharp
public class DataPlugin
{
    [KernelFunction]
    [Description(@"Searches a database for records matching criteria.
Use this when:
- User asks to find or search for data
- User mentions filtering or querying
- User wants to look up information

Examples:
- 'Find all users from California'
- 'Search for orders over $100'
- 'Look up customer by email'")]
    public async Task<string> SearchDatabase(
        [Description(@"The search query in natural language.
Examples: 
- 'users in California'
- 'orders greater than 100 dollars'
- 'customer with email john@example.com'")] 
        string query,
        
        [Description("Maximum number of results to return (1-100)")] 
        int limit = 10,
        
        [Description("Sort order: 'asc' for ascending, 'desc' for descending")] 
        string sortOrder = "asc")
    {
        // Implementation
        return await SearchAsync(query, limit, sortOrder);
    }
}
```

#### Function Metadata Attributes

```csharp
using Microsoft.SemanticKernel;
using System.ComponentModel;

public class AdvancedPlugin
{
    [KernelFunction("calculate_total")]
    [Description("Calculates the total cost including tax and shipping")]
    [return: Description("The total cost as a formatted currency string")]
    public string CalculateTotal(
        [Description("Base price before tax")] 
        [DefaultValue(0.0)]
        double price,
        
        [Description("Tax rate as decimal (e.g., 0.08 for 8%)")] 
        [DefaultValue(0.0)]
        double taxRate = 0.0,
        
        [Description("Shipping cost")] 
        [DefaultValue(0.0)]
        double shipping = 0.0)
    {
        var total = price + (price * taxRate) + shipping;
        return $"${total:F2}";
    }
}
```

### Prompt Templates in Function Descriptions

You can use structured formats in descriptions to guide AI behavior:

```csharp
[KernelFunction]
[Description(@"
PRIMARY PURPOSE: Send email notifications to users
WHEN TO USE: User explicitly requests to send email or notify someone
REQUIRED: recipient email address
OPTIONAL: subject, body, attachments

DECISION CRITERIA:
- USE if: user says 'send email', 'notify', 'email them'
- DON'T USE if: user just wants to draft or compose (use DraftEmail instead)

EXAMPLES:
✓ 'Send an email to john@example.com about the meeting'
✓ 'Notify the team about the update'
✗ 'Help me write an email' (use DraftEmail)
✗ 'What should I say in an email?' (use DraftEmail)
")]
public async Task SendEmail(
    [Description("Recipient email address (must be valid format)")] string to,
    [Description("Email subject line")] string subject,
    [Description("Email body content")] string body)
{
    // Implementation
}
```

### Loading from External Sources

#### Database-Driven Function Metadata

```csharp
public class DynamicPluginLoader
{
    private readonly IDatabase _db;
    
    public async Task<KernelPlugin> LoadPluginFromDatabaseAsync(string pluginName)
    {
        // Fetch metadata from database
        var functionConfigs = await _db.GetFunctionsForPlugin(pluginName);
        
        var functions = new List<KernelFunction>();
        
        foreach (var config in functionConfigs)
        {
            var function = KernelFunctionFactory.CreateFromMethod(
                method: CreateDynamicMethod(config),
                functionName: config.Name,
                description: config.Description,
                parameters: config.Parameters.Select(p => new KernelParameterMetadata(p.Name)
                {
                    Description = p.Description,
                    DefaultValue = p.DefaultValue,
                    IsRequired = p.IsRequired
                }).ToList()
            );
            
            functions.Add(function);
        }
        
        return KernelPluginFactory.CreateFromFunctions(pluginName, functions);
    }
}
```

#### Remote API-Defined Functions

```csharp
// Load OpenAPI specification
var openApiSpec = await File.ReadAllTextAsync("weather-api-spec.json");

// Import as plugin
var plugin = await kernel.ImportPluginFromOpenApiAsync(
    pluginName: "WeatherAPI",
    uri: new Uri("https://api.weather.com/openapi.json"),
    executionParameters: new OpenApiFunctionExecutionParameters
    {
        EnableDynamicPayload = true,
        EnablePayloadNamespacing = false
    }
);

// SK automatically creates functions from API endpoints with descriptions from OpenAPI spec
```

### Semantic Function Configuration Files

For semantic (prompt-based) functions, use directory structure:

```
plugins/
  EmailPlugin/
    SendEmail/
      config.json          # Function configuration
      skprompt.txt         # Prompt template
    DraftEmail/
      config.json
      skprompt.txt
```

**config.json:**
```json
{
  "schema": 1,
  "type": "completion",
  "description": "Drafts a professional email based on key points",
  "execution_settings": {
    "default": {
      "max_tokens": 500,
      "temperature": 0.7,
      "top_p": 0.9
    }
  },
  "input_variables": [
    {
      "name": "recipient",
      "description": "Who the email is addressed to",
      "required": true
    },
    {
      "name": "subject",
      "description": "Email subject line",
      "required": true
    },
    {
      "name": "keyPoints",
      "description": "Main points to include in the email",
      "required": true
    },
    {
      "name": "tone",
      "description": "Tone of the email (formal, casual, friendly)",
      "default": "professional"
    }
  ]
}
```

**skprompt.txt:**
```
Draft a {{$tone}} email to {{$recipient}} with the subject "{{$subject}}".

Include these key points:
{{$keyPoints}}

Make it clear, concise, and appropriate for business communication.
```

**Load and use:**
```csharp
var pluginDir = Path.Combine(Directory.GetCurrentDirectory(), "plugins", "EmailPlugin");
var emailPlugin = kernel.ImportPluginFromPromptDirectory(pluginDir);

var draft = await kernel.InvokeAsync(emailPlugin["DraftEmail"], new()
{
    ["recipient"] = "team@company.com",
    ["subject"] = "Q4 Planning Meeting",
    ["keyPoints"] = "- Meeting on Friday\n- Review budget\n- Discuss priorities",
    ["tone"] = "friendly"
});
```

### Best Practices for Function Metadata

#### ✅ DO:

1. **Be specific and action-oriented**
   ```csharp
   ✓ "Searches products by name, category, or SKU and returns matching items"
   ✗ "Does product stuff"
   ```

2. **Include usage examples**
   ```csharp
   [Description(@"Converts currency amounts.
Examples: 
- 'Convert 100 USD to EUR'
- 'How much is 50 GBP in JPY?'")]
   ```

3. **Specify when to use vs not use**
   ```csharp
   [Description(@"USE: When user wants to save/persist data
DON'T USE: For temporary calculations or read-only queries")]
   ```

4. **Document parameter constraints**
   ```csharp
   [Description("Temperature in celsius (-273.15 to 1000)")]
   ```

5. **Use structured format for complex functions**
   ```csharp
   [Description(@"
PURPOSE: Query database
INPUTS: SQL query, timeout
OUTPUTS: JSON result set
SIDE EFFECTS: Read-only, no data modification")]
   ```

#### ❌ DON'T:

1. **Don't be vague**
   ```csharp
   ✗ "Handles data"
   ✗ "Processes information"
   ```

2. **Don't forget parameter descriptions**
   ```csharp
   ✗ public void Process(string data)  // No description!
   ```

3. **Don't use technical jargon AI might not understand**
   ```csharp
   ✗ "Performs SIMD-optimized vectorization" 
   ✓ "Processes large arrays efficiently"
   ```

4. **Don't describe implementation details**
   ```csharp
   ✗ "Uses Entity Framework to query SQL Server"
   ✓ "Searches database for matching records"
   ```

### Testing Function Metadata

Verify AI can understand your function descriptions:

```csharp
// Print all function metadata
foreach (var plugin in kernel.Plugins)
{
    Console.WriteLine($"\nPlugin: {plugin.Name}");
    foreach (var function in plugin)
    {
        Console.WriteLine($"  Function: {function.Name}");
        Console.WriteLine($"  Description: {function.Description}");
        foreach (var param in function.Metadata.Parameters)
        {
            Console.WriteLine($"    - {param.Name}: {param.Description}");
        }
    }
}
```

### Resources

- **OpenAPI Plugin Import**: https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/using-the-openapi-plugin
- **Plugin Configuration**: https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/
- **Function Metadata**: https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/adding-native-plugins
- **Prompt Template Config**: https://learn.microsoft.com/en-us/semantic-kernel/prompts/configure-prompts

---

## 🎯 Key Takeaways

1. **Two main template systems**: Basic (simple substitution) and Handlebars (full-featured)
2. **Template variables are NOT persisted** - they're only for the current execution
3. **Use ChatHistory** for conversational memory across requests
4. **Use Semantic Memory (vectors)** for long-term knowledge storage
5. **Handlebars `{{set}}`** creates internal variables for multi-step pipelines
6. **Input variables** `{{$var}}` are passed at invocation time
7. **Control flow** (if/each) only available in Handlebars
8. **Function calls** in Handlebars: `{{Plugin-Function param=value}}`
9. **State management** requires explicit ChatHistory or Memory stores
10. **Always handle missing data** with conditionals
11. **Function descriptions are prompts** - AI uses them to understand when/how to call functions
12. **Load from files** - Store plugin configs in JSON/YAML for maintainability
13. **Rich metadata helps planners** - Detailed descriptions improve AI function selection
14. **OpenAPI import** - Automatically create plugins from API specifications

---

**Questions or need clarification?** Review the official documentation links above or explore the GitHub samples for more examples!
