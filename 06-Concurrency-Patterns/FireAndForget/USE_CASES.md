# Fire and Forget - Real-World Use Cases

## 🌐 Web API Scenarios

### Use Case 1: Email Confirmation After Registration

**Scenario:** User registers on your platform

```csharp
[HttpPost("/register")]
public class UserRegistrationController
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(RegisterRequest request)
    {
        // 1. Validate request
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        // 2. Create user in database
        var user = await _userService.CreateUserAsync(request);
        
        // 3. Fire-and-forget: Send confirmation email
        // This shouldn't delay registration response
        _ = _emailService.SendConfirmationEmailAsync(user.Email, user.ConfirmationToken)
            .FireAndForget(ex => 
                _logger.Warn($"Failed to send confirmation email: {ex?.Message}")
            );
        
        // 4. Return immediately
        return Ok(new 
        { 
            userId = user.Id,
            message = "Please check your email to confirm registration"
        });
    }
}
```

**Why Fire-and-Forget:**
- Email delivery is slow (external service)
- User doesn't need to wait for email to register
- Email failure shouldn't prevent registration
- Better user experience (quick response)

**Alternative Actions:** Welcome email, SMS notification

---

### Use Case 2: Order Confirmation and Inventory Update

**Scenario:** E-commerce checkout

```csharp
[HttpPost("checkout")]
public async Task<IActionResult> CheckoutAsync(CheckoutRequest request)
{
    // 1. Process payment
    var payment = await _paymentService.ProcessAsync(request.PaymentInfo);
    if (!payment.Success)
        return BadRequest("Payment failed");
    
    // 2. Create order (must complete before response)
    var order = await _orderService.CreateOrderAsync(request.Items);
    
    // 3. Update inventory (background - eventual consistency)
    _ = _inventoryService.UpdateStockAsync(request.Items)
        .FireAndForget(ex =>
            _logger.Error("Inventory update failed", ex)
        );
    
    // 4. Send order confirmation email
    _ = _emailService.SendOrderConfirmationAsync(order)
        .FireAndForget();
    
    // 5. Trigger fulfillment process
    _ = _fulfillmentService.InitiateFulfillmentAsync(order)
        .FireAndForget();
    
    // 6. Return immediately
    return Ok(new { orderId = order.Id, total = order.Total });
}
```

**Why Fire-and-Forget:**
- Inventory update can happen asynchronously
- Email sending is non-critical to checkout success
- Fulfillment process triggers in background
- User gets immediate response

**Performance Impact:** Response time: ~500ms instead of ~3500ms

---

### Use Case 3: User Activity Logging

**Scenario:** Track user actions for analytics

```csharp
[ApiController]
[Route("api/[controller]")]
public class ActivityController : ControllerBase
{
    private readonly IActivityLogger _logger;
    
    [HttpGet("view/{itemId}")]
    public async Task<IActionResult> ViewItem(int itemId)
    {
        // 1. Get item data (fast)
        var item = await _itemService.GetItemAsync(itemId);
        
        // 2. Log activity in background (fire-and-forget)
        // We don't care if logging fails
        _ = _logger.LogActivityAsync(new ActivityLog
        {
            UserId = User.Identity.Name,
            Action = "ViewItem",
            ItemId = itemId,
            Timestamp = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        }).FireAndForget();
        
        // 3. Return item immediately
        return Ok(item);
    }
    
    [HttpPost("purchase/{itemId}")]
    public async Task<IActionResult> PurchaseItem(int itemId)
    {
        var result = await _purchaseService.PurchaseAsync(itemId);
        
        // Log multiple activities in background
        var tasks = new[]
        {
            _logger.LogActivityAsync(new ActivityLog { Action = "Purchase", ItemId = itemId }),
            _analytics.TrackEventAsync("purchase", new { itemId, price = result.Price }),
            _notifications.NotifyAdminsAsync($"New purchase: {itemId}"),
            _reportGenerator.UpdateDailyReportAsync(DateTime.Today)
        };
        
        // Fire all in parallel
        foreach (var task in tasks)
            _ = task.FireAndForget();
        
        return Ok(result);
    }
}
```

