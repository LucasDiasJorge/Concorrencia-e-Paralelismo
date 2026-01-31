using RaceCondition.Models;

namespace RaceCondition.Examples;

/// <summary>
/// Demonstra race condition em operações bancárias.
/// Cenário: Múltiplos depósitos simultâneos causam perda de dados.
/// </summary>
public static class BankAccountRaceCondition
{
    /// <summary>
    /// Executa demonstração de race condition em conta bancária.
    /// </summary>
    public static void RunDemo()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("EXEMPLO 1: RACE CONDITION EM CONTA BANCÁRIA");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📌 CENÁRIO:");
        Console.WriteLine("   - Saldo inicial: R$ 1.000,00");
        Console.WriteLine("   - 10 threads fazendo 100 depósitos de R$ 1,00 cada");
        Console.WriteLine("   - Total esperado: R$ 1.000,00 + R$ 1.000,00 = R$ 2.000,00");

        // Teste com versão INSEGURA
        Console.WriteLine("\n❌ VERSÃO INSEGURA (COM RACE CONDITION):");
        BankAccount unsafeAccount = new BankAccount(1000m);
        RunConcurrentDeposits(unsafeAccount, isThreadSafe: false);

        decimal unsafeBalance = unsafeAccount.Balance;
        Console.WriteLine($"   Saldo final: R$ {unsafeBalance:N2}");
        Console.WriteLine($"   Perda de dados: R$ {2000m - unsafeBalance:N2}");
        Console.WriteLine($"   Precisão: {(unsafeBalance / 2000m) * 100:F2}%");

        // Teste com versão SEGURA
        Console.WriteLine("\n✅ VERSÃO SEGURA (COM LOCK):");
        BankAccount safeAccount = new BankAccount(1000m);
        RunConcurrentDeposits(safeAccount, isThreadSafe: true);

        decimal safeBalance = safeAccount.Balance;
        Console.WriteLine($"   Saldo final: R$ {safeBalance:N2}");
        Console.WriteLine($"   Perda de dados: R$ {2000m - safeBalance:N2}");
        Console.WriteLine($"   Precisão: {(safeBalance / 2000m) * 100:F2}%");

        // Explicação técnica
        Console.WriteLine("\n📚 EXPLICAÇÃO TÉCNICA:");
        Console.WriteLine("   Race condition ocorre porque a operação de depósito envolve:");
        Console.WriteLine("   1. Ler saldo atual");
        Console.WriteLine("   2. Calcular novo saldo");
        Console.WriteLine("   3. Escrever novo saldo");
        Console.WriteLine("\n   Se duas threads executam simultaneamente:");
        Console.WriteLine("   Thread A: Lê 1000 → Calcula 1001 → Escreve 1001");
        Console.WriteLine("   Thread B: Lê 1000 → Calcula 1001 → Escreve 1001");
        Console.WriteLine("   Resultado: 1001 (esperado: 1002) ❌");
        Console.WriteLine("\n   SOLUÇÃO: Use lock para garantir atomicidade.");
    }

    /// <summary>
    /// Executa depósitos concorrentes em uma conta bancária.
    /// </summary>
    /// <param name="account">Conta bancária para realizar os depósitos.</param>
    /// <param name="isThreadSafe">Se true, usa versão segura; caso contrário, usa versão insegura.</param>
    private static void RunConcurrentDeposits(BankAccount account, bool isThreadSafe)
    {
        const int numberOfThreads = 10;
        const int depositsPerThread = 100;
        const decimal depositAmount = 1m;

        Thread[] threads = new Thread[numberOfThreads];

        DateTime startTime = DateTime.Now;

        for (int i = 0; i < numberOfThreads; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < depositsPerThread; j++)
                {
                    if (isThreadSafe)
                    {
                        account.DepositSafe(depositAmount);
                    }
                    else
                    {
                        account.DepositUnsafe(depositAmount);
                    }
                }
            });
            threads[i].Start();
        }

        // Aguarda todas as threads terminarem
        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        TimeSpan elapsed = DateTime.Now - startTime;
        Console.WriteLine($"   Tempo de execução: {elapsed.TotalMilliseconds:F2}ms");
    }

    /// <summary>
    /// Demonstra race condition com saques simultâneos.
    /// Cenário: Múltiplos saques podem deixar o saldo negativo.
    /// </summary>
    public static void RunWithdrawalRaceCondition()
    {
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("RACE CONDITION: SAQUES SIMULTÂNEOS");
        Console.WriteLine(new string('=', 80));

        Console.WriteLine("\n📌 CENÁRIO:");
        Console.WriteLine("   - Saldo inicial: R$ 100,00");
        Console.WriteLine("   - 5 threads tentando sacar R$ 30,00 simultaneamente");
        Console.WriteLine("   - Apenas 3 saques deveriam ter sucesso");

        // Versão INSEGURA
        Console.WriteLine("\n❌ VERSÃO INSEGURA:");
        BankAccount unsafeAccount = new BankAccount(100m);
        int unsafeSuccessfulWithdrawals = RunConcurrentWithdrawals(unsafeAccount, false);
        Console.WriteLine($"   Saques realizados: {unsafeSuccessfulWithdrawals}");
        Console.WriteLine($"   Saldo final: R$ {unsafeAccount.Balance:N2}");
        
        if (unsafeAccount.Balance < 0)
        {
            Console.WriteLine($"   ⚠️  SALDO NEGATIVO! Overdraft de R$ {Math.Abs(unsafeAccount.Balance):N2}");
        }

        // Versão SEGURA
        Console.WriteLine("\n✅ VERSÃO SEGURA:");
        BankAccount safeAccount = new BankAccount(100m);
        int safeSuccessfulWithdrawals = RunConcurrentWithdrawals(safeAccount, true);
        Console.WriteLine($"   Saques realizados: {safeSuccessfulWithdrawals}");
        Console.WriteLine($"   Saldo final: R$ {safeAccount.Balance:N2}");

        Console.WriteLine("\n📚 EXPLICAÇÃO:");
        Console.WriteLine("   Time-of-Check to Time-of-Use (TOCTOU) vulnerability:");
        Console.WriteLine("   1. Thread A verifica saldo >= 30 ✓");
        Console.WriteLine("   2. Thread B verifica saldo >= 30 ✓");
        Console.WriteLine("   3. Thread A saca 30");
        Console.WriteLine("   4. Thread B saca 30");
        Console.WriteLine("   Resultado: Saldo negativo! ❌");
    }

    /// <summary>
    /// Executa saques concorrentes.
    /// </summary>
    private static int RunConcurrentWithdrawals(BankAccount account, bool isThreadSafe)
    {
        const int numberOfThreads = 5;
        const decimal withdrawAmount = 30m;
        int successCount = 0;
        object countLock = new object();

        Thread[] threads = new Thread[numberOfThreads];

        for (int i = 0; i < numberOfThreads; i++)
        {
            threads[i] = new Thread(() =>
            {
                bool success = isThreadSafe 
                    ? account.WithdrawSafe(withdrawAmount)
                    : account.WithdrawUnsafe(withdrawAmount);

                if (success)
                {
                    lock (countLock)
                    {
                        successCount++;
                    }
                }
            });
            threads[i].Start();
        }

        foreach (Thread thread in threads)
        {
            thread.Join();
        }

        return successCount;
    }
}
