using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// EXAMPLE 2: Fire and Forget with Error Handling
/// 
/// This is the MOST IMPORTANT pattern to understand!
/// Unobserved exceptions in fire-and-forget operations are dangerous.
/// This example shows how to handle exceptions safely.
/// </summary>
class FireAndForgetWithErrorHandlingExample : IExample
{
    public async Task Run()
    {
        Console.WriteLine("PATTERN 1: Try-Catch Inside the Operation");
        Console.WriteLine("─".PadRight(50, '─'));
        
        _ = OperationWithInternalErrorHandling();
        await Task.Delay(2000);
        
        Console.WriteLine("\nPATTERN 2: ContinueWith for Error Handling");
        Console.WriteLine("─".PadRight(50, '─'));
        
        Task<string> task = RiskyOperationAsync();
        _ = task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                Console.WriteLine($"❌ Error caught: {t.Exception?.InnerException?.Message}");
            }
            else if (t.IsCompletedSuccessfully)
            {
                Console.WriteLine($"✓ Operation completed: {t.Result}");
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
        
        await Task.Delay(2000);
        
        Console.WriteLine("\nPATTERN 3: Fire-and-Forget Extension Method");
        Console.WriteLine("─".PadRight(50, '─'));
        
        // Recommended approach: Use a helper method
        Task<string> operation = RiskyOperationAsync();
        operation.FireAndForget(ex => 
            Console.WriteLine($"❌ Error in fire-and-forget: {ex?.Message}")
        );
        
        await Task.Delay(2000);
        
        Console.WriteLine("\nPATTERN 4: Task.Run with Exception Aggregation");
        Console.WriteLine("─".PadRight(50, '─'));
        
        _ = Task.Run(async () =>
        {
            try
            {
                await RiskyOperationAsync();
                Console.WriteLine("✓ Wrapped operation completed");
            }
            catch (Exception ex)
            {
                // Log the exception - it won't crash the app
                Console.WriteLine($"❌ Handled exception: {ex.Message}");
            }
        });
        
        await Task.Delay(2000);
    }
    
    // Pattern 1: Error handling inside the operation
    async Task OperationWithInternalErrorHandling()
    {
        try
        {
            await Task.Delay(500);
            throw new InvalidOperationException("Something went wrong!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  └─ ✓ Error handled internally: {ex.Message}");
        }
    }
    
    // Pattern 2 & 3: Risky operation that might fail
    async Task<string> RiskyOperationAsync()
    {
        await Task.Delay(500);
        // 50% chance of failure
        if (DateTime.Now.Millisecond % 2 == 0)
            throw new Exception("Random operation failure");
        return "Success!";
    }
}

/// <summary>
/// CRITICAL INSIGHTS:
/// 
/// 1. UNOBSERVED EXCEPTIONS ARE DANGEROUS
///    - If a fire-and-forget task throws and nobody catches it,
///      TaskScheduler.UnobservedTaskException fires
///    - In .NET Framework, this could crash the AppDomain
///    - In .NET Core/.NET 6+, it just logs and continues
/// 
/// 2. EXCEPTION HANDLING STRATEGIES
///    
///    A) Exception inside the operation (SAFEST)
///       - The operation itself handles all exceptions
///       - No exceptions leak out
///       
///    B) ContinueWith for observation (GOOD)
///       - Continues after task completes, failed or not
///       - Can inspect the exception
///       - Doesn't block the caller
///       
///    C) Fire-and-Forget extension method (RECOMMENDED)
///       - Encapsulates the pattern
///       - Clear intent with explicit exception handling
///       - Can be reused across codebase
/// 
/// 3. SYNCHRONIZATION CONTEXT MATTERS
///    - ContinueWith(action, SynchronizationContext.Current)
///    - If SyncContext is null, runs on ThreadPool
///    - If SyncContext exists, runs on that context
///    - This affects where exception handlers execute
/// 
/// 4. BACKGROUND VS FOREGROUND EXCEPTIONS
///    - Fire-and-forget exceptions are "background"
///    - They won't stop the main thread
///    - But they can be observed via UnobservedTaskException
///    - Always logging is a good safety net
/// </summary>
