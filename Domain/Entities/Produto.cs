using Domain.Exceptions;

namespace Domain.Entities
{
    public class Produto
    {
        public long Id { get; set; }

        public string? Nome { get; set; }
        public string? EnderecoProduto { get; set; }

        public decimal PrecoAtual { get; set; }
        public decimal PrecoAlvo { get; set; }

        public Produto() { }

        public Produto(string nome, string enderecoProduto, decimal precoAtual, decimal precoAlvo)
        {
            if (string.IsNullOrEmpty(enderecoProduto))
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

        /// <summary>
        /// Atualiza os dados que o usuário controla. O preço atual fica de fora de propósito:
        /// quem dita esse valor é o scraper, não quem edita o cadastro.
        /// </summary>
        public void AtualizarCadastro(string nome, string enderecoProduto, decimal precoAlvo)
        {
            if (string.IsNullOrEmpty(enderecoProduto))
            {
                throw new InvalidProductURLException("Não pode ser vázia.");
            }

            if (precoAlvo <= 0)
            {
                throw new InvalidProductValueException("Preço alvo precisa ser maior que zero!");
            }

            Nome = nome;
            EnderecoProduto = enderecoProduto;
            PrecoAlvo = precoAlvo;
        }

        /// <summary>
        /// Guarda o preço que o scraper encontrou. Aqui não vale a regra do construtor
        /// (atual > alvo): um preço abaixo do alvo é justamente o caso que queremos registrar.
        /// </summary>
        public void RegistrarPrecoColetado(decimal precoRecebidoDoScraper)
        {
            if (precoRecebidoDoScraper <= 0)
            {
                throw new InvalidProductValueException("Preço coletado precisa ser maior que zero!");
            }

            PrecoAtual = precoRecebidoDoScraper;
        }

        public bool AtingiuPrecoAlvo(decimal precoRecebidoDoScraper, out string Mensagem)
        {
            Mensagem = "";

            if(precoRecebidoDoScraper <= this.PrecoAlvo)
            {
                Mensagem = $"O produto {this.Nome} atingiu o preço de R${precoRecebidoDoScraper}";
                return true;
            }

            return false;
        }
    }
}
