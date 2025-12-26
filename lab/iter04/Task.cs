using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SmartTaskOrchestrator;

/// <summary>
/// Calendar plugin for managing schedule and finding available time slots
/// </summary>
public class CalendarPlugin
{
    // TODO[N1]: Implement Calendar Plugin Functions
    // [YOUR CODE GOES HERE]
    // 
    // Instructions from README:
    // - Implement `GetTodaysSchedule()` method that returns a formatted string of today's meetings
    //   - Return format: "9:00 AM - Team Standup\n2:00 PM - Client Review"
    //   - Include 2-3 sample meetings
    // - Implement `FindNextAvailableSlot(int durationMinutes)` method
    //   - Find the next available time slot of the requested duration
    //   - Return format: "Tomorrow at 10:00 AM" or "Today at 4:00 PM"
    //   - Consider current time and existing meetings
    //
    // Hints:
    // - Use `[KernelFunction]` attribute to make functions discoverable
    // - Use `[Description("...")]` to help the planner understand what each function does
    // - Return simple strings - the planner will interpret them
    [KernelFunction]
    [Description("Returns formatted string of today's meeting. Example: 9:00 AM - Team Standup\n2:00 PM - Client Review")]
    public string GetTodaysSchedule()
    {
        return "9:00 AM - Team Standup\n2:00 PM - Client Review\n4:00 PM - Planning";
    }

    [KernelFunction]
    [Description("Find next available slot for a meeting with the specified duration.")]
    public string FindNextAvailableSlot(
        [Description("Meeting duration in minutes.")] int durationMinutes)
    {
        return "Tomorrow at 9:00 AM";
    }
}

/// <summary>
/// Email plugin for sending notifications and reminders
/// </summary>
public class EmailPlugin
{
    // TODO[N2]: Implement Email Plugin Functions
    // [YOUR CODE GOES HERE]
    //
    // Instructions from README:
    // - Implement `SendMeetingReminder(string recipient, string meetingTime, string topic)` method
    //   - Return confirmation message: "✓ Reminder sent to {recipient} for {topic} at {meetingTime}"
    // - Implement `SendAgendaEmail(string recipient, string agenda)` method
    //   - Return confirmation: "✓ Agenda sent to {recipient}"
    //
    // Hints:
    // - These are simulation functions (no real emails)
    // - Clear descriptions help the planner choose the right function
    // - Return status messages that confirm the action
    [KernelFunction]
    [Description("Sends meeting reminder and returns confirmation message.")]
    public string SendMeetingReminder(
        [Description("Meeting participant email or name")] string recipient, 
        [Description("Meeting time")] string meetingTime, 
        [Description("Meeting topic")] string topic)
    {
        return $"✓ Reminder sent to {recipient} for {topic} at {meetingTime}";
    }

    [KernelFunction]
    [Description("Sends agenda email")]
    public string SendAgendaEmail(
        [Description("Meeting participant email or name")] string recipient, 
        [Description("Meeting agenda")] string agenda)
    {
        return $"✓ Agenda {agenda} sent to {recipient}";
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
        // TODO[N3]: Configure and Use FunctionCallingStepwisePlanner
        // [YOUR CODE GOES HERE]
        //
        // Instructions from README:
        // - Create a `FunctionCallingStepwisePlanner` instance
        // - Configure `FunctionCallingStepwisePlannerOptions` with:
        //   - MaxIterations: 10
        //   - MaxTokens: 4000
        // - Create and execute a plan for the goal: "Schedule a team meeting for tomorrow and send reminders to all participants"
        // - The planner should automatically:
        //   1. Check the schedule using CalendarPlugin
        //   2. Find an available time slot
        //   3. Send reminder emails using EmailPlugin
        // - Capture and return the final result from the planner
        //
        // Hints:
        // - FunctionCallingStepwisePlanner is deprecated - use automatic function calling instead
        // - Configure OpenAIPromptExecutionSettings with ToolCallBehavior.AutoInvokeKernelFunctions
        // - Set MaxTokens to 4000
        // - The kernel will automatically discover and call your plugin functions
        
        var executionSettings = new OpenAIPromptExecutionSettings
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            MaxTokens = 4000
        };

        var result = await _kernel.InvokePromptAsync(goal, new(executionSettings));
        return result.ToString();
    }
}
