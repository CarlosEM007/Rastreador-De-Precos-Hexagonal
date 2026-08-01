using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Ports.Driven
{
    public interface IProductRepository
    {
        Task SalvarProduto(Produto produto);
        Task<Produto> BuscarProduto(long id);
    }
}
