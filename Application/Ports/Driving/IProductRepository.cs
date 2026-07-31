using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Ports.Driving
{
    public interface IProductRepository
    {
        void SalvarProduto(Produto produto);
        Produto BuscarProduct(long id);
    }
}
