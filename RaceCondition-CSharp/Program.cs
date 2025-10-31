using RaceCondition.Examples;
using RaceCondition.Solutions;

namespace RaceCondition;

/// <summary>
/// Programa principal que demonstra race conditions e suas soluções.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        
        PrintHeader();
        
        bool running = true;

        while (running)
        {
            PrintMenu();
            ConsoleKeyInfo key = Console.ReadKey(true);

            Console.Clear();
            PrintHeader();

            switch (key.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    BankAccountRaceCondition.RunDemo();
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    CounterRaceCondition.RunDemo();
                    break;

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    ListRaceCondition.RunDemo();
                    break;

                case ConsoleKey.D4:
                case ConsoleKey.NumPad4:
                    CacheRaceCondition.RunDemo();
                    break;

                case ConsoleKey.D5:
                case ConsoleKey.NumPad5:
                    BankAccountRaceCondition.RunWithdrawalRaceCondition();
                    break;

                case ConsoleKey.D6:
                case ConsoleKey.NumPad6:
                    CounterRaceCondition.DemonstrateCompareExchange();
                    break;

                case ConsoleKey.L:
                    LockSolution.RunDemo();
                    break;

                case ConsoleKey.I:
                    InterlockedSolution.RunDemo();
                    break;

                case ConsoleKey.S:
                    SemaphoreSolution.RunDemo();
                    break;

                case ConsoleKey.R:
                    ReaderWriterLockSolution.RunDemo();
                    break;

                case ConsoleKey.C:
                    ConcurrentCollectionsSolution.RunDemo();
                    break;

                case ConsoleKey.M:
                    MonitorSolution.RunDemo();
                    break;

                case ConsoleKey.A:
                    RunAllExamples();
                    break;

                case ConsoleKey.H:
                    ShowHelp();
                    break;

                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                    running = false;
                    Console.WriteLine("\n👋 Até logo!\n");
                    break;

                default:
                    Console.WriteLine("\n❌ Opção inválida! Pressione H para ajuda.");
                    break;
            }

            if (running)
            {
                Console.WriteLine("\n" + new string('=', 80));
                Console.WriteLine("Pressione qualquer tecla para continuar...");
                Console.ReadKey(true);
                Console.Clear();
                PrintHeader();
            }
        }
    }

    /// <summary>
    /// Imprime o cabeçalho do programa.
    /// </summary>
    private static void PrintHeader()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(@"
╔════════════════════════════════════════════════════════════════════════════════╗
║                                                                                ║
║                    🔒 RACE CONDITION EM C# - GUIA COMPLETO                    ║
║                                                                                ║
║              Demonstrações práticas e soluções otimizadas para                ║
║                    problemas de concorrência em .NET                          ║
║                                                                                ║
╚════════════════════════════════════════════════════════════════════════════════╝
");
        Console.ResetColor();
    }

    /// <summary>
    /// Imprime o menu principal.
    /// </summary>
    private static void PrintMenu()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n┌─────────────────────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│                          📋 EXEMPLOS DE RACE CONDITION                      │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────────────────────┘");
        Console.ResetColor();

        Console.WriteLine("  [1] 🏦 Conta Bancária - Depósitos simultâneos");
        Console.WriteLine("  [2] 🔢 Contador - Incrementos perdidos");
        Console.WriteLine("  [3] 📝 Listas/Coleções - Modificações concorrentes");
        Console.WriteLine("  [4] 💾 Cache - Leitura/Escrita simultânea");
        Console.WriteLine("  [5] 💰 Saques Simultâneos - TOCTOU vulnerability");
        Console.WriteLine("  [6] 🔄 Compare-And-Swap - Lock-free programming");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n┌─────────────────────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│                          ✅ SOLUÇÕES E TÉCNICAS                              │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────────────────────┘");
        Console.ResetColor();

        Console.WriteLine("  [L] 🔐 Lock - Exclusão mútua simples");
        Console.WriteLine("  [I] ⚡ Interlocked - Operações atômicas (MAIS RÁPIDO)");
        Console.WriteLine("  [S] 🚦 Semaphore - Controle de concorrência");
        Console.WriteLine("  [R] 📖 ReaderWriterLock - Otimizado para leitura");
        Console.WriteLine("  [C] 📦 Concurrent Collections - Thread-safe por design");
        Console.WriteLine("  [M] 🎛️  Monitor - Wait/Pulse avançado");

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n┌─────────────────────────────────────────────────────────────────────────────┐");
        Console.WriteLine("│                              ⚙️  UTILITÁRIOS                                 │");
        Console.WriteLine("└─────────────────────────────────────────────────────────────────────────────┘");
        Console.ResetColor();

        Console.WriteLine("  [A] 🎯 Executar TODOS os exemplos");
        Console.WriteLine("  [H] ❓ Ajuda e informações");
        Console.WriteLine("  [Q] 🚪 Sair");

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("\n" + new string('─', 80));
        Console.ResetColor();
        Console.Write("Escolha uma opção: ");
    }

    /// <summary>
    /// Executa todos os exemplos sequencialmente.
    /// </summary>
    private static void RunAllExamples()
    {
        Console.WriteLine("\n🎯 EXECUTANDO TODOS OS EXEMPLOS...\n");

        // Exemplos de problemas
        BankAccountRaceCondition.RunDemo();
        Console.WriteLine("\n\nPressione qualquer tecla para continuar...");
        Console.ReadKey(true);
        Console.Clear();
        PrintHeader();

        CounterRaceCondition.RunDemo();
        Console.WriteLine("\n\nPressione qualquer tecla para continuar...");
        Console.ReadKey(true);
        Console.Clear();
        PrintHeader();

        ListRaceCondition.RunDemo();
        Console.WriteLine("\n\nPressione qualquer tecla para continuar...");
        Console.ReadKey(true);
        Console.Clear();
        PrintHeader();

        CacheRaceCondition.RunDemo();
        Console.WriteLine("\n\nPressione qualquer tecla para continuar...");
        Console.ReadKey(true);
        Console.Clear();
        PrintHeader();

        // Soluções
        LockSolution.RunDemo();
        Console.WriteLine("\n\nPressione qualquer tecla para continuar...");
        Console.ReadKey(true);
        Console.Clear();
        PrintHeader();

        InterlockedSolution.RunDemo();
        Console.WriteLine("\n\nPressione qualquer tecla para continuar...");
        Console.ReadKey(true);
        Console.Clear();
        PrintHeader();

        SemaphoreSolution.RunDemo();
        Console.WriteLine("\n\nPressione qualquer tecla para continuar...");
        Console.ReadKey(true);
        Console.Clear();
        PrintHeader();

        ReaderWriterLockSolution.RunDemo();
        Console.WriteLine("\n\nPressione qualquer tecla para continuar...");
        Console.ReadKey(true);
        Console.Clear();
        PrintHeader();

        ConcurrentCollectionsSolution.RunDemo();
        Console.WriteLine("\n\nPressione qualquer tecla para continuar...");
        Console.ReadKey(true);
        Console.Clear();
        PrintHeader();

        MonitorSolution.RunDemo();

        Console.WriteLine("\n\n" + new string('=', 80));
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ TODOS OS EXEMPLOS FORAM EXECUTADOS COM SUCESSO!");
        Console.ResetColor();
    }

    /// <summary>
    /// Exibe informações de ajuda.
    /// </summary>
    private static void ShowHelp()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n" + new string('=', 80));
        Console.WriteLine("📖 AJUDA - RACE CONDITION EM C#");
        Console.WriteLine(new string('=', 80));
        Console.ResetColor();

        Console.WriteLine("\n🎯 SOBRE ESTE PROJETO:");
        Console.WriteLine("   Este programa demonstra problemas de race condition e como resolvê-los");
        Console.WriteLine("   de forma eficiente em C#, sem perder performance.");

        Console.WriteLine("\n📚 O QUE VOCÊ VAI APRENDER:");
        Console.WriteLine("   ✓ O que é race condition e por que acontece");
        Console.WriteLine("   ✓ Como detectar race conditions no seu código");
        Console.WriteLine("   ✓ Diferentes técnicas de sincronização");
        Console.WriteLine("   ✓ Trade-offs de performance entre soluções");
        Console.WriteLine("   ✓ Quando usar cada técnica");

        Console.WriteLine("\n🔍 TÉCNICAS DEMONSTRADAS:");
        Console.WriteLine("\n   1. LOCK (Monitor)");
        Console.WriteLine("      - Mais simples e comum");
        Console.WriteLine("      - Exclusão mútua básica");
        Console.WriteLine("      - Bom para seções críticas curtas");

        Console.WriteLine("\n   2. INTERLOCKED");
        Console.WriteLine("      - Operações atômicas em hardware");
        Console.WriteLine("      - 5-10x mais rápido que lock");
        Console.WriteLine("      - Ideal para operações simples (contador, flags)");

        Console.WriteLine("\n   3. SEMAPHORE");
        Console.WriteLine("      - Controla número de threads simultâneas");
        Console.WriteLine("      - Rate limiting, connection pools");
        Console.WriteLine("      - Throttling de recursos");

        Console.WriteLine("\n   4. READERWRITERLOCKSLIM");
        Console.WriteLine("      - Múltiplas leituras simultâneas");
        Console.WriteLine("      - Escrita exclusiva");
        Console.WriteLine("      - Ótimo para caches (80%+ leituras)");

        Console.WriteLine("\n   5. CONCURRENT COLLECTIONS");
        Console.WriteLine("      - ConcurrentDictionary, ConcurrentBag, etc.");
        Console.WriteLine("      - Thread-safe por design");
        Console.WriteLine("      - Sem necessidade de locks manuais");

        Console.WriteLine("\n💡 DICAS RÁPIDAS:");
        Console.WriteLine("   • Comece simples: Use lock para casos gerais");
        Console.WriteLine("   • Otimize depois: Profile antes de otimizar");
        Console.WriteLine("   • Prefira Interlocked para contadores");
        Console.WriteLine("   • Use ConcurrentCollections quando possível");
        Console.WriteLine("   • Mantenha seções críticas pequenas");
        Console.WriteLine("   • Evite operações longas (I/O) dentro de locks");

        Console.WriteLine("\n📊 QUANDO USAR CADA SOLUÇÃO:");
        Console.WriteLine("\n   ┌──────────────────────┬─────────────────────────────┐");
        Console.WriteLine("   │ Cenário              │ Solução Recomendada         │");
        Console.WriteLine("   ├──────────────────────┼─────────────────────────────┤");
        Console.WriteLine("   │ Contador simples     │ Interlocked.Increment       │");
        Console.WriteLine("   │ Seção crítica        │ lock { }                    │");
        Console.WriteLine("   │ Cache read-heavy     │ ReaderWriterLockSlim        │");
        Console.WriteLine("   │ Coleções             │ ConcurrentCollections       │");
        Console.WriteLine("   │ Rate limiting        │ SemaphoreSlim               │");
        Console.WriteLine("   │ Pool de recursos     │ SemaphoreSlim               │");
        Console.WriteLine("   └──────────────────────┴─────────────────────────────┘");

        Console.WriteLine("\n🔗 RECURSOS ADICIONAIS:");
        Console.WriteLine("   • Microsoft Docs: Threading in C#");
        Console.WriteLine("   • CLR via C# (Jeffrey Richter)");
        Console.WriteLine("   • Concurrent Programming on Windows (Joe Duffy)");

        Console.WriteLine("\n💻 CÓDIGO FONTE:");
        Console.WriteLine("   Todos os exemplos incluem:");
        Console.WriteLine("   - Tipagem explícita");
        Console.WriteLine("   - Comentários detalhados");
        Console.WriteLine("   - Explicações técnicas");
        Console.WriteLine("   - Comparações de performance");

        Console.WriteLine("\n👨‍💻 DESENVOLVIDO PARA APRENDIZADO:");
        Console.WriteLine("   Este projeto é educacional e focado em boas práticas");
        Console.WriteLine("   de programação concorrente em C#/.NET");
    }
}