**Why Fire-and-Forget:**
- Logging shouldn't delay response
- Logging failure is non-critical
- Analytics can be eventual
- Multiple fire-and-forget operations run in parallel

---

## 📊 Data Processing Scenarios

### Use Case 4: Scheduled Report Generation

**Scenario:** Generate reports without blocking request

```csharp
[HttpPost("generate-report")]
public IActionResult GenerateReport(ReportRequest request)
{
    // Create job immediately
    var jobId = Guid.NewGuid();
    
    // Fire-and-forget: Start report generation in background
    _ = Task.Run(async () =>
    {
        try
        {
            var report = await _reportService.GenerateAsync(request);
            await _storage.SaveReportAsync(jobId, report);
            await _notificationHub.Clients.User(User.Identity.Name)
                .SendAsync("report-ready", jobId);
        }
        catch (Exception ex)
        {
            _logger.Error($"Report generation failed: {ex.Message}");
            await _storage.SaveErrorAsync(jobId, ex);
        }
    }, CancellationToken.None).FireAndForget();
    
    // Return job ID immediately
    return Accepted(new 
    { 
        jobId = jobId,
        statusUrl = $"/api/reports/status/{jobId}"
    });
}

[HttpGet("status/{jobId}")]
public async Task<IActionResult> GetReportStatus(Guid jobId)
{
    var status = await _storage.GetReportStatusAsync(jobId);
    return Ok(status);
}
```

**Why Fire-and-Forget:**
- Reports take minutes/hours to generate
- User shouldn't wait
- Use polling or WebSocket to check status
- Better UX with background processing

---

### Use Case 5: Batch Data Import

**Scenario:** Import large CSV files

```csharp
[HttpPost("import")]
public async Task<IActionResult> ImportDataAsync(IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest("File required");
    
    // Save file temporarily
    var tempPath = await _storage.SaveTempFileAsync(file);
    
    // Fire-and-forget: Process import in background
    var importId = await _importService.RegisterImportAsync(tempPath);
    
    _ = Task.Run(async () =>
    {
        try
        {
            var result = await _importService.ProcessImportAsync(importId);
            
            // Notify user when complete
            await _notificationHub.Clients.User(User.Identity.Name)
                .SendAsync("import-complete", new 
                { 
                    importId,
                    recordsImported = result.SuccessCount,
                    errors = result.ErrorCount
                });
        }
        catch (Exception ex)
        {
            _logger.Error($"Import failed: {ex}");
            await _importService.MarkAsFailedAsync(importId, ex.Message);
        }
        finally
        {
            // Cleanup temp file
            _ = _storage.DeleteAsync(tempPath).FireAndForget();
        }
    }).FireAndForget();
    
    // Return immediately
    return Accepted(new { importId });
}
```

**Why Fire-and-Forget:**
- Large file processing takes time
- Progress updates via WebSocket or polling
- Cleanup happens asynchronously
- User gets immediate confirmation

---

## 🔔 Notification Scenarios

### Use Case 6: Multi-Channel Notifications

**Scenario:** Send notifications via multiple channels

```csharp
public class NotificationService
{
    public async Task NotifyUserAsync(int userId, string message)
    {
        var user = await _userService.GetUserAsync(userId);
        
        // Send via multiple channels in parallel (fire-and-forget)
        var tasks = new[]
        {
            SendEmailAsync(user.Email, message),
            SendSmsAsync(user.Phone, message),
            SendPushNotificationAsync(user.DeviceTokens, message),
            WriteInAppNotificationAsync(userId, message),
            LogNotificationAsync(userId, "email", message)
        };
        
        // Fire all in parallel without waiting
        foreach (var task in tasks)
            _ = task.FireAndForget(ex => 
                _logger.Warn($"Notification channel failed: {ex?.Message}")
            );
    }
    
    private async Task SendEmailAsync(string email, string message)
    {
        try
        {
            await _emailClient.SendAsync(email, "Notification", message);
        }
        catch (Exception ex)
        {
            _logger.Warn($"Email failed for {email}: {ex.Message}");
        }
    }
    
    // ... other Send*Async methods with internal error handling
}
```

