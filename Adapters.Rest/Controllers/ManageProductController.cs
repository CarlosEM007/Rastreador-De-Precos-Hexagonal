using Application.Dto;
using Application.Ports.Driving;
using Microsoft.AspNetCore.Mvc;

namespace Adapters.Rest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManageProductController: ControllerBase
    {
        private readonly IManageProductsService _service;

        public ManageProductController(IManageProductsService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterProduct(ProductDto product)
        {
            await _service.RegistrarProduto(product);

            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProdutoResumoDto>>> ListarProdutos()
        {
            return Ok(await _service.ListarProdutos());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoResumoDto>> ObterProduto(long id)
        {
            return Ok(await _service.ObterProduto(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarProduto(long id, ProductDto product)
        {
            await _service.AtualizarProduto(id, product);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoverProduto(long id)
        {
            await _service.RemoverProduto(id);

            return NoContent();
        }

        /// <summary>
        /// Rastreamento sob demanda: publica o produto na fila e devolve na hora.
        /// O preço chega depois, pelo caminho assíncrono — por isso 202, e não 200.
        /// </summary>
        [HttpPost("{id}/rastrear")]
        public async Task<IActionResult> RastrearAgora(long id)
        {
            await _service.ConsultarPrecoProduto(id);

            return Accepted();
        }
    }
}
