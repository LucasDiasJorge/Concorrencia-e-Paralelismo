-- ============================================================
-- Script de Setup do Banco de Dados
-- Demonstração de Incremento Atômico vs Não-Atômico
-- ============================================================

-- Cria o banco de dados se não existir
CREATE DATABASE IF NOT EXISTS counter_demo;

-- Seleciona o banco de dados
USE counter_demo;

-- Remove a tabela se existir (para testes limpos)
DROP TABLE IF EXISTS products;

-- Cria a tabela de produtos
CREATE TABLE products (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    stock_quantity INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Insere o produto de demonstração
INSERT INTO products (id, name, stock_quantity) 
VALUES (1, 'Produto Teste', 0);

-- Verifica se o produto foi inserido corretamente
SELECT * FROM products;

-- ============================================================
-- Comandos úteis para testes manuais
-- ============================================================

-- Reseta o estoque para 0
-- UPDATE products SET stock_quantity = 0 WHERE id = 1;

-- Verifica o estoque atual
-- SELECT id, name, stock_quantity FROM products WHERE id = 1;

-- Incremento atômico (exemplo)
-- UPDATE products SET stock_quantity = stock_quantity + 1 WHERE id = 1;
-- SELECT stock_quantity FROM products WHERE id = 1;