**Why Fire-and-Forget:**
- Multiple channels run in parallel
- No need to wait for all to complete
- Some channels might be slow (email)
- Partial failures acceptable (at least one channel succeeds)

---

### Use Case 7: Webhook Notifications

**Scenario:** Notify external systems of events

```csharp
[HttpPost("orders")]
public async Task<IActionResult> CreateOrderAsync(CreateOrderDto dto)
{
    // Create order in database
    var order = await _orderService.CreateAsync(dto);
    
    // Fire-and-forget: Notify external systems
    _ = NotifyExternalSystemsAsync(order).FireAndForget(ex =>
        _logger.Error("Failed to notify external systems", ex)
    );
    
    return CreatedAtAction(nameof(GetOrder), new { orderId = order.Id }, order);
}

private async Task NotifyExternalSystemsAsync(Order order)
{
    try
    {
        var tasks = new[]
        {
            // Notify inventory system
            _httpClient.PostAsync("https://inventory.example.com/webhook", 
                new { type = "order_created", orderId = order.Id }),
            
            // Notify fulfillment system
            _httpClient.PostAsync("https://fulfillment.example.com/webhook",
                new { type = "order_created", orderId = order.Id }),
            
            // Notify partner systems
            _httpClient.PostAsync("https://partners.example.com/webhook",
                new { type = "order_created", orderId = order.Id })
        };
        
        await Task.WhenAll(tasks);
    }
    catch (Exception ex)
    {
        // Log but don't throw - order was already created
        _logger.Error("Webhook notification failed", ex);
    }
}
```

**Why Fire-and-Forget:**
- External system notifications shouldn't delay order creation
- Multiple systems notified in parallel
- Retries can be handled separately

---

## 🏠 Infrastructure & Maintenance

### Use Case 8: Cache Invalidation

**Scenario:** Invalidate caches after data updates

```csharp
[HttpPut("users/{id}")]
public async Task<IActionResult> UpdateUserAsync(int id, UpdateUserDto dto)
{
    // Update user
    var updated = await _userService.UpdateAsync(id, dto);
    
    // Fire-and-forget: Invalidate caches
    // Cache invalidation is eventual - not critical to response
    _ = _cache.InvalidateAsync($"user:{id}")
        .FireAndForget(ex => _logger.Warn("Cache invalidation failed", ex));
    
    // Also invalidate related caches in parallel
    var cacheTasks = new[]
    {
        _cache.InvalidateAsync($"user:{id}:profile"),
        _cache.InvalidateAsync($"user:{id}:settings"),
        _cache.InvalidateAsync("users:list"),
        _redisCache.RemoveAsync($"user_{id}_data")
    };
    
    foreach (var task in cacheTasks)
        _ = task.FireAndForget();
    
    return Ok(updated);
}
```

**Why Fire-and-Forget:**
- Cache invalidation doesn't affect data correctness
- Can be eventual consistency
- Response shouldn't wait for cache operations
- Multiple caches invalidated in parallel

---

### Use Case 9: Database Maintenance

**Scenario:** Cleanup old data without blocking app

```csharp
[HttpDelete("temp-files")]
public IActionResult TriggerCleanup()
{
    // Fire-and-forget: Clean old temporary files
    _ = Task.Run(async () =>
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddDays(-7);
            var oldFiles = await _fileRepository.FindOldFilesAsync(cutoff);
            
            foreach (var file in oldFiles)
            {
                await _storage.DeleteAsync(file.Path);
                await _fileRepository.RemoveAsync(file.Id);
            }
            
            _logger.Info($"Cleanup completed: {oldFiles.Count} files deleted");
        }
        catch (Exception ex)
        {
            _logger.Error("Cleanup failed", ex);
        }
    }).FireAndForget();
    
    return Ok("Cleanup initiated");
}
```

**Why Fire-and-Forget:**
- Cleanup can take a long time
- Doesn't block responses
- Can run during off-peak hours if scheduled
- Failure is non-critical

---

## 🎯 Complex Workflows

### Use Case 10: Multi-Step Async Workflow

**Scenario:** Complex business process with multiple async steps

