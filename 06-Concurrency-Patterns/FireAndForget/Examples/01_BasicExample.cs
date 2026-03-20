using System;
using System.Threading.Tasks;

/// <summary>
/// EXAMPLE 1: Basic Fire and Forget Pattern
/// 
/// Fire and Forget occurs when you start an async operation without awaiting it.
/// The operation runs in the background independently of the caller.
/// </summary>
class FireAndForgetBasicExample : IExample
{
    public async Task Run()
    {
        Console.WriteLine("APPROACH 1: Using async void (AVOID in production)");
        Console.WriteLine("─".PadRight(50, '─'));
        
        // ❌ NOT RECOMMENDED: async void
        // This approach is problematic because exceptions are unobserved
        FireAndForgetOperation();
        
        Console.WriteLine("✓ Operation started (async void method called)");
        await Task.Delay(3000);
        Console.WriteLine("✓ Main continues and waits for background operation");
        
        Console.WriteLine("\nAPPROACH 2: Using Task without await (RECOMMENDED)");
        Console.WriteLine("─".PadRight(50, '─'));
        
        // ✓ RECOMMENDED: Start task without awaiting
        // The task runs in the background without blocking execution
        _ = FireAndForgetTaskOperation();
        
        Console.WriteLine("✓ Task-based fire and forget started");
        await Task.Delay(3000);
        Console.WriteLine("✓ Main continues while background operation completes");
        
        Console.WriteLine("\nAPPROACH 3: Using Task with explicit fire and forget");
        Console.WriteLine("─".PadRight(50, '─'));
        
        // ✓ MOST EXPLICIT & RECOMMENDED
        #pragma warning disable CS4014
        BackgroundWorkAsync();
        #pragma warning restore CS4014
        
        Console.WriteLine("✓ Fire and forget with pragma directive (suppresses warning)");
        await Task.Delay(3000);
    }
    
    // ❌ Problematic: async void
    // - Exceptions crash the application
    // - Cannot track completion
    // - Synchronization context issues
    async void FireAndForgetOperation()
    {
        await Task.Delay(1000);
        Console.WriteLine("  └─ Background operation completed (async void)");
    }
    
    // ✓ Good: async Task
    // - Exceptions can be observed
    // - Operation can be tracked
    // - Better control over execution
    async Task FireAndForgetTaskOperation()
    {
        await Task.Delay(1000);
        Console.WriteLine("  └─ Background operation completed (async Task)");
    }
    
    // ✓ Alternative: Non-async entry point
    async Task BackgroundWorkAsync()
    {
        await Task.Delay(1000);
        Console.WriteLine("  └─ Background work completed (explicit discard)");
    }
}

/// <summary>
/// KEY CONCEPTS:
/// 
/// 1. ASYNC VOID vs ASYNC TASK
///    - async void: Cannot track, unobserved exceptions crash app
///    - async Task: Can track, exceptions observable (but ignored if not awaited)
/// 
/// 2. THE UNDERSCORE DISCARD (_)
///    The underscore explicitly tells the compiler you intentionally ignore the Task.
///    This suppresses "not awaited" warnings and makes intent clear.
/// 
/// 3. EXECUTION MODEL
///    When you don't await a Task, it:
///    - Captures the current SynchronizationContext (if any)
///    - Runs on a ThreadPool thread
///    - Completes independently of the caller
///    - May complete after main thread finishes
/// 
/// 4. CLR BEHAVIOR
///    The runtime keeps track of fire-and-forget tasks:
///    - If they complete with unobserved exceptions, TaskScheduler.UnobservedTaskException fires
///    - The AppDomain may crash if the exception isn't handled
///    - Modern .NET versions are more lenient than .NET Framework
/// </summary>
