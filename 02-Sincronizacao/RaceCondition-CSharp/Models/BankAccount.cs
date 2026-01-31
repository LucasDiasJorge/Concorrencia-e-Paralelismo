namespace RaceCondition.Models;

/// <summary>
/// Representa uma conta bancária com saldo compartilhado entre múltiplas threads.
/// Este modelo é usado para demonstrar race conditions em operações financeiras.
/// </summary>
public class BankAccount
{
    private decimal _balance;
    private readonly object _lockObject = new object();

    /// <summary>
    /// Obtém o saldo atual da conta.
    /// </summary>
    public decimal Balance => _balance;

    /// <summary>
    /// Inicializa uma nova conta bancária com saldo inicial.
    /// </summary>
    /// <param name="initialBalance">Saldo inicial da conta.</param>
    public BankAccount(decimal initialBalance)
    {
        _balance = initialBalance;
    }

    /// <summary>
    /// Deposita um valor na conta - VERSÃO COM RACE CONDITION.
    /// Esta implementação é INSEGURA para uso concorrente!
    /// </summary>
    /// <param name="amount">Valor a ser depositado.</param>
    public void DepositUnsafe(decimal amount)
    {
        // PROBLEMA: Estas 3 operações não são atômicas
        decimal currentBalance = _balance;           // 1. Lê o valor
        Thread.Sleep(1);                             // Simula processamento
        decimal newBalance = currentBalance + amount; // 2. Calcula novo valor
        Thread.Sleep(1);                             // Simula processamento
        _balance = newBalance;                       // 3. Escreve o valor
    }

    /// <summary>
    /// Saca um valor da conta - VERSÃO COM RACE CONDITION.
    /// Esta implementação é INSEGURA para uso concorrente!
    /// </summary>
    /// <param name="amount">Valor a ser sacado.</param>
    /// <returns>True se o saque foi realizado, False se não há saldo suficiente.</returns>
    public bool WithdrawUnsafe(decimal amount)
    {
        // PROBLEMA: Check-Then-Act não é atômico
        if (_balance >= amount)
        {
            Thread.Sleep(1); // Simula processamento
            _balance -= amount;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Deposita um valor na conta - VERSÃO SEGURA COM LOCK.
    /// Esta implementação é SEGURA para uso concorrente.
    /// </summary>
    /// <param name="amount">Valor a ser depositado.</param>
    public void DepositSafe(decimal amount)
    {
        lock (_lockObject)
        {
            decimal currentBalance = _balance;
            Thread.Sleep(1);
            decimal newBalance = currentBalance + amount;
            Thread.Sleep(1);
            _balance = newBalance;
        }
    }

    /// <summary>
    /// Saca um valor da conta - VERSÃO SEGURA COM LOCK.
    /// Esta implementação é SEGURA para uso concorrente.
    /// </summary>
    /// <param name="amount">Valor a ser sacado.</param>
    /// <returns>True se o saque foi realizado, False se não há saldo suficiente.</returns>
    public bool WithdrawSafe(decimal amount)
    {
        lock (_lockObject)
        {
            if (_balance >= amount)
            {
                Thread.Sleep(1);
                _balance -= amount;
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Reseta o saldo da conta.
    /// </summary>
    /// <param name="newBalance">Novo saldo da conta.</param>
    public void Reset(decimal newBalance)
    {
        lock (_lockObject)
        {
            _balance = newBalance;
        }
    }
}