```csharp
[HttpPost("claims")]
public async Task<IActionResult> SubmitClaimAsync(ClaimRequest request)
{
    // Step 1: Create claim (must complete successfully)
    var claim = await _claimService.CreateAsync(request);
    
    // Step 2: Start background workflow (fire-and-forget)
    // This includes multiple steps that don't need to block response
    _ = ProcessClaimWorkflowAsync(claim).FireAndForget(ex =>
        _logger.Error("Claim processing workflow failed", ex)
    );
    
    // Return immediately
    return CreatedAtAction(nameof(GetClaim), new { claimId = claim.Id });
}

private async Task ProcessClaimWorkflowAsync(Claim claim)
{
    try
    {
        // Step 1: Validate claim with external service
        var validationResult = await _externalValidator.ValidateAsync(claim);
        if (!validationResult.IsValid)
        {
            await _claimService.MarkAsRejectedAsync(claim.Id, validationResult.Reason);
            return;
        }
        
        // Step 2: Run fraud detection
        var fraudScore = await _fraudDetectionService.AnalyzeAsync(claim);
        if (fraudScore > 0.8)
        {
            await _claimService.FlagForReviewAsync(claim.Id);
        }
        
        // Step 3: Notify relevant parties in parallel
        var tasks = new[]
        {
            _notificationService.NotifyClaimantAsync(claim),
            _notificationService.NotifyProcessorAsync(claim),
            _auditService.LogAsync("claim_submitted", claim.Id)
        };
        
        await Task.WhenAll(tasks);
        
        // Step 4: Schedule follow-up tasks
        _ = ScheduleFollowUpAsync(claim).FireAndForget();
        
    }
    catch (Exception ex)
    {
        _logger.Error($"Workflow failed for claim {claim.Id}", ex);
        await _claimService.MarkAsErrorAsync(claim.Id, ex.Message);
    }
}

private async Task ScheduleFollowUpAsync(Claim claim)
{
    var followUpDate = DateTime.UtcNow.AddDays(7);
    await _scheduler.ScheduleAsync(
        jobId: $"follow-up-{claim.Id}",
        executeAt: followUpDate,
        jobData: new { claimId = claim.Id }
    );
}
```

**Why Fire-and-Forget:**
- Complex workflows shouldn't block API responses
- Multiple async operations run in parallel
- Each step has proper error handling
- User gets immediate confirmation

---

## 📈 Best Practices Summary by Use Case

| Use Case | Priority | Pattern | Error Handling |
|----------|----------|---------|----------------|
| Email sending | Low | Fire-and-Forget | Log, retry later |
| Cache invalidation | Very Low | Fire-and-Forget | Log only |
| Analytics | Very Low | Fire-and-Forget | Silent failure |
| Logging | Very Low | Fire-and-Forget | Silent failure |
| Report generation | Low | Fire-and-Forget + polling | Log, store error |
| Webhooks | Medium | Fire-and-Forget | Log, retry logic |
| Import/Export | Medium | Fire-and-Forget + progress | Log, user notification |
| Cleanup | Very Low | Fire-and-Forget | Log only |
| Notifications | Low | Fire-and-Forget | Log, try alternate channel |

---

## ⚠️ When NOT to Use Fire-and-Forget

❌ **Don't Use:**
- Database operations where you need to validate the result
- API calls where the response is needed
- Critical business logic
- Security-critical operations
- Payment processing
- Authorization checks
- Data that must be consistent

✅ **Consider Sequential (Await):**
- Critical path operations
- Operations that affect response content
- Data validation
- Error conditions that must propagate

---

## 🎓 Summary

Fire-and-forget is perfect for:
1. **Non-blocking notifications** (email, SMS, push)
2. **Non-critical operations** (logging, analytics)
3. **Eventual consistency scenarios** (cache invalidation)
4. **Background tasks** (reports, imports, cleanup)
5. **External system notifications** (webhooks, events)

Always remember:
- ✅ Handle exceptions somehow
- ✅ Log failures appropriately
- ✅ Make the async nature clear
- ✅ Never use for critical operations
- ❌ Never use async void
- ❌ Never ignore all exceptions
