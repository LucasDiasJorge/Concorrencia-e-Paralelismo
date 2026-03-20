using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// EXAMPLE 5: Best Practices & Anti-Patterns
/// 
/// This example demonstrates what to DO and what NOT to DO when using fire-and-forget.
/// </summary>
class FireAndForgetBestPracticesExample : IExample
{
    public async Task Run()
    {
        Console.WriteLine("ANTI-PATTERN 1: async void");
        Console.WriteLine("─".PadRight(50, '─'));
        DemonstrateAsyncVoidAntiPattern();
        await Task.Delay(1000);
        
        Console.WriteLine("\nANTI-PATTERN 2: Ignoring exceptions");
        Console.WriteLine("─".PadRight(50, '─'));
        await DemonstrateIgnoredExceptions();
        
        Console.WriteLine("\nANTI-PATTERN 3: Deadlock scenarios");
        Console.WriteLine("─".PadRight(50, '─'));
        await DemonstrateDeadlockRisks();
        
        Console.WriteLine("\nBEST PRACTICE 1: Explicit fire-and-forget");
        Console.WriteLine("─".PadRight(50, '─'));
        await DemonstrateBestPractice1();
        
        Console.WriteLine("\nBEST PRACTICE 2: Extension method for fire-and-forget");
        Console.WriteLine("─".PadRight(50, '─'));
        await DemonstrateBestPractice2();
        
        Console.WriteLine("\nBEST PRACTICE 3: Structured concurrency");
        Console.WriteLine("─".PadRight(50, '─'));
        await DemonstrateBestPractice3();
    }
    
    // ❌ ANTI-PATTERN: async void
    void DemonstrateAsyncVoidAntiPattern()
    {
        Console.WriteLine("❌ AVOID THIS:");
        Console.WriteLine("   async void FireAndForgetBad()");
        Console.WriteLine("   {");
        Console.WriteLine("       await Task.Delay(500);");
        Console.WriteLine("   }");
        Console.WriteLine();
        Console.WriteLine("PROBLEMS:");
        Console.WriteLine("  • Exceptions are unobserved");
        Console.WriteLine("  • Cannot track when operation completes");
        Console.WriteLine("  • SynchronizationContext capture issues");
        Console.WriteLine("  • Debugging is difficult");
        
        FireAndForgetBadAsync();
    }
    
    // ❌ ANTI-PATTERN: Ignoring exceptions silently
    async Task DemonstrateIgnoredExceptions()
    {
        Console.WriteLine("❌ AVOID THIS:");
        Console.WriteLine("   _ = RiskyOperationAsync(); // Ignoring exception!");
        Console.WriteLine();
        Console.WriteLine("PROBLEMS:");
        Console.WriteLine("  • Exceptions silently fail");
        Console.WriteLine("  • No way to diagnose issues");
        Console.WriteLine("  • May leave system in bad state");
        
        // Bad: Exception is completely ignored
        _ = RiskyOperationThatMightFailAsync();
        
        await Task.Delay(1000);
        Console.WriteLine("✓ Operation completed (but we don't know if it failed!)");
    }
    
    // ❌ ANTI-PATTERN: Deadlock risks
    async Task DemonstrateDeadlockRisks()
    {
        Console.WriteLine("❌ RISKY: Mixing sync and async in fire-and-forget");
        Console.WriteLine("   Block().Wait();      // DEADLOCK if called from UI thread!");
        Console.WriteLine("   GetResultAsync().Result; // DEADLOCK!");
        Console.WriteLine();
        Console.WriteLine("PROBLEMS:");
        Console.WriteLine("  • Can deadlock on UI thread");
        Console.WriteLine("  • SynchronizationContext blocking issues");
        Console.WriteLine("  • ASP.NET thread pool exhaustion");
        Console.WriteLine("  • .NET Framework more prone than .NET Core");
        
        Console.WriteLine("\n✓ Safe: Always use await, not .Wait() or .Result");
    }
    
    // ✓ BEST PRACTICE 1: Explicit discard with compiler warning suppression
    async Task DemonstrateBestPractice1()
    {
        Console.WriteLine("✓ BEST: Explicit underscore discard");
        Console.WriteLine("   _ = BackgroundOperationAsync();");
        Console.WriteLine();
        Console.WriteLine("BENEFITS:");
        Console.WriteLine("  ✓ Compiler warning suppressed (intent clear)");
        Console.WriteLine("  ✓ Still need error handling inside operation");
        Console.WriteLine("  ✓ Simple and straightforward");
        
        _ = BackgroundOperationWithErrorHandlingAsync();
        
        await Task.Delay(1000);
        Console.WriteLine("  └─ ✓ Background operation completed");
    }
    
