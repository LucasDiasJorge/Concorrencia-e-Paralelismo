using System.Threading;

namespace AtomicSequenceSafe;

public sealed class AtomicCounter : ICounter
{
    private long _value;

    public AtomicCounter()
    {
        _value = 0L;
    }

    public long Next()
    {
        return Interlocked.Increment(ref _value);
    }

    public long Value
    {
        get
        {
            return Interlocked.Read(ref _value);
        }
    }
}
