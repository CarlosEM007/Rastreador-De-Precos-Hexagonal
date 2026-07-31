using Application.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Ports.Driven
{
    public interface IManageProductsService
    {
        void RegistrarProduto(ProductDto product);
    }
}
