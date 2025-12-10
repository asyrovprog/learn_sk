# Reference Guide - Lab 04

## TODO N1 Hints – Calendar Plugin Functions

**Concept**: Native functions are C# methods decorated with `[KernelFunction]` that can be discovered and invoked by the planner.

**Key Points**:
- Add `[KernelFunction]` attribute above each method
- Add `[Description("...")]` to help the AI understand the function's purpose
- Return simple string messages that the planner can interpret
- For `GetTodaysSchedule()`: Return a multi-line string with 2-3 meetings
- For `FindNextAvailableSlot()`: Consider parameter `durationMinutes` and return a time

**Example Structure**:
```csharp
[KernelFunction]
[Description("Gets today's meeting schedule")]
public string GetTodaysSchedule()
{
    // Return formatted schedule string
}
```

---

## TODO N2 Hints – Email Plugin Functions

**Concept**: Simulation functions that return status messages confirming actions.

**Key Points**:
- These don't send real emails - just return confirmation strings
- Include all relevant information in the confirmation message
- Use descriptive function descriptions for the planner
- Parameter descriptions help the planner pass correct arguments

**Example Structure**:
```csharp
[KernelFunction]
[Description("Sends a meeting reminder email")]
public string SendMeetingReminder(
    [Description("Email recipient")] string recipient,
    // ... other parameters
{
    return $"✓ Reminder sent to {recipient}...";
}
```

---

## TODO N3 Hints – FunctionCallingStepwisePlanner

**Concept**: The planner analyzes goals, discovers available functions, and orchestrates them automatically.

**Key Points**:
- Import: `using Microsoft.SemanticKernel.Planning;`
- FunctionCallingStepwisePlanner is deprecated - use HandlebarsPlanner instead
- Create planner with options (MaxIterations, MaxTokens)
- The planner will automatically call your plugin functions based on the goal
- Execute the plan and return the result

**Example Structure**:
```csharp
var planner = new HandlebarsPlanner(new HandlebarsPlannerOptions
{
    // Set options
});

var plan = await planner.CreatePlanAsync(_kernel, goal);
var result = await plan.InvokeAsync(_kernel);
return result.ToString();
```

**Note**: FunctionCallingStepwisePlanner was deprecated. Use HandlebarsPlanner or rely on automatic function calling with `ToolCallBehavior.AutoInvokeKernelFunctions`.

---

## Additional Tips

1. **Test incrementally**: Implement and test TODO N1, then N2, then N3
2. **Check descriptions**: The planner relies on clear function descriptions
3. **Simple returns**: Return human-readable strings, not complex objects
4. **Debug output**: Add Console.WriteLine in your functions to see when they're called

---

## Common Issues

- **"Function not found"**: Check that `[KernelFunction]` attribute is present
- **"Wrong parameters"**: Ensure parameter descriptions are clear
- **"Planner fails"**: Verify all previous TODOs are correctly implemented
- **Compilation errors**: Make sure all using statements are present

---

## Reference Solution

<details>
<summary>Reference Solution (open after completion)</summary>

### Complete Task.cs

```csharp
using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SmartTaskOrchestrator;

/// <summary>
/// Calendar plugin for managing schedule and finding available time slots
/// </summary>
public class CalendarPlugin
{
    [KernelFunction]
    [Description("Gets today's meeting schedule with all appointments")]
    public string GetTodaysSchedule()
    {
        return "9:00 AM - Team Standup\n" +
               "11:00 AM - Code Review Session\n" +
               "2:00 PM - Client Review\n" +
               "4:00 PM - Sprint Planning";
    }

    [KernelFunction]
    [Description("Finds the next available time slot for a meeting of the specified duration in minutes")]
    public string FindNextAvailableSlot(
        [Description("Duration of the meeting in minutes")] int durationMinutes)
    {
        var now = DateTime.Now;
        
        // Simple logic: if it's before 3 PM, suggest today at 5 PM, otherwise tomorrow at 10 AM
        if (now.Hour < 15)
        {
            return "Today at 5:00 PM";
        }
        else
        {
            return "Tomorrow at 10:00 AM";
        }
    }
}

/// <summary>
/// Email plugin for sending notifications and reminders
/// </summary>
public class EmailPlugin
{
    [KernelFunction]
    [Description("Sends a meeting reminder email to a participant")]
    public string SendMeetingReminder(
        [Description("Email address of the recipient")] string recipient,
        [Description("Time of the meeting")] string meetingTime,
        [Description("Topic or subject of the meeting")] string topic)
    {
        return $"✓ Reminder sent to {recipient} for {topic} at {meetingTime}";
    }

    [KernelFunction]
    [Description("Sends the meeting agenda to a recipient via email")]
    public string SendAgendaEmail(
        [Description("Email address of the recipient")] string recipient,
        [Description("The meeting agenda content")] string agenda)
    {
        return $"✓ Agenda sent to {recipient}";
    }
}

/// <summary>
/// Document plugin for creating meeting materials
/// </summary>
public class DocumentPlugin
{
    [KernelFunction]
    [Description("Creates a meeting agenda with the given topic and attendees")]
    public string CreateAgenda(
        [Description("The meeting topic")] string topic,
        [Description("Comma-separated list of attendees")] string attendees)
    {
        return $"📋 MEETING AGENDA\n" +
               $"Topic: {topic}\n" +
               $"Attendees: {attendees}\n" +
               $"Items:\n" +
               $"1. Opening remarks\n" +
               $"2. Main discussion\n" +
               $"3. Action items\n" +
               $"4. Closing";
    }
}

/// <summary>
/// Main orchestrator class that uses planners to coordinate tasks
/// </summary>
public class TaskOrchestrator
{
    private readonly Kernel _kernel;

    public TaskOrchestrator(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<string> ExecuteGoalAsync(string goal)
    {
        // Use automatic function calling instead of deprecated FunctionCallingStepwisePlanner
        var executionSettings = new Microsoft.SemanticKernel.Connectors.OpenAI.OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = Microsoft.SemanticKernel.Connectors.OpenAI.ToolCallBehavior.AutoInvokeKernelFunctions,
            MaxTokens = 4000
        };

        var result = await _kernel.InvokePromptAsync(goal, new(executionSettings));
        return result.ToString();
    }
}
```

**Key Points:**
- All plugin methods use `[KernelFunction]` and `[Description]` attributes
- Descriptions are clear and help the AI understand when to use each function
- Simple string returns make results easy for the planner to interpret
- `ToolCallBehavior.AutoInvokeKernelFunctions` enables automatic function orchestration
- The kernel will automatically chain function calls to achieve the goal

</details>
