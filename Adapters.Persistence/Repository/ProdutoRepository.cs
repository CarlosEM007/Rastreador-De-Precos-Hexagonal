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

        public async Task SalvarProduto(Produto produto)
        {
            await _context.Produtos.AddAsync(produto);

            await _context.SaveChangesAsync();
        }
    }
}
