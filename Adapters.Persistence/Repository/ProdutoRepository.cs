using Adapters.Persistence.Data;
using Application.Ports.Driven;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Adapters.Persistence.Repository
{
    public class ProdutoRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProdutoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Produto> BuscarProduto(long id)
        {
            Produto? produto = await _context.Produtos.Where(p => p.Id == id).FirstOrDefaultAsync();

            if(produto == null)
            {
                throw new ProductNotFoundException("Produto não encontrado.");
            }

            return produto;
        }

        public async Task<IReadOnlyList<Produto>> ListarProdutos()
        {
            // AsNoTracking: a listagem é só leitura, não precisa do change tracker.
            return await _context.Produtos
                                 .AsNoTracking()
                                 .OrderBy(p => p.Id)
                                 .ToListAsync();
        }

        public async Task SalvarProduto(Produto produto)
        {
            await _context.Produtos.AddAsync(produto);

            await _context.SaveChangesAsync();
        }

        public async Task AtualizarProduto(Produto produto)
        {
            // O produto já vem rastreado quando foi lido por este mesmo contexto.
            // O Update cobre o caso em que não veio (ex.: veio de outro escopo).
            _context.Produtos.Update(produto);

            await _context.SaveChangesAsync();
        }

        public async Task RemoverProduto(long id)
        {
            var produto = await BuscarProduto(id);

            _context.Produtos.Remove(produto);

            await _context.SaveChangesAsync();
        }
    }
}
