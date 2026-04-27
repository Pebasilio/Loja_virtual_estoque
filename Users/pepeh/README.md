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
├── Controllers/              # Endpoints da API (ProductsController, CategoriesController, StockController)
├── Models/                   # Entidades (Product, Category, StockMovement) + enum MovementType
├── Repositories/             # Repository Pattern (interfaces + implementações CRUD)
├── Data/                     # AppDbContext (Entity Framework Core)
├── Database/                 # Script SQL (schema.sql) para criação manual do banco
├── Properties/               # launchSettings.json (porta, ambiente)
├── Postman/                  # Coleção Postman para teste dos endpoints
├── Program.cs                # Configuração, DI, seed do banco
├── appsettings.json          # Configurações (connection string SQLite)
├── ApiEstoqueRoupas.csproj   # Projeto .NET
└── bin/Debug/net7.0/estoque.db  # Banco SQLite (gerado no 1º run)
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

## Pré-requisitos

- **.NET 7 SDK** ou superior
- **ASP.NET Core Runtime 7.0** (para execução)
- Windows 10+, Linux ou macOS

## Como Executar

1. Clone ou baixe o repositório:
```bash
git clone https://github.com/Pebasilio/Loja_virtual_estoque.git
cd Loja_virtual_estoque/Loja_virtual_estoque/Users/pepeh
```

2. Restaure as dependências:
```bash
dotnet restore
```

3. Execute a aplicação:
```bash
dotnet run
```

4. Acesse via navegador:
   - **API (Swagger UI)**: `http://localhost:5123/swagger`
   - **Raiz da API**: `http://localhost:5123/api`

O banco `estoque.db` é criado automaticamente no primeiro start, com 4 categorias e 20 produtos de exemplo.

## Solução de Problemas

- **"Framework '7.0.0' not found"**: Instale o ASP.NET Core Runtime 7.0 em https://dotnet.microsoft.com/download
- **Porta 5123 já em uso**: Altere em `Properties/launchSettings.json`

## Testes com Postman

A coleção está em `Postman/EstoqueAPI.postman_collection.json`. Importe no Postman (File → Import):

1. Abra Postman
2. Clique em **File → Import**
3. Selecione o arquivo `Postman/EstoqueAPI.postman_collection.json`
4. A variável `{{baseUrl}}` já vem configurada para `http://localhost:5123`

Todas as requisições estão prontas para testar: **Products**, **Categories** e **Stock**.

## Desenvolvimento

Para fazer build sem executar:
```bash
dotnet build
```

Para testar apenas (se houver testes):
```bash
dotnet test
```

Para publicar (Release):
```bash
dotnet publish -c Release
```

## Script do banco

Um script SQL equivalente ao schema gerado pelo EF está em `Database/schema.sql`, incluindo `CREATE TABLE`, chaves estrangeiras, índices e `INSERT`s de seed. Use-o se preferir criar o banco manualmente com SQLite CLI.

## Notas Importantes

- **Banco de dados**: SQLite (arquivo local `bin/Debug/net7.0/estoque.db`) — não requer servidor externo
- **Arquivos ignorados**: `bin/`, `obj/`, `*.db` estão no `.gitignore` (não são versionados)
- **Connection string**: Configurada em `appsettings.json` — pode ser alterada se necessário
- **Ambiente**: Leia a url e porta em `Properties/launchSettings.json` sob o profile `http`

## Conceitos de C# aplicados

- **Classes e objetos** — `Product`, `Category`, `StockMovement`
- **Propriedades** — incluindo calculadas (`StockStatus`, `TotalStockValue`)
- **Construtores** — inicialização de entidades com validação
- **Collections** — `List<T>` para armazenar múltiplos registros
- **Enum** — `MovementType { ENTRADA, SAIDA }` para tipos de movimentação
- **Injeção de Dependência (DI)** — repositórios injetados nos controllers
- **Async/Await** — operações de banco de dados assíncronas
- **LINQ** — queries para filtros e buscas
- **Entity Framework Core** — mapeamento objeto-relacional (ORM)
- **Entity Framework Core** — DbContext, relacionamentos, `Include`, `AsNoTracking`.
- **Repository Pattern** — `IProductRepository`, `ICategoryRepository`, `IStockMovementRepository`.
- **Injeção de dependência** — repositórios injetados nos controllers.