    // ✓ BEST PRACTICE 2: Fire-and-forget extension method
    async Task DemonstrateBestPractice2()
    {
        Console.WriteLine("✓ BEST: Fire-and-forget extension method");
        Console.WriteLine("   operation.FireAndForget(ex => Log(ex));");
        Console.WriteLine();
        Console.WriteLine("BENEFITS:");
        Console.WriteLine("  ✓ Exception handler clear and explicit");
        Console.WriteLine("  ✓ Reusable across codebase");
        Console.WriteLine("  ✓ Intent very clear");
        
        BackgroundOperationAsync().FireAndForget(
            ex => Console.WriteLine($"  └─ Error handled: {ex?.Message}")
        );
        
        await Task.Delay(1000);
        Console.WriteLine("  └─ ✓ Fire-and-forget completed with error handling");
    }
    
    // ✓ BEST PRACTICE 3: Structured concurrency
    async Task DemonstrateBestPractice3()
    {
        Console.WriteLine("✓ BEST: Structured concurrency with tracking");
        Console.WriteLine("   var tracker = new BackgroundWorkTracker();");
        Console.WriteLine("   tracker.Start(() => Operation());");
        Console.WriteLine("   await tracker.CompleteAsync();");
        Console.WriteLine();
        Console.WriteLine("BENEFITS:");
        Console.WriteLine("  ✓ All background work is tracked");
        Console.WriteLine("  ✓ Can gracefully shutdown");
        Console.WriteLine("  ✓ Know when all operations complete");
        Console.WriteLine("  ✓ Prevents process termination");
        
        StructuredConcurrencyTracker tracker = new StructuredConcurrencyTracker();
        tracker.Start(async () =>
        {
            await Task.Delay(500);
            Console.WriteLine("  └─ Tracked operation 1 completed");
        });
        tracker.Start(async () =>
        {
            await Task.Delay(700);
            Console.WriteLine("  └─ Tracked operation 2 completed");
        });
        
        await tracker.CompleteAsync();
        Console.WriteLine("  └─ ✓ All tracked operations completed");
    }
    
    async void FireAndForgetBadAsync()
    {
        await Task.Delay(500);
        Console.WriteLine("  └─ async void completed (but unobserved!)");
    }
    
    async Task RiskyOperationThatMightFailAsync()
    {
        await Task.Delay(500);
        throw new InvalidOperationException("This exception is completely ignored!");
    }
    
    async Task BackgroundOperationWithErrorHandlingAsync()
    {
        try
        {
            await Task.Delay(500);
            Console.WriteLine("  └─ Operation completed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  └─ Handled: {ex.Message}");
        }
    }
    
    async Task BackgroundOperationAsync()
    {
        await Task.Delay(500);
    }
}

// ✓ Structured Concurrency Tracker
class StructuredConcurrencyTracker
{
    private readonly TaskCompletionSource<bool> _completion = new();
    private int _pendingCount;
    
    public void Start(Func<Task> operation)
    {
        Interlocked.Increment(ref _pendingCount);
        _ = RunAsync(operation);
    }
    
    private async Task RunAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        finally
        {
            if (Interlocked.Decrement(ref _pendingCount) == 0)
            {
                _completion.TrySetResult(true);
            }
        }
    }
    
    public Task CompleteAsync() => _completion.Task;
}

/// <summary>
/// DECISION TREE FOR FIRE-AND-FORGET:
/// 
/// 1. Is exception handling needed?
///    ├─ YES → Use extension method or try-catch inside
///    └─ NO → Direct discard with _
/// 
/// 2. Do you need to track completion?
///    ├─ YES → Use BackgroundWorkTracker or collect tasks in list
///    └─ NO → Simple _ discard
/// 
/// 3. Is this a one-off or reusable?
///    ├─ ONE-OFF → Try-catch inside operation or direct discard
///    └─ REUSABLE → Create extension method
/// 
/// 4. Are there multiple operations?
///    ├─ FEW (1-5) → Collect in Task[] and Task.WhenAll
///    └─ MANY → Use structured concurrency tracker
/// 
/// REMEMBER:
/// ✓ ALWAYS handle exceptions (inside operation or with extension method)
/// ✓ ALWAYS use async Task, NEVER async void (except event handlers)
/// ✓ ALWAYS make intent clear (comments or extension method name)
/// ✓ ALWAYS test exception paths
/// ✓ ALWAYS consider what happens if operation fails
/// </summary>
/// 