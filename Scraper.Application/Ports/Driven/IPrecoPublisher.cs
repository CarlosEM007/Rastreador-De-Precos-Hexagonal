using Rastreador.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Ports.Driven
{
    public interface IPrecoPublisher
    {
        Task PublicarPreco(ProdutoPrecoDto produto);
    }
}
