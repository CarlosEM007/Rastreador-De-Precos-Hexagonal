using Adapters.Persistence.Data;
using Application.Ports.Driving;
using Domain.Entities;

namespace Adapters.Persistence.Repository
{
    public class ProdutoRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProdutoRepository(AppDbContext context)
        {
            _context = context;
        }

        public Produto BuscarProduct(long id)
        {
            throw new NotImplementedException();
        }

        public void SalvarProduto(Produto produto)
        {
            throw new NotImplementedException();
        }
    }
}
