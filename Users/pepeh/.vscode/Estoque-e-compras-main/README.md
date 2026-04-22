# Sistema de Controle de Estoque — Web API

Web API em **ASP.NET Core (C#)** para gerenciar um estoque de roupas. O sistema controla produtos, categorias e movimentações de estoque (entradas e saídas), com regras de negócio para alertas de reposição, cálculos de valor de inventário e relatórios.

## Participante
- Pedro Henrique Basilio
- Matheus Henrique Bueno
- Gabriel Ferraz Canoff

## Arquitetura

Projeto organizado em camadas:

```
/
├── Controllers/   # Endpoints da API (ProductsController, CategoriesController, StockController)
├── Models/        # Entidades (Product, Category, StockMovement) + enum MovementType + DTOs
├── Repositories/  # Repository Pattern (interfaces + implementações)
├── Data/          # AppDbContext (Entity Framework Core)
├── Database/      # Script SQL (schema.sql) para criação manual do banco
├── Postman/       # Coleção Postman para teste dos endpoints
├── Program.cs     # Configuração, DI, seed do banco
└── estoque.db     # Banco SQLite (gerado automaticamente no 1º run)
```

Fluxo: **Controller → Repository → DbContext → SQLite**

## Tecnologias

- .NET 7 / C#
- ASP.NET Core Web API (Controllers)
- Entity Framework Core (Sqlite)
- Repository Pattern
- Swagger (Swashbuckle)
- SQLite

## Entidades e Relacionamentos

- **Category** `1 ← N` **Product** (uma categoria possui vários produtos)
- **Product** `1 ← N` **StockMovement** (um produto possui várias movimentações)
- **Enum `MovementType`**: `ENTRADA`, `SAIDA`

## Regras de Negócio

- Validação de campos obrigatórios (Nome, Categoria válida, valores não-negativos).
- Saída de estoque bloqueia operação quando estoque é insuficiente.
- Alerta automático (`NeedsRestock`) quando `Quantity <= ReorderThreshold`.
- Classificação de alerta: `CRITICAL` (estoque zero) ou `WARNING`.
- Sugestão de reposição calculada: `(ReorderThreshold * 3) - Quantity`.
- Propriedade calculada `TotalStockValue` (Quantity × Price) e `StockStatus` (OK / ESTOQUE_BAIXO / SEM_ESTOQUE).
- Relatório agregado: total de produtos, unidades, valor de inventário, preço médio, entradas/saídas do dia.

## Endpoints

### Produtos — `/api/products`

| Método | Rota                           | Descrição                                            |
|--------|--------------------------------|------------------------------------------------------|
| GET    | `/api/products`                | Lista todos os produtos (incluindo a categoria)     |
| GET    | `/api/products/{id}`           | Busca um produto pelo ID                            |
| GET    | `/api/products/low-stock`      | Lista produtos com estoque abaixo do limite        |
| POST   | `/api/products`                | Cria um novo produto                                |
| PUT    | `/api/products/{id}`           | Atualiza um produto existente                       |
| DELETE | `/api/products/{id}`           | Remove um produto                                   |

### Categorias — `/api/categories`

| Método | Rota                     | Descrição                                     |
|--------|--------------------------|-----------------------------------------------|
| GET    | `/api/categories`        | Lista todas as categorias (com produtos)      |
| GET    | `/api/categories/{id}`   | Busca uma categoria pelo ID                   |
| POST   | `/api/categories`        | Cria nova categoria                           |
| PUT    | `/api/categories/{id}`   | Atualiza categoria                            |
| DELETE | `/api/categories/{id}`   | Remove categoria (apenas se sem produtos)     |

### Estoque / Movimentações — `/api/stock`

| Método | Rota                                 | Descrição                                                  |
|--------|--------------------------------------|------------------------------------------------------------|
| POST   | `/api/stock/entry`                   | Registra entrada de estoque                                |
| POST   | `/api/stock/exit`                    | Registra saída de estoque (valida disponibilidade)        |
| GET    | `/api/stock/history/{productId}`     | Histórico de movimentações do produto                     |
| GET    | `/api/stock/movements?type=ENTRADA`  | Movimentações (filtrável por `ENTRADA` ou `SAIDA`)        |
| GET    | `/api/stock/restock-alerts`          | Produtos que precisam de reposição (com sugestão)         |
| GET    | `/api/stock/report`                  | Relatório consolidado do estoque                          |

## Exemplos de Payload

### POST `/api/products`
```json
{
  "name": "Camiseta Regata",
  "categoryId": 1,
  "quantity": 20,
  "reorderThreshold": 5,
  "price": 39.90
}
```

### POST `/api/stock/entry`
```json
{
  "productId": 1,
  "quantity": 10,
  "reason": "Reposição de fornecedor",
  "user": "admin"
}
```

### POST `/api/stock/exit`
```json
{
  "productId": 1,
  "quantity": 2,
  "reason": "Venda balcão",
  "user": "vendedor"
}
```

## Como Executar

1. Instale o **.NET 7 SDK**.
2. No diretório do projeto, execute:

```bash
dotnet restore
dotnet run
```

3. Acesse:
   - API: `http://localhost:5123/api/products`
   - Swagger: `http://localhost:5123/swagger`

O banco `estoque.db` é criado automaticamente no primeiro start, com 4 categorias e 20 produtos de exemplo.

## Testes com Postman

A coleção está em `Postman/EstoqueAPI.postman_collection.json`. Importe no Postman (File → Import) — já vem com a variável `{{baseUrl}}` apontando para `http://localhost:5123` e requisições prontas para todos os endpoints (Products, Categories, Stock).

## Script do banco

Um script SQL equivalente ao schema gerado pelo EF está em `Database/schema.sql`, incluindo `CREATE TABLE`, chaves estrangeiras, índices e `INSERT`s de seed.

## Conceitos de C# aplicados

- **Classes e objetos** — `Product`, `Category`, `StockMovement`.
- **Propriedades** — incluindo calculadas (`StockStatus`, `TotalStockValue`).
- **Construtores** — `Category(string)`, `Product(string, int, int, int, decimal)`.
- **Collections** — `List<Product>`, `List<Category>`, `List<StockMovement>`.
- **Enum** — `MovementType { ENTRADA, SAIDA }`.
- **Entity Framework Core** — DbContext, relacionamentos, `Include`, `AsNoTracking`.
- **Repository Pattern** — `IProductRepository`, `ICategoryRepository`, `IStockMovementRepository`.
- **Injeção de dependência** — repositórios injetados nos controllers.
