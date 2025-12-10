# Lab 04: Smart Task Orchestrator with AI Planners

**Topic:** Planners & AI Orchestration  
**Estimated Time:** ~25 minutes  
**Objective:** Build a task automation system that uses AI planners to break down high-level goals into executable function sequences.

---

## 🎯 Overview

In this lab, you'll create a Smart Task Orchestrator that demonstrates the power of Semantic Kernel planners. The system will:
- Accept high-level goals (e.g., "Prepare for team meeting")
- Use `FunctionCallingStepwisePlanner` to automatically decompose goals into actionable steps
- Orchestrate multiple plugins to execute the plan
- Handle dynamic planning with context awareness

You'll implement three plugins with native functions that the planner will orchestrate:
1. **CalendarPlugin** - Check schedule, find available times
2. **EmailPlugin** - Send reminders and notifications
3. **DocumentPlugin** - Create agendas and meeting notes

---

## 📝 Tasks

### TODO N1 – Implement Calendar Plugin Functions

**Learning Objective:** Create native functions that can be discovered and invoked by the planner.

**Instructions:**
- Implement `GetTodaysSchedule()` method that returns a formatted string of today's meetings
  - Return format: "9:00 AM - Team Standup\n2:00 PM - Client Review"
  - Include 2-3 sample meetings
- Implement `FindNextAvailableSlot(int durationMinutes)` method
  - Find the next available time slot of the requested duration
  - Return format: "Tomorrow at 10:00 AM" or "Today at 4:00 PM"
  - Consider current time and existing meetings

**Hints:**
- Use `[KernelFunction]` attribute to make functions discoverable
- Use `[Description("...")]` to help the planner understand what each function does
- Return simple strings - the planner will interpret them

---

### TODO N2 – Implement Email Plugin Functions

**Learning Objective:** Create functions that simulate email operations for the planner to orchestrate.

**Instructions:**
- Implement `SendMeetingReminder(string recipient, string meetingTime, string topic)` method
  - Return confirmation message: "✓ Reminder sent to {recipient} for {topic} at {meetingTime}"
- Implement `SendAgendaEmail(string recipient, string agenda)` method
  - Return confirmation: "✓ Agenda sent to {recipient}"

**Hints:**
- These are simulation functions (no real emails)
- Clear descriptions help the planner choose the right function
- Return status messages that confirm the action

---

### TODO N3 – Configure and Use FunctionCallingStepwisePlanner

**Learning Objective:** Set up the planner and execute automated task orchestration.

**Instructions:**
- Create a `FunctionCallingStepwisePlanner` instance
- Configure `FunctionCallingStepwisePlannerOptions` with:
  - MaxIterations: 10
  - MaxTokens: 4000
- Create and execute a plan for the goal: "Schedule a team meeting for tomorrow and send reminders to all participants"
- The planner should automatically:
  1. Check the schedule using CalendarPlugin
  2. Find an available time slot
  3. Send reminder emails using EmailPlugin
- Capture and return the final result from the planner

**Hints:**
- Import the planner: `using Microsoft.SemanticKernel.Planning;`
- Use `await planner.CreatePlanAsync(kernel, goal)` to create the plan
- Execute the plan with `await plan.InvokeAsync(kernel)`
- The planner will automatically select and chain the right functions

---

## 🧪 Testing

Run the program to test your orchestrator:

```bash
dotnet run
```

Expected output should show:
1. Planner analyzing the goal
2. Sequential execution of discovered functions
3. Final result with completed actions

---

## 🎓 Key Concepts

- **Native Functions**: C# methods decorated with `[KernelFunction]` that planners can discover
- **Function Descriptions**: Help the AI understand when and how to use each function
- **Automatic Orchestration**: Planner selects and chains functions without manual specification
- **Stepwise Planning**: Iterative approach where the planner decides next steps based on previous results
- **Plugin Architecture**: Organized collections of related functions

---

## 📚 Resources

- [Semantic Kernel Planners Documentation](https://learn.microsoft.com/en-us/semantic-kernel/agents/planners/)
- [FunctionCallingStepwisePlanner Guide](https://learn.microsoft.com/en-us/semantic-kernel/agents/planners/stepwise-planner)

---

Good luck! 🚀
