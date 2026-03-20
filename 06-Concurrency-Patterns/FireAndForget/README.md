# Fire and Forget Pattern in C#

## 📋 Table of Contents

1. [What is Fire and Forget?](#what-is-fire-and-forget)
2. [Core Concepts](#core-concepts)
3. [Implementation Approaches](#implementation-approaches)
4. [Error Handling](#error-handling)
5. [Real-World Use Cases](#real-world-use-cases)
6. [Best Practices](#best-practices)
7. [Anti-Patterns](#anti-patterns)
8. [Performance Impact](#performance-impact)
9. [Examples](#examples)

## 🎯 What is Fire and Forget?

**Fire and Forget** is an asynchronous pattern where you start a background operation but don't wait for it to complete. The caller continues execution immediately while the operation proceeds independently.

### Core Characteristic

```csharp
// Fire and Forget: Start operation, don't wait
_ = BackgroundOperationAsync();
Console.WriteLine("This prints immediately!");

// Regular Await: Wait for operation to complete
await BackgroundOperationAsync();
Console.WriteLine("This prints after operation completes");
```

### Key Principle

> Fire and Forget means **starting** an async operation without **awaiting** it.

## 🧠 Core Concepts

### 1. Execution Model

When you fire-and-forget an async operation:

1. **Task Creation**: The async method is invoked, creating a Task
2. **Context Capture**: The current `SynchronizationContext` is captured (if any)
3. **Immediate Return**: The method returns immediately without waiting
4. **Background Execution**: The operation continues on a ThreadPool thread
5. **Independent Completion**: The operation completes independently of the caller

### 2. Thread Behavior

```
Main Thread Timeline:
├─ T0: Start fire-and-forget operation
├─ T0: Return immediately (non-blocking!)
├─ T0-T100: Execute next statements (continues normally)
│
ThreadPool Timeline:
├─ T50: Background operation starts (whenever ThreadPool schedules it)
├─ T50-T200: Operation executes
└─ T200: Operation completes (main thread doesn't wait)
```

### 3. Synchronization Context

The fire-and-forget operation captures the current `SynchronizationContext`:

```csharp
// On UI thread (SynchronizationContext = UIContext)
_ = BackgroundOperationAsync();  // Captures UIContext for continuation

// In ThreadPool (SynchronizationContext = null)
_ = BackgroundOperationAsync();  // No context captured
```

### 4. Task Lifecycle

```csharp
// Fire-and-forget task has these states:
var task = OperationAsync();  // Created (WaitingForActivation)
_ = task;                      // Returns immediately
// Task continues on ThreadPool...
// Eventually reaches: RanToCompletion, Faulted, or Cancelled
```

## 🛠️ Implementation Approaches

### Approach 1: Direct Task Discard (RECOMMENDED)

```csharp
// Clear intent: explicitly discard the task
_ = BackgroundOperationAsync();
```

**Pros:**
- Simple and readable
- Suppresses compiler warning
- Explicit intent with the `_` discard

**Cons:**
- Still need to handle exceptions inside the operation
- No tracking of completion

**When to use:**
- Simple background operations
- Exception handling inside operation
- One-off fire-and-forget calls

---

### Approach 2: async void (❌ AVOID)

```csharp
// ❌ NOT RECOMMENDED
async void BackgroundOperationBad()
{
    await Task.Delay(1000);
}

BackgroundOperationBad();  // Fire and forget (async void)
```

**Problems:**
- Unobserved exceptions crash the application
- No way to track completion
- SynchronizationContext capture issues
- Difficult to debug
- Microsoft strongly discourages this pattern

**Exception Behavior:**
```csharp
async void BadOperationAsync()
{
    await Task.Delay(500);
    throw new Exception("CRASH!");  // ← This crashes the AppDomain!
}
```

---

### Approach 3: Fire-and-Forget Extension Method (BEST FOR PRODUCTION)

```csharp
/// Extension method for safe fire-and-forget
public static void FireAndForget(
    this Task task,
    Action<Exception?>? errorHandler = null)
{
    _ = task.ContinueWith(t =>
    {
        if (t.IsFaulted)
        {
            var ex = t.Exception?.GetBaseException();
            errorHandler?.Invoke(ex);
        }
    }, TaskScheduler.Default);
}

// Usage:
BackgroundOperationAsync().FireAndForget(
    ex => Logger.Error("Background operation failed", ex)
);
```

**Pros:**
- Explicit error handling
- Reusable across codebase
- Intent very clear
- Production-ready

**Cons:**
- Slightly more complex
- Still need to define the extension method

---

### Approach 4: Task.Run Wrapper

```csharp
_ = Task.Run(async () =>
{
    try
    {
        await BackgroundOperationAsync();
    }
    catch (Exception ex)
    {
        Logger.Error("Operation failed", ex);
    }
});
```

**Pros:**
- Guaranteed ThreadPool execution
- Exception handling inside
- Can track completion if needed

**Cons:**
- More verbose
- Extra task overhead

---

### Approach 5: Structured Concurrency (BEST FOR MULTIPLE OPERATIONS)

```csharp
var tracker = new BackgroundWorkTracker();

// Queue multiple operations
for (int i = 0; i < 10; i++)
{
    tracker.Track(async () =>
    {
        await ProcessAsync(i);
    });
}

// Wait for all to complete
await tracker.WaitAllAsync();  // Graceful shutdown
```

**Pros:**
- Track all background operations
- Graceful shutdown support
- Know when operations complete
- Prevent premature exit

**Cons:**
- More boilerplate
- Requires tracker implementation

## 🚨 Error Handling

### Problem: Unobserved Exceptions

```csharp
// ❌ DANGEROUS: Exception is unobserved
_ = OperationThatThrowsAsync();

// The exception happens in background:
async Task OperationThatThrowsAsync()
{
    await Task.Delay(500);
    throw new Exception("DANGER!");  // Where does this go?
}
```

### Solution 1: Handle Inside Operation

```csharp
// ✓ BEST: Handle the exception inside
_ = OperationWithHandlingAsync();

async Task OperationWithHandlingAsync()
{
    try
    {
        await Task.Delay(500);
        // Do work
    }
    catch (Exception ex)
    {
        Logger.Error("Operation failed", ex);
    }
}
```

### Solution 2: ContinueWith

```csharp
// ✓ GOOD: Use ContinueWith to observe completion
var task = OperationAsync();
task.ContinueWith(t =>
{
    if (t.IsFaulted)
    {
        Logger.Error("Operation failed", t.Exception);
    }
});
```

### Solution 3: Extension Method (RECOMMENDED)

```csharp
public static void FireAndForget<T>(
    this Task<T> task,
    Action<T>? successHandler = null,
    Action<Exception?>? errorHandler = null)
{
    _ = task.ContinueWith(t =>
    {
        if (t.IsFaulted)
        {
            errorHandler?.Invoke(t.Exception?.GetBaseException());
        }
        else if (t.IsCompletedSuccessfully)
        {
            successHandler?.Invoke(t.Result);
        }
    }, TaskScheduler.Default);
}

// Usage:
OperationAsync().FireAndForget(
    success: result => Console.WriteLine($"Completed: {result}"),
    error: ex => Logger.Error("Failed", ex)
);
```

## 💼 Real-World Use Cases

### 1. **Background Notifications** ⭐

```csharp
// User submits form, send confirmation email
[HttpPost("/submit")]
public async Task<IActionResult> SubmitForm(FormData data)
{
    // Save form immediately
    await _repository.SaveAsync(data);
    
    // Send email in background (don't wait)
    _ = _emailService.SendConfirmationAsync(data.Email);
    
    // Return response immediately
    return Ok(new { message = "Form submitted" });
}
```

**Why:**
- Email sending takes time (network I/O)
- User shouldn't wait for email to be sent
- Operation doesn't affect form submission result

---

### 2. **Logging and Monitoring**

```csharp
public class RequestLogger
{
    public void LogRequest(HttpContext context)
    {
        // Log request in background (don't block request handling)
        _ = LogToFileAsync(context);
        
        // Move on immediately
    }
    
    private async Task LogToFileAsync(HttpContext context)
    {
        try
        {
            await _logger.WriteAsync($"[{DateTime.Now}] {context.Request.Path}");
        }
        catch (Exception ex)
        {
            // Handle logging failure gracefully
            System.Diagnostics.Debug.WriteLine($"Logging failed: {ex.Message}");
        }
    }
}
```

**Why:**
- Logging shouldn't block request processing
- Request handling takes priority
- Logging failure shouldn't crash the app

---

### 3. **Cache Invalidation**

```csharp
public async Task<User> UpdateUserAsync(int userId, UserData data)
{
    // Update database
    var updated = await _repository.UpdateAsync(userId, data);
    
    // Invalidate cache in background (don't wait)
    _ = _cache.InvalidateAsync($"user:{userId}");
    
    // Return immediately
    return updated;
}
```

**Why:**
- Cache invalidation is not critical to operation
- Network call to cache might be slow
- Response shouldn't wait for cache invalidation

---

### 4. **Analytics and Telemetry**

```csharp
public class AnalyticsTracker
{
    public void TrackUserAction(string action, Dictionary<string, object> data)
    {
        // Fire off analytics call without waiting
        _ = SendAnalyticsAsync(action, data);
        
        // User interaction continues immediately
    }
    
    private async Task SendAnalyticsAsync(string action, Dictionary<string, object> data)
    {
        try
        {
            await _analyticsClient.TrackAsync(action, data);
        }
        catch
        {
            // Analytics failure shouldn't affect user experience
        }
    }
}
```

**Why:**
- Analytics is supplementary
- User shouldn't wait for tracking request
- Failure is non-critical

---

### 5. **Report Generation**

```csharp
[HttpPost("/generate-report")]
public IActionResult GenerateReport(ReportParams parameters)
{
    // Start report generation in background
    _ = _reportService.GenerateAsync(parameters);
    
    // Return immediately with job ID
    return Accepted(new { jobId = "report-123", status = "Processing" });
}
```

**Why:**
- Report generation takes significant time
- User doesn't need to wait
- Can check status asynchronously

---

### 6. **Cleanup and Maintenance Tasks**

```csharp
public class TempFileManager
{
    public async Task<string> CreateTempFileAsync(byte[] content)
    {
        string path = await _storage.SaveAsync(content);
        
        // Schedule deletion in background (don't wait)
        _ = DeleteAfterDelayAsync(path, TimeSpan.FromHours(24));
        
        return path;
    }
    
    private async Task DeleteAfterDelayAsync(string path, TimeSpan delay)
    {
        await Task.Delay(delay);
        try
        {
            await _storage.DeleteAsync(path);
        }
        catch (Exception ex)
        {
            Logger.Warn($"Cleanup failed: {ex.Message}");
        }
    }
}
```

**Why:**
- Cleanup is ancillary
- Shouldn't block primary operation
- Failure isn't critical

---

### 7. **WebSocket/SignalR Broadcasts**

```csharp
[ApiController]
public class NotificationController
{
    [HttpPost("notify")]
    public async Task<IActionResult> NotifyUsers(string message)
    {
        // Fire-and-forget SignalR broadcast
        _ = _hubContext.Clients.All.SendAsync("message", message);
        
        // Return immediately (don't wait for all clients)
        return Ok();
    }
}
```

**Why:**
- Broadcasting to many clients takes time
- Response shouldn't wait for all clients
- Some clients might be disconnected

## ✅ Best Practices

### 1. **Always Handle Exceptions**

```csharp
// ✓ GOOD: Exception handled inside
_ = OperationAsync();

async Task OperationAsync()
{
    try
    {
        await Task.Delay(500);
        // Do work
    }
    catch (Exception ex)
    {
        Logger.Error("Operation failed", ex);
    }
}

// ✓ GOOD: Exception handled with extension method
OperationAsync().FireAndForget(
    ex => Logger.Error("Background operation failed", ex)
);
```

### 2. **Never Use async void**

```csharp
// ❌ AVOID
async void BadExample()
{
    await Task.Delay(500);
}

// ✓ GOOD
async Task GoodExample()
{
    await Task.Delay(500);
}
```

### 3. **Use Underscore Discard (_) for Clarity**

```csharp
// ✓ GOOD: Intent is clear
_ = BackgroundOperationAsync();

// ⚠️ RISKY: Same functionality but unclear intent
#pragma warning disable CS4014
BackgroundOperationAsync();
#pragma warning restore CS4014
```

### 4. **Comments Explain Why**

```csharp
// ✓ GOOD: Explain the decision
// Fire-and-forget because email delivery doesn't affect form submission
// Email service has its own error handling
_ = _emailService.SendAsync(email);

// ❌ BAD: No explanation
_ = _emailService.SendAsync(email);
```

### 5. **Log Failures**

```csharp
// ✓ GOOD: Any failures are logged
_ = BackgroundWorkAsync();

async Task BackgroundWorkAsync()
{
    try
    {
        // Do work
    }
    catch (Exception ex)
    {
        // ALWAYS log, even if non-critical
        Logger.Warn($"Background operation failed: {ex.Message}");
    }
}
```

### 6. **Use Extension Methods for Common Patterns**

```csharp
// ✓ GOOD: Reusable pattern
public static void FireAndForget(
    this Task task,
    Action<Exception?>? errorHandler = null)
{
    _ = task.ContinueWith(t =>
    {
        if (t.IsFaulted)
            errorHandler?.Invoke(t.Exception?.GetBaseException());
    }, TaskScheduler.Default);
}

// Usage across codebase:
OperationAsync().FireAndForget(ex => Logger.Error("Failed", ex));
```

### 7. **Consider Tracking for Multiple Operations**

```csharp
// ✓ GOOD: Track multiple fire-and-forget operations
var tracker = new BackgroundTaskTracker();

for (int i = 0; i < 10; i++)
{
    tracker.Track(async () => await ProcessItemAsync(i));
}

// Ensure all complete during graceful shutdown
await tracker.WaitAllAsync();
```

## ❌ Anti-Patterns

### Anti-Pattern 1: Unchecked async void

```csharp
// ❌ DANGER: Exceptions crash the app
async void BackgroundOperation()
{
    await Task.Delay(1000);
    throw new Exception("CRASH!");
}

BackgroundOperation();  // Exception is unobserved
```

### Anti-Pattern 2: Fire and Forget without Error Handling

```csharp
// ❌ DANGER: Where does the exception go?
_ = _repository.SaveDataAsync();  // No logging, no handling

// Later, in production:
// Exception occurs silently, data not saved, nobody knows
```

### Anti-Pattern 3: Mixing .Wait() with Fire-and-Forget

```csharp
// ❌ DANGER: Defeats the purpose
_ = OperationAsync();        // Fire-and-forget
var result = OtherOperation.Wait();  // Then blocking wait
// This might cause deadlock scenarios
```

### Anti-Pattern 4: Ignoring Deadlock Risks

```csharp
// ❌ DANGEROUS on UI thread (deadlock possible)
var result = OperationAsync().Result;  // DEADLOCK!

// Fire-and-forget, then try to access result
_ = OperationAsync();
CommonProperties.Result = ???;  // Can't get result here
```

### Anti-Pattern 5: No Comments Explaining Decision

```csharp
// ❌ UNCLEAR: Why fire-and-forget?
_ = _analytics.TrackAsync(data);

// 6 months later: New developer reads this
// "Why aren't we awaiting this? Is this a bug?"
```

## 📊 Performance Impact

### Throughput Comparison

```
Scenario: Process 1000 items with 10ms operation each

Sequential (Await each):     10,000ms (10 seconds)
├─ Item 1: 10ms
├─ Item 2: 10ms
└─ Item 1000: 10ms

Fire-and-Forget (All parallel):  ~100ms (depends on ThreadPool size)
├─ Item 1-1000:  All in parallel
└─ Last one completes: ~100ms

Speedup: ~100x faster with fire-and-forget!
```

### ThreadPool Considerations

```csharp
// Fire-and-forget uses ThreadPool
var maxThreadCount = Environment.ProcessorCount;
Console.WriteLine($"Max ThreadPool threads: {maxThreadCount}");

// Too many fire-and-forget operations can overwhelm ThreadPool
for (int i = 0; i < 10000; i++)
{
    _ = ExpensiveOperationAsync();  // ⚠️ May queue, not parallelism
}
```

### Memory Implications

```
Each Task has memory overhead:
- Task object: ~100 bytes
- Task state machine: varies
- Captured context: variable

Fire-and-forget 10,000 tasks: ~1-10MB (temporary)
But if tasks accumulate without completing: memory leak!
```

### CPU Usage Pattern

```
Fire-and-Forget:
┌──────────────────────────────────┐  CPU Usage
│ ██████████████████████████       │  100% (efficient ThreadPool usage)
└──────────────────────────────────┘

Await Sequential:
┌──────────────────────────────────┐  CPU Usage
│ ██░░░░░░░░░░░░░░░░░░░░░░░░░░░░  │  10% (waiting on I/O)
└──────────────────────────────────┘
```

## 🧪 Examples

The project includes 5 detailed examples:

### Example 1: Basic Fire and Forget
- Simple async operations
- async void vs async Task
- Task discard with underscore
- CLR behavior explanation

### Example 2: Error Handling
- Try-catch inside operations
- ContinueWith error handling
- Extension method pattern
- Task.Run wrapping

### Example 3: Fire and Forget with Tracking
- List-based tracking
- Task factory approach
- Structured concurrency
- Background task tracker

### Example 4: Fire and Forget vs Task.Run
- Comparison matrix
- Performance benchmarks
- Synchronization context
- When to use each approach

### Example 5: Best Practices
- Anti-patterns explained
- Right way to do it
- Extension method implementation
- Decision tree

## 🎓 Learning Path

1. **Start:** Run Example 1 (Basic)
2. **Understand:** Read Core Concepts section
3. **Practice:** Example 2 (Error Handling)
4. **Apply:** Study Real-World Use Cases
5. **Avoid:** Review Anti-Patterns section
6. **Optimize:** Example 4 (Performance)
7. **Master:** Example 5 (Best Practices)

## 📚 Key Takeaways

| Aspect | Details |
|--------|---------|
| **Definition** | Starting async operation without awaiting |
| **Use Case** | Background operations that don't block caller |
| **Best Implementation** | async Task with _ discard or extension method |
| **Avoid** | async void (except event handlers) |
| **Performance** | Can be 10-100x faster than sequential awaiting |
| **Pitfall** | Unobserved exceptions in unhandled operations |
| **Production-Ready** | Extension method with error callback |
| **Multiple Ops** | Use BackgroundTaskTracker for structured concurrency |

## 🔗 Common Patterns

### Pattern 1: Background Email
```csharp
_ = _emailService.SendAsync(recipient).FireAndForget(
    ex => _logger.Error("Email failed", ex)
);
```

### Pattern 2: Async Cleanup
```csharp
_ = _storage.DeleteAsync(path).FireAndForget(
    ex => _logger.Warn("Cleanup failed", ex)
);
```

### Pattern 3: Tracking Multiple
```csharp
var tracker = new BackgroundTaskTracker();
foreach (var item in items)
    tracker.Track(() => ProcessAsync(item));
await tracker.WaitAllAsync();
```

## 🚀 When to Fire-and-Forget

✅ **Good Fit:**
- Email/SMS notifications
- Logging operations
- Analytics tracking
- Cache invalidation
- Report generation
- Cleanup tasks

❌ **Bad Fit:**
- Database operations you need to validate
- API calls where result matters
- Critical operations
- Operations holding locks
- Anything that must complete before response

---

**Remember:** Fire-and-forget is powerful but requires discipline. Always handle exceptions and make the async nature explicit through code comments and naming.
