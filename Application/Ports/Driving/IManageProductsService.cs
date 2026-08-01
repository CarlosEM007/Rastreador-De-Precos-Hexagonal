using Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Ports.Driving
{
    public interface IManageProductsService
    {
        Task RegistrarProduto(ProductDto product);
        Task ConsultarPrecoProduto(long id);
    }
}
