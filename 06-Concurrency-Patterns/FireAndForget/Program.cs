using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Fire and Forget Pattern in C#
/// Demonstrates async operations that don't require waiting for completion
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Fire and Forget Pattern in C# ===\n");
        
        Dictionary<string, Func<Task>> menu = new Dictionary<string, Func<Task>>
        {
            ["1"] = () => RunExample<FireAndForgetBasicExample>("Basic Fire and Forget"),
            ["2"] = () => RunExample<FireAndForgetWithErrorHandlingExample>("Fire and Forget with Error Handling"),
            ["3"] = () => RunExample<FireAndForgetWithTrackingExample>("Fire and Forget with Tracking"),
            ["4"] = () => RunExample<FireAndForgetVsTaskRunExample>("Fire and Forget vs Task.Run"),
            ["5"] = () => RunExample<FireAndForgetBestPracticesExample>("Best Practices & Anti-Patterns"),
        };

        while (true)
        {
            Console.WriteLine("\n📋 Fire and Forget Examples:");
            Console.WriteLine("1. Basic Fire and Forget");
            Console.WriteLine("2. Fire and Forget with Error Handling");
            Console.WriteLine("3. Fire and Forget with Tracking");
            Console.WriteLine("4. Fire and Forget vs Task.Run");
            Console.WriteLine("5. Best Practices & Anti-Patterns");
            Console.WriteLine("0. Exit");
            
            Console.Write("\nSelect an example (0-5): ");
            string choice = Console.ReadLine() ?? "0";

            if (choice == "0")
                break;

            if (menu.TryGetValue(choice, out Func<Task> example))
            {
                await example();
            }
            else
            {
                Console.WriteLine("❌ Invalid choice");
            }
        }
    }

    static async Task RunExample<T>(string title) where T : IExample, new()
    {
        Console.WriteLine($"\n{'=', -50}");
        Console.WriteLine($"📌 {title}");
        Console.WriteLine($"{'=', -50}\n");
        
        T example = new T();
        await example.Run();
    }
}

interface IExample
{
    Task Run();
}

/// <summary>
/// Fire-and-Forget Extension Methods
/// These are used across all examples to safely handle background operations.
/// </summary>
static class FireAndForgetExtensions
{
    public static void FireAndForget(
        this Task task,
        Action<Exception?>? errorHandler = null)
    {
        _ = task.ContinueWith(t =>
        {
                if (t.IsFaulted)
                {
                    Exception? ex = t.Exception?.GetBaseException();
                    errorHandler?.Invoke(ex);
                }
        }, TaskScheduler.Default);
    }
    
    public static void FireAndForget<T>(
        this Task<T> task,
        Action<T?>? successHandler = null,
        Action<Exception?>? errorHandler = null)
    {
        _ = task.ContinueWith(t =>
        {
            if (t.IsFaulted)
            {
                Exception? ex = t.Exception?.GetBaseException();
                errorHandler?.Invoke(ex);
            }
            else if (t.IsCompletedSuccessfully)
            {
                successHandler?.Invoke(t.Result);
            }
        }, TaskScheduler.Default);
    }
}
