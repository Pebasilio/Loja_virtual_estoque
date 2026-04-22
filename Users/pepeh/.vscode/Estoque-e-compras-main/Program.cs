using ApiEstoqueRoupas.Data;
using ApiEstoqueRoupas.Models;
using ApiEstoqueRoupas.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=estoque.db"));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IStockMovementRepository, StockMovementRepository>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    if (!context.Categories.Any())
    {
        var camisas = new Category("Camisas");
        var jaquetas = new Category("Jaquetas");
        var calcas = new Category("Calças");
        var meias = new Category("Meias");

        context.Categories.AddRange(camisas, jaquetas, calcas, meias);
        context.SaveChanges();

        var produtosIniciais = new List<Product>
        {
            new Product("Camisa Polo Azul", camisas.Id, 30, 5, 89.90m),
            new Product("Camisa Branca", camisas.Id, 25, 5, 59.90m),
            new Product("Camisa Preta", camisas.Id, 40, 8, 59.90m),
            new Product("Jaqueta Jeans", jaquetas.Id, 20, 3, 199.90m),
            new Product("Jaqueta de Couro", jaquetas.Id, 10, 2, 499.90m),
            new Product("Calça Jeans Azul", calcas.Id, 35, 6, 149.90m),
            new Product("Calça Moletom Cinza", calcas.Id, 28, 4, 119.90m),
            new Product("Calça Preta", calcas.Id, 18, 3, 129.90m),
            new Product("Meias Brancas (par)", meias.Id, 100, 20, 14.90m),
            new Product("Meias Pretas (par)", meias.Id, 80, 15, 14.90m),
            new Product("Camisa Social Azul", camisas.Id, 25, 5, 129.90m),
            new Product("Camisa Social Branca", camisas.Id, 30, 6, 129.90m),
            new Product("Camisa Estampada", camisas.Id, 22, 4, 79.90m),
            new Product("Jaqueta de Moletom", jaquetas.Id, 15, 3, 169.90m),
            new Product("Jaqueta Puffer", jaquetas.Id, 12, 2, 289.90m),
            new Product("Calça Cargo Verde", calcas.Id, 20, 5, 139.90m),
            new Product("Calça Social Preta", calcas.Id, 17, 3, 159.90m),
            new Product("Calça Jeans Clara", calcas.Id, 33, 6, 149.90m),
            new Product("Meias Coloridas (par)", meias.Id, 70, 10, 19.90m),
            new Product("Meias Esportivas (par)", meias.Id, 90, 15, 24.90m)
        };

        context.Products.AddRange(produtosIniciais);
        context.SaveChanges();

        Console.WriteLine($"Banco criado com {produtosIniciais.Count} produtos e 4 categorias.");
    }
    else
    {
        Console.WriteLine($"Banco existente com {context.Products.Count()} produtos.");
    }
}

app.MapControllers();

Console.WriteLine("\nSERVIDOR RODANDO");
Console.WriteLine("  Swagger:    http://localhost:5123/swagger");
Console.WriteLine("  Produtos:   http://localhost:5123/api/products");
Console.WriteLine("  Categorias: http://localhost:5123/api/categories");
Console.WriteLine("  Estoque:    http://localhost:5123/api/stock/...\n");

app.Run();
