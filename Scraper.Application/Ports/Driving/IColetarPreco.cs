using Rastreador.Contracts;

namespace Application.Ports.Driving
{
    public interface IColetarPreco
    {
        Task ColetarPrecoProduto(ProdutoContratoDto produto);
    }
}
