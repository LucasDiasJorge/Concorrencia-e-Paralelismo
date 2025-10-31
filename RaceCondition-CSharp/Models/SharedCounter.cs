namespace RaceCondition.Models;

/// <summary>
/// Representa um contador compartilhado entre múltiplas threads.
/// Demonstra diferentes técnicas de sincronização para operações de incremento.
/// </summary>
public class SharedCounter
{
    private int _counterUnsafe;
    private int _counterWithLock;
    private int _counterWithInterlocked;
    private readonly object _lockObject = new object();

    /// <summary>
    /// Obtém o valor do contador não seguro (sujeito a race condition).
    /// </summary>
    public int CounterUnsafe => _counterUnsafe;

    /// <summary>
    /// Obtém o valor do contador protegido com lock.
    /// </summary>
    public int CounterWithLock => _counterWithLock;

    /// <summary>
    /// Obtém o valor do contador usando Interlocked.
    /// </summary>
    public int CounterWithInterlocked => _counterWithInterlocked;

    /// <summary>
    /// Incrementa o contador - VERSÃO INSEGURA COM RACE CONDITION.
    /// A operação counter++ não é atômica e consiste em:
    /// 1. Ler valor atual
    /// 2. Incrementar
    /// 3. Escrever novo valor
    /// </summary>
    public void IncrementUnsafe()
    {
        // PROBLEMA: Esta operação não é atômica!
        _counterUnsafe++;
        // Equivalente a:
        // int temp = _counterUnsafe;
        // temp = temp + 1;
        // _counterUnsafe = temp;
    }

    /// <summary>
    /// Incrementa o contador - VERSÃO SEGURA COM LOCK.
    /// O lock garante que apenas uma thread execute o código por vez.
    /// Overhead: ~25-50ns por operação.
    /// </summary>
    public void IncrementWithLock()
    {
        lock (_lockObject)
        {
            _counterWithLock++;
        }
    }

    /// <summary>
    /// Incrementa o contador - VERSÃO SEGURA COM INTERLOCKED.
    /// Usa operações atômicas do processador (CPU-level).
    /// Overhead: ~5-10ns por operação.
    /// MAIS RÁPIDO que lock para operações simples!
    /// </summary>
    public void IncrementWithInterlocked()
    {
        Interlocked.Increment(ref _counterWithInterlocked);
    }

    /// <summary>
    /// Decrementa o contador usando Interlocked.
    /// </summary>
    public void DecrementWithInterlocked()
    {
        Interlocked.Decrement(ref _counterWithInterlocked);
    }

    /// <summary>
    /// Adiciona um valor ao contador usando Interlocked.
    /// </summary>
    /// <param name="value">Valor a ser adicionado.</param>
    /// <returns>O novo valor do contador após a adição.</returns>
    public int AddWithInterlocked(int value)
    {
        return Interlocked.Add(ref _counterWithInterlocked, value);
    }

    /// <summary>
    /// Compara e troca o valor do contador (Compare-And-Swap).
    /// Operação atômica fundamental em programação lock-free.
    /// </summary>
    /// <param name="comparand">Valor esperado.</param>
    /// <param name="newValue">Novo valor se a comparação for bem-sucedida.</param>
    /// <returns>O valor original do contador.</returns>
    public int CompareExchangeWithInterlocked(int comparand, int newValue)
    {
        return Interlocked.CompareExchange(ref _counterWithInterlocked, newValue, comparand);
    }

    /// <summary>
    /// Reseta todos os contadores para zero.
    /// </summary>
    public void ResetAll()
    {
        _counterUnsafe = 0;
        _counterWithLock = 0;
        Interlocked.Exchange(ref _counterWithInterlocked, 0);
    }

    /// <summary>
    /// Obtém um resumo dos valores de todos os contadores.
    /// </summary>
    /// <returns>String formatada com os valores.</returns>
    public string GetSummary()
    {
        return $"Unsafe: {_counterUnsafe:N0}, Lock: {_counterWithLock:N0}, Interlocked: {_counterWithInterlocked:N0}";
    }
}
