using System;
using System.Diagnostics;
using System.Threading.Tasks;

/// <summary>
/// EXAMPLE 4: Fire and Forget vs Task.Run
/// 
/// Compares different approaches to background operations and their implications.
/// Understanding the differences is crucial for choosing the right pattern.
/// </summary>
class FireAndForgetVsTaskRunExample : IExample
{
    public async Task Run()
    {
        Console.WriteLine("Comparing Approaches to Background Operations\n");
        
        Console.WriteLine("APPROACH 1: Awaiting (NOT fire-and-forget)");
        Console.WriteLine("─".PadRight(50, '─'));
        await DemonstrateAwait();
        
        Console.WriteLine("\nAPPROACH 2: Pure Fire and Forget (async void)");
        Console.WriteLine("─".PadRight(50, '─'));
        DemonstratePureFireAndForget();
        await Task.Delay(2000);
        
        Console.WriteLine("\nAPPROACH 3: Fire and Forget with Task.Run");
        Console.WriteLine("─".PadRight(50, '─'));
        await DemonstrateTaskRun();
        
        Console.WriteLine("\nAPPROACH 4: Fire and Forget with Task Discard");
        Console.WriteLine("─".PadRight(50, '─'));
        await DemonstrateTaskDiscard();
        
        Console.WriteLine("\nPERFORMANCE COMPARISON");
        Console.WriteLine("─".PadRight(50, '─'));
        await ComparePerformance();
    }
    
    async Task DemonstrateAwait()
    {
        Stopwatch sw = Stopwatch.StartNew();
        
        // Blocks until operation completes
        await BackgroundWorkAsync();
        
        sw.Stop();
        Console.WriteLine($"  └─ ⏱️  Total time: {sw.ElapsedMilliseconds}ms (BLOCKED)");
        Console.WriteLine($"  └─ ✓ Caller waits for operation to complete");
    }
    
    async void DemonstratePureFireAndForget()
    {
        Stopwatch sw = Stopwatch.StartNew();
        
        // ❌ PROBLEMS:
        // - async void method
        // - Exceptions crash the app
        // - No way to track completion
        PureFireAndForgetOperation();
        
        sw.Stop();
        Console.WriteLine($"  └─ ⏱️  Return time: {sw.ElapsedMilliseconds}ms (NOT BLOCKED)");
        Console.WriteLine($"  └─ ❌ DANGER: Unobserved exceptions!");
        Console.WriteLine($"  └─ ❌ DANGER: Cannot track completion!");
    }
    
    async Task DemonstrateTaskRun()
    {
        Stopwatch sw = Stopwatch.StartNew();
        
        // ✓ Starts work on ThreadPool
        // ✓ Returns immediately
        // ⚠️ Still need error handling
        _ = Task.Run(async () => await BackgroundWorkAsync());
        
        sw.Stop();
        Console.WriteLine($"  └─ ⏱️  Return time: {sw.ElapsedMilliseconds}ms (NOT BLOCKED)");
        Console.WriteLine($"  └─ ✓ Returns immediately");
        Console.WriteLine($"  └─ ✓ Runs on ThreadPool");
        
        await Task.Delay(1500);
        Console.WriteLine($"  └─ ✓ Background work completes independently");
    }
    
    async Task DemonstrateTaskDiscard()
    {
        Stopwatch sw = Stopwatch.StartNew();
        
        // ✓ RECOMMENDED: Direct task discard
        // ✓ Clear intent (explicit _ discard)
        // ✓ Suppresses compiler warning
        _ = BackgroundWorkAsync();
        
        sw.Stop();
        Console.WriteLine($"  └─ ⏱️  Return time: {sw.ElapsedMilliseconds}ms (NOT BLOCKED)");
        Console.WriteLine($"  └─ ✓ Returns immediately (preferred)");
        
        await Task.Delay(1500);
        Console.WriteLine($"  └─ ✓ Background work progresses");
    }
    
    async Task ComparePerformance()
    {
        const int iterations = 1000;
        
        // Test 1: Sequential awaiting
        Stopwatch sw1 = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            await FastOperationAsync();
        }
        sw1.Stop();
        
        // Test 2: Fire and forget (collect tasks)
        Stopwatch sw2 = Stopwatch.StartNew();
        Task[] tasks = new Task[iterations];
        for (int i = 0; i < iterations; i++)
        {
            tasks[i] = FastOperationAsync();
        }
        await Task.WhenAll(tasks);
        sw2.Stop();
        
        Console.WriteLine($"Sequential Awaiting   : {sw1.ElapsedMilliseconds}ms");
        Console.WriteLine($"Fire and Forget (All): {sw2.ElapsedMilliseconds}ms");
        Console.WriteLine($"Speedup: {(double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds:F2}x");
        Console.WriteLine($"  └─ Fire and Forget is {((double)sw1.ElapsedMilliseconds / sw2.ElapsedMilliseconds - 1) * 100:F1}% faster");
    }
    
    async void PureFireAndForgetOperation()
    {
        await Task.Delay(500);
        Console.WriteLine("  └─ async void: operation completed");
    }
    
    async Task BackgroundWorkAsync()
    {
        await Task.Delay(500);
        Console.WriteLine("  └─ Background work completed");
    }
    
    async Task FastOperationAsync()
    {
        await Task.Delay(1);
    }
}

/// <summary>
/// COMPARISON MATRIX:
/// 
/// ┌─────────────────┬──────────┬──────────┬──────────┬──────────┐
/// │ Aspect          │ Await    │ async    │ Task.Run │ Discard  │
/// │                 │          │ void     │          │          │
/// ├─────────────────┼──────────┼──────────┼──────────┼──────────┤
/// │ Non-blocking    │   ❌     │   ✓      │    ✓     │    ✓     │
/// │ Exception safe  │   ✓      │   ❌     │    ✓     │    ✓     │
/// │ Trackable       │   ✓      │   ❌     │    ✓     │    ✓     │
/// │ Compiler warns  │   ❌     │   ❌     │    ⚠️    │    ❌     │
/// │ Clear intent    │   ✓      │   ❌     │    ✓     │    ✓✓    │
/// │ Production use  │   ✓      │   ❌     │    ✓     │    ✓✓    │
/// └─────────────────┴──────────┴──────────┴──────────┴──────────┘
/// 
/// KEY INSIGHTS:
/// 
/// 1. PERFORMANCE TRADE-OFFS
///    - Awaiting blocks until completion (sequential)
///    - Fire-and-forget allows parallelism (concurrent)
///    - Fire-and-forget can be >> times faster
///    - But only when there are multiple independent operations
/// 
/// 2. EXCEPTION HANDLING
///    - Awaiting: Exceptions propagate to caller
///    - async void: Exceptions are unobserved (dangerous!)
///    - Discard: Exceptions still exist but must be handled inside
///    - Task.Run: Exceptions in the task, not thrown to caller
/// 
/// 3. SYNCHRONIZATION CONTEXT
///    - Await: Captures and uses SynchronizationContext
///    - async void: Captures context, but unobserved
///    - Discard: Uses current thread's context
///    - Task.Run: Forces ThreadPool execution
/// 
/// 4. WHEN TO USE EACH
///    - Await: When you need the result or must wait
///    - async void: NEVER (except for event handlers)
///    - Task.Run: When you need to ensure ThreadPool execution
///    - Discard: For background operations that don't need tracking
/// </summary>
