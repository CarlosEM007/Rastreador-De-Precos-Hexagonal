using Application.Ports.Driven;
using Rastreador.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adapters.Messaging.Publisher
{
    public class PrecoPublisher : IPrecoPublisher
    {
        public Task PublicarPreco(ProdutoPrecoDto produto)
        {
            throw new NotImplementedException();
        }
    }
}
