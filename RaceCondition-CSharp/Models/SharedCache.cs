using System.Collections.Concurrent;

namespace RaceCondition.Models;

/// <summary>
/// Representa um cache compartilhado entre múltiplas threads.
/// Demonstra race conditions em operações de leitura/escrita de cache.
/// </summary>
/// <typeparam name="TKey">Tipo da chave do cache.</typeparam>
/// <typeparam name="TValue">Tipo do valor armazenado no cache.</typeparam>
public class SharedCache<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _cacheUnsafe = new();
    private readonly Dictionary<TKey, TValue> _cacheWithLock = new();
    private readonly ConcurrentDictionary<TKey, TValue> _cacheThreadSafe = new();
    private readonly ReaderWriterLockSlim _readerWriterLock = new();
    private readonly object _lockObject = new object();

    /// <summary>
    /// Obtém o número de itens no cache não seguro.
    /// </summary>
    public int UnsafeCount => _cacheUnsafe.Count;

    /// <summary>
    /// Obtém o número de itens no cache com lock.
    /// </summary>
    public int LockCount => _cacheWithLock.Count;

    /// <summary>
    /// Obtém o número de itens no cache thread-safe.
    /// </summary>
    public int ThreadSafeCount => _cacheThreadSafe.Count;

    #region Unsafe Operations

    /// <summary>
    /// Adiciona um item ao cache - VERSÃO INSEGURA.
    /// PROBLEMA: Dictionary não é thread-safe!
    /// </summary>
    public void AddUnsafe(TKey key, TValue value)
    {
        if (!_cacheUnsafe.ContainsKey(key))
        {
            Thread.Sleep(1); // Simula processamento
            _cacheUnsafe.Add(key, value);
        }
    }

    /// <summary>
    /// Obtém um item do cache - VERSÃO INSEGURA.
    /// PROBLEMA: Pode lançar exceção se outra thread modificar o dicionário.
    /// </summary>
    public TValue? GetUnsafe(TKey key)
    {
        if (_cacheUnsafe.ContainsKey(key))
        {
            return _cacheUnsafe[key];
        }
        return default;
    }

    #endregion

    #region Lock-Based Operations

    /// <summary>
    /// Adiciona um item ao cache - VERSÃO SEGURA COM LOCK.
    /// Lock exclusivo para leitura e escrita.
    /// Overhead: Médio (~50-100ns).
    /// </summary>
    public void AddWithLock(TKey key, TValue value)
    {
        lock (_lockObject)
        {
            if (!_cacheWithLock.ContainsKey(key))
            {
                Thread.Sleep(1);
                _cacheWithLock.Add(key, value);
            }
        }
    }

    /// <summary>
    /// Obtém um item do cache - VERSÃO SEGURA COM LOCK.
    /// </summary>
    public TValue? GetWithLock(TKey key)
    {
        lock (_lockObject)
        {
            if (_cacheWithLock.ContainsKey(key))
            {
                return _cacheWithLock[key];
            }
            return default;
        }
    }

    #endregion

    #region ReaderWriterLock Operations

    /// <summary>
    /// Adiciona um item ao cache usando ReaderWriterLockSlim.
    /// Permite múltiplas leituras simultâneas, mas escrita exclusiva.
    /// IDEAL para cenários com muitas leituras e poucas escritas.
    /// </summary>
    public void AddWithReaderWriterLock(TKey key, TValue value)
    {
        _readerWriterLock.EnterWriteLock();
        try
        {
            if (!_cacheWithLock.ContainsKey(key))
            {
                Thread.Sleep(1);
                _cacheWithLock[key] = value;
            }
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Obtém um item do cache usando ReaderWriterLockSlim.
    /// Múltiplas threads podem ler simultaneamente.
    /// </summary>
    public TValue? GetWithReaderWriterLock(TKey key)
    {
        _readerWriterLock.EnterReadLock();
        try
        {
            if (_cacheWithLock.ContainsKey(key))
            {
                return _cacheWithLock[key];
            }
            return default;
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
    }

    #endregion

    #region ConcurrentDictionary Operations

    /// <summary>
    /// Adiciona um item ao cache - VERSÃO COM CONCURRENTDICTIONARY.
    /// Thread-safe por design, sem necessidade de locks manuais.
    /// Overhead: Baixo (~20-30ns).
    /// RECOMENDADO para a maioria dos casos!
    /// </summary>
    public void AddThreadSafe(TKey key, TValue value)
    {
        _cacheThreadSafe.TryAdd(key, value);
    }

    /// <summary>
    /// Obtém um item do cache - VERSÃO COM CONCURRENTDICTIONARY.
    /// Thread-safe e eficiente.
    /// </summary>
    public TValue? GetThreadSafe(TKey key)
    {
        _cacheThreadSafe.TryGetValue(key, out TValue? value);
        return value;
    }

    /// <summary>
    /// Obtém ou adiciona um item ao cache de forma atômica.
    /// Se a chave não existe, cria o valor usando a factory.
    /// </summary>
    public TValue GetOrAddThreadSafe(TKey key, Func<TKey, TValue> valueFactory)
    {
        return _cacheThreadSafe.GetOrAdd(key, valueFactory);
    }

    #endregion

    /// <summary>
    /// Limpa todos os caches.
    /// </summary>
    public void ClearAll()
    {
        lock (_lockObject)
        {
            _cacheUnsafe.Clear();
            _cacheWithLock.Clear();
        }
        _cacheThreadSafe.Clear();
    }

    /// <summary>
    /// Libera recursos do ReaderWriterLockSlim.
    /// </summary>
    public void Dispose()
    {
        _readerWriterLock?.Dispose();
    }
}
