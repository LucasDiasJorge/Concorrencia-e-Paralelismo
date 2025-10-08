using System;
using System.Diagnostics;
using System.Threading;

namespace AtomicSequenceSafe;

public sealed class CounterRunner
{
    private readonly ICounter _counter;
    private readonly int _threadsCount;
    private readonly int _incrementsPerThread;

    public CounterRunner(ICounter counter, int threadsCount, int incrementsPerThread)
    {
        _counter = counter ?? throw new ArgumentNullException(nameof(counter));
        _threadsCount = threadsCount;
        _incrementsPerThread = incrementsPerThread;
    }

    public TimeSpan Run()
    {
        Thread[] workers = new Thread[_threadsCount];
        Stopwatch sw = Stopwatch.StartNew();

        for (int i = 0; i < _threadsCount; i++)
        {
            workers[i] = new Thread(() =>
            {
                for (int j = 0; j < _incrementsPerThread; j++)
                {
                    _ = _counter.Next();
                }
            }) { IsBackground = true };
        }

        for (int i = 0; i < _threadsCount; i++) workers[i].Start();
        for (int i = 0; i < _threadsCount; i++) workers[i].Join();

        sw.Stop();
        return sw.Elapsed;
    }
}
