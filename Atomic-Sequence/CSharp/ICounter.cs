namespace AtomicSequenceSafe;

public interface ICounter
{
    long Next();
    long Value { get; }
}
