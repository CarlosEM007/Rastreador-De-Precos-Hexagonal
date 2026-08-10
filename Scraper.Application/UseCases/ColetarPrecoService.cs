using Application.Ports.Driven;
using Application.Ports.Driving;
using Rastreador.Contracts;

namespace Application.UseCases
{
    public class ColetarPrecoService : IColetarPreco
    {
        private readonly IColetorDePreco _coletor;
        private readonly IPrecoPublisher _publisher;

        public ColetarPrecoService(IColetorDePreco coletor, IPrecoPublisher publisher)
        {
            _coletor = coletor;
            _publisher = publisher;
        }

        public async Task ColetarPrecoProduto(ProdutoContratoDto produto)
        {
            ProdutoPrecoDto prod = await _coletor.BuscarPrecoDoProduto(produto);

            await _publisher.PublicarPreco(prod);
        }
    }
}
