using Application.Dto;

namespace Application.Ports.Driving
{
    public interface IManageProductsService
    {
        Task RegistrarProduto(ProductDto product);
        Task ConsultarPrecoProduto(long id);
    }
}
