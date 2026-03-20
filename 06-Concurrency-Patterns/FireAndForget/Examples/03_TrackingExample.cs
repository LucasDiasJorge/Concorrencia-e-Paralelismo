using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// EXAMPLE 3: Fire and Forget with Tracking
/// 
/// Sometimes you need to track fire-and-forget operations to know when they complete.
/// This example shows how to maintain control over background operations.
/// </summary>
class FireAndForgetWithTrackingExample : IExample
{
    private readonly BackgroundTaskTracker _tracker = new();
    
    public async Task Run()
    {
        Console.WriteLine("PATTERN 1: List-Based Tracking");
        Console.WriteLine("─".PadRight(50, '─'));
        
        await DemonstrateListTracking();
        
        Console.WriteLine("\nPATTERN 2: Task Factory Tracking");
        Console.WriteLine("─".PadRight(50, '─'));
        
        await DemonstrateTaskFactory();
        
        Console.WriteLine("\nPATTERN 3: Structured Concurrency");
        Console.WriteLine("─".PadRight(50, '─'));
        
        await DemonstrateStructuredConcurrency();
    }
    
    async Task DemonstrateListTracking()
    {
        List<Task> tasks = new List<Task>();
        
        for (int i = 0; i < 3; i++)
        {
            int taskId = i;
            Task task = Task.Run(async () =>
            {
                Console.WriteLine($"  └─ Task {taskId} started");
                await Task.Delay(1000 + (taskId * 500));
                Console.WriteLine($"  └─ Task {taskId} completed");
            });
            
            tasks.Add(task);
        }
        
        Console.WriteLine($"✓ Started {tasks.Count} background tasks");
        
        // Wait for all to complete if needed
        Console.WriteLine("  Waiting for all tasks...");
        await Task.WhenAll(tasks);
        Console.WriteLine("✓ All tasks completed");
    }
    
    async Task DemonstrateTaskFactory()
    {
        TaskFactory factory = new TaskFactory();
        List<Task> tasks = new List<Task>();
        
        for (int i = 0; i < 3; i++)
        {
            int taskId = i;
            Task task = factory.StartNew(async () =>
            {
                Console.WriteLine($"  └─ Factory Task {taskId} started");
                await Task.Delay(800);
                Console.WriteLine($"  └─ Factory Task {taskId} completed");
            }).Unwrap();
            
            tasks.Add(task);
        }
        
        Console.WriteLine($"✓ Started {tasks.Count} factory-based tasks");
        await Task.WhenAll(tasks);
        Console.WriteLine("✓ All factory tasks completed");
    }
    
    async Task DemonstrateStructuredConcurrency()
    {
        Console.WriteLine("✓ Using background task tracker");
        
        for (int i = 0; i < 3; i++)
        {
            int taskId = i;
            _tracker.Track(async () =>
            {
                Console.WriteLine($"  └─ Tracked Task {taskId} started");
                await Task.Delay(700);
                Console.WriteLine($"  └─ Tracked Task {taskId} completed");
            });
        }
        
        Console.WriteLine($"✓ {_tracker.PendingCount} tasks registered");
        await _tracker.WaitAllAsync();
        Console.WriteLine($"✓ All tracked tasks completed. Total executed: {_tracker.CompletedCount}");
    }
}

/// <summary>
/// Background Task Tracker - Structured Concurrency for Fire-and-Forget
/// 
/// This pattern ensures all background operations can be tracked and awaited if needed.
/// Useful for scenarios where you need graceful shutdown or completion monitoring.
/// </summary>
class BackgroundTaskTracker : IDisposable
{
    private readonly HashSet<Task> _tasks = new();
    private readonly ReaderWriterLockSlim _lock = new();
    private int _completedCount;
    
    public int PendingCount 
    { 
        get 
        {
            _lock.EnterReadLock();
            try
            {
                return _tasks.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        } 
    }
    
    public int CompletedCount => _completedCount;
    
    /// <summary>
    /// Track a fire-and-forget operation.
    /// The operation runs in the background but is tracked for completion.
    /// </summary>
    public void Track(Func<Task> operation)
    {
        Task task = TrackInternal(operation);
    }
    
    private async Task TrackInternal(Func<Task> operation)
    {
        Task? task = null;
        
        try
        {
            task = operation();
            
            _lock.EnterWriteLock();
            try
            {
                _tasks.Add(task);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
            await task;
            Interlocked.Increment(ref _completedCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ⚠️  Task error: {ex.Message}");
        }
        finally
        {
            if (task != null)
            {
                _lock.EnterWriteLock();
                try
                {
                    _tasks.Remove(task);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
        }
    }
    
    /// <summary>
    /// Wait for all tracked operations to complete.
    /// Useful for graceful shutdown scenarios.
    /// </summary>
    public async Task WaitAllAsync()
    {
        while (true)
        {
            Task[] snapshot;
            _lock.EnterReadLock();
            try
            {
                if (_tasks.Count == 0)
                    break;
                snapshot = _tasks.ToArray();
            }
            finally
            {
                _lock.ExitReadLock();
            }
            
            await Task.WhenAll(snapshot);
        }
    }
    
    public void Dispose()
    {
        _lock.EnterWriteLock();
        try
        {
            _tasks.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
            _lock.Dispose();
        }
    }
}

/// <summary>
/// INTERNAL MECHANICS:
/// 
/// 1. WHY TRACK FIRE-AND-FORGET?
///    - Graceful application shutdown
///    - Monitoring ongoing background work
///    - Preventing premature process termination
///    - Testing and debugging
/// 
/// 2. TRACKING APPROACHES
///    
///    A) List of Tasks (Simple)
///       - Store all running tasks in a collection
///       - Use Task.WhenAll to wait for completion
///       - Works well for a known number of tasks
///       
///    B) Thread-Safe Tracker (Production)
///       - Manage task lifecycle properly
///       - Handle task addition and removal atomically
///       - Track completion statistics
///       - Provide graceful shutdown mechanism
/// 
/// 3. SYNCHRONIZATION PATTERNS
///    - Use Lock/ReaderWriterLock for thread-safe access
///    - Snapshot the task collection to avoid holding locks during await
///    - Remove tasks after they complete to avoid memory leaks
/// 
/// 4. REAL-WORLD SCENARIOS
///    - Background notification sending
///    - Logging operations
///    - Cache invalidation
///    - User analytics
///    - Report generation
/// </summary>
