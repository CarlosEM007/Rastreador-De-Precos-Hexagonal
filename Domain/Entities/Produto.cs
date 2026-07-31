using Domain.Exceptions;

namespace Domain.Entities
{
    public class Produto
    {
        public long Id { get; set; }

        public string Nome { get; set; }
        public string EnderecoProduto { get; set; }

        public decimal PrecoAtual { get; set; }
        public decimal PrecoAlvo { get; set; }

        public Produto(string nome, string enderecoProduto, decimal precoAtual, decimal precoAlvo)
        {
            if (enderecoProduto == null)
            {
                throw new InvalidProductURLException("Não pode ser vázia.");
            }

            if(precoAtual <= precoAlvo)
            {
                throw new InvalidProductValueException("Preço atual não pode ser menor ou igual que preço alvo!");
            }

            Nome = nome;
            EnderecoProduto = enderecoProduto;
            PrecoAtual = precoAtual;
            PrecoAlvo = precoAlvo;
        }

        public bool AtingiuPrecoAlvo(decimal precoAtual, decimal precoAlvo, out string Mensagem)
        {
            Mensagem = "";

            if(PrecoAtual <= precoAlvo)
            {
                Mensagem = $"O produto {this.Nome} atingiu o valor {precoAtual}";
                return true;
            }

            return false;
        }
    }
}
