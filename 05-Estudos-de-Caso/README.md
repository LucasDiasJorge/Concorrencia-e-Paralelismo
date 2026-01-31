# 🌍 Estudos de Caso

Teoria é importante, mas ver os conceitos aplicados em cenários reais consolida o aprendizado. Esta seção contém projetos que simulam problemas encontrados em sistemas de produção.

---

## 📂 Conteúdo

### [Database-Atomicity/](Database-Atomicity/)
Demonstração de operações atômicas vs não-atômicas em banco de dados (MySQL).

**Cenário:** Sistema de estoque onde múltiplos processos atualizam a quantidade de um produto simultaneamente.

**O que você encontra:**
- Operação atômica: `UPDATE SET stock = stock + 1`
- Operação não-atômica: SELECT → incrementa em memória → UPDATE
- Demonstração de incrementos perdidos
- Script SQL para setup

**Lição:** Operações atômicas no banco são essenciais para consistência. O padrão read-modify-write em passos separados é receita para disaster.

---

## 🎯 O que você vai aprender aqui

1. **Atomicidade em bancos de dados** — Como garantir consistência
2. **Optimistic vs Pessimistic Locking** — Estratégias de bloqueio
3. **Problemas reais de concorrência** — Race conditions em produção
4. **Trade-offs de design** — Performance vs consistência

---

## 💡 Cenários clássicos de problemas

### 1. Dupla reserva (Double Booking)
Dois usuários reservam o mesmo assento/quarto ao mesmo tempo.
```
User A: Vê assento disponível → Reserva
User B: Vê assento disponível → Reserva (antes de A salvar)
Resultado: Mesmo assento reservado duas vezes!
```

### 2. Overselling
Loja vende mais produtos do que tem em estoque.
```
Estoque: 1 unidade
User A: Vê 1 → Compra
User B: Vê 1 → Compra (antes do estoque atualizar)
Resultado: -1 em estoque!
```

### 3. Transferência bancária
Saldo inconsistente durante transferência.
```
Saldo A: 100, Saldo B: 50
Thread 1: A -= 30 (70), B += 30 (80) ✓
Thread 2: Lê A=100, B=50 (entre as operações) ← Estado inconsistente!
```

---

## 🔧 Soluções típicas

### No banco de dados
```sql
-- Atômico: UPDATE único
UPDATE products SET stock = stock - 1 WHERE id = 1 AND stock > 0;

-- Com transação e lock
BEGIN TRANSACTION;
SELECT * FROM products WHERE id = 1 FOR UPDATE;
-- processa...
UPDATE products SET stock = stock - 1 WHERE id = 1;
COMMIT;
```

### Na aplicação
```csharp
// Optimistic Locking com versão
var product = await GetProduct(id);
product.Stock--;
product.Version++;

var updated = await UpdateIfVersionMatches(product, originalVersion);
if (!updated) 
    throw new ConcurrencyException("Alguém modificou antes de você!");
```

---

## 📖 Como usar esta seção

1. **Leia o cenário** — Entenda o problema
2. **Execute a demo** — Veja o bug acontecendo
3. **Estude a solução** — Compare as abordagens
4. **Implemente você mesmo** — Crie variações do problema

---

## 🔗 Conexão com outras seções

| Seção | Relação |
|-------|---------|
| [01-Fundamentos](../01-Fundamentos/) | Conceitos básicos que fundamentam os problemas |
| [02-Sincronizacao](../02-Sincronizacao/) | Técnicas usadas para resolver os problemas |
| [03-Estruturas-Concorrentes](../03-Estruturas-Concorrentes/) | Ferramentas que abstraem soluções |

---

## 💭 Reflexão final

> "Em sistemas distribuídos e concorrentes, se algo pode dar errado, eventualmente dará. A questão não é **se** vai acontecer, mas **quando** e **como você vai lidar** com isso."

O objetivo destes estudos de caso é desenvolver a intuição para identificar onde os problemas podem surgir antes que eles causem danos em produção.
