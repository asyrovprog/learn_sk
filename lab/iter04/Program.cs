using Microsoft.SemanticKernel;
using SmartTaskOrchestrator;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🤖 Smart Task Orchestrator - Lab 04");
        Console.WriteLine("=" .PadRight(50, '='));
        Console.WriteLine();

        // Setup
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("❌ ERROR: OPENAI_API_KEY environment variable not set");
            return;
        }

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-4o-mini", apiKey);
        var kernel = builder.Build();

        // Register plugins
        try
        {
            kernel.Plugins.AddFromObject(new CalendarPlugin(), "Calendar");
            kernel.Plugins.AddFromObject(new EmailPlugin(), "Email");
            kernel.Plugins.AddFromObject(new DocumentPlugin(), "Document");
        }
        catch (ArgumentException ex) when (ex.Message.Contains("doesn't implement any [KernelFunction]"))
        {
            Console.WriteLine("❌ ERROR: Plugins are missing [KernelFunction] attributes");
            Console.WriteLine("   → See README section 'TODO N1' and 'TODO N2' for implementation guidance");
            return;
        }

        var orchestrator = new TaskOrchestrator(kernel);

        // Run tests
        int passed = 0;
        int total = 3;

        Console.WriteLine("Running Tests...");
        Console.WriteLine();

        // Test 1: CalendarPlugin functions
        try
        {
            Console.WriteLine("Test 1: CalendarPlugin Functions");
            var calendarPlugin = new CalendarPlugin();
            
            var schedule = calendarPlugin.GetTodaysSchedule();
            if (string.IsNullOrEmpty(schedule))
            {
                throw new Exception("GetTodaysSchedule returned empty result");
            }
            
            var slot = calendarPlugin.FindNextAvailableSlot(30);
            if (string.IsNullOrEmpty(slot))
            {
                throw new Exception("FindNextAvailableSlot returned empty result");
            }
            
            Console.WriteLine($"  Schedule: {schedule.Replace("\n", " | ")}");
            Console.WriteLine($"  Next slot: {slot}");
            Console.WriteLine("  ✅ PASS");
            passed++;
        }
        catch (NotImplementedException ex)
        {
            Console.WriteLine($"  ❌ FAIL - {ex.Message}");
            Console.WriteLine("     → See README section 'TODO N1 – Implement Calendar Plugin Functions'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ FAIL - {ex.Message}");
        }
        Console.WriteLine();

        // Test 2: EmailPlugin functions
        try
        {
            Console.WriteLine("Test 2: EmailPlugin Functions");
            var emailPlugin = new EmailPlugin();
            
            var reminder = emailPlugin.SendMeetingReminder("john@example.com", "Tomorrow at 10:00 AM", "Team Planning");
            if (!reminder.Contains("john@example.com") || !reminder.Contains("Team Planning"))
            {
                throw new Exception("SendMeetingReminder doesn't contain expected information");
            }
            
            var agendaEmail = emailPlugin.SendAgendaEmail("team@example.com", "Test agenda");
            if (!agendaEmail.Contains("team@example.com"))
            {
                throw new Exception("SendAgendaEmail doesn't contain recipient");
            }
            
            Console.WriteLine($"  Reminder: {reminder}");
            Console.WriteLine($"  Agenda email: {agendaEmail}");
            Console.WriteLine("  ✅ PASS");
            passed++;
        }
        catch (NotImplementedException ex)
        {
            Console.WriteLine($"  ❌ FAIL - {ex.Message}");
            Console.WriteLine("     → See README section 'TODO N2 – Implement Email Plugin Functions'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ FAIL - {ex.Message}");
        }
        Console.WriteLine();

        // Test 3: Planner orchestration
        try
        {
            Console.WriteLine("Test 3: Planner Orchestration");
            Console.WriteLine("  Goal: Schedule a team meeting for tomorrow and send reminders");
            Console.WriteLine("  Executing planner...");
            
            var result = await orchestrator.ExecuteGoalAsync(
                "Schedule a team meeting for tomorrow and send reminders to all participants");
            
            if (string.IsNullOrEmpty(result))
            {
                throw new Exception("ExecuteGoalAsync returned empty result");
            }
            
            Console.WriteLine($"  Result: {result}");
            Console.WriteLine("  ✅ PASS");
            passed++;
        }
        catch (NotImplementedException ex)
        {
            Console.WriteLine($"  ❌ FAIL - {ex.Message}");
            Console.WriteLine("     → See README section 'TODO N3 – Configure and Use FunctionCallingStepwisePlanner'");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ❌ FAIL - {ex.Message}");
            Console.WriteLine("     Make sure you've implemented TODO[N1] and TODO[N2] first");
        }
        Console.WriteLine();

        // Summary
        Console.WriteLine("=" .PadRight(50, '='));
        Console.WriteLine($"Tests: {passed}/{total} passed");
        
        if (passed == total)
        {
            Console.WriteLine("🎉 All tests passed! Lab complete!");
        }
        else
        {
            Console.WriteLine($"❌ {total - passed} test(s) failed. Review the TODOs above.");
        }
    }
}
