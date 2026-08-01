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


        [HttpGet("{id}")]
        public async Task<IActionResult> ConsultarPrecoProduto(long id)
        {
            await _service.ConsultarPrecoProduto(id);

            return Ok();
        }
    }
}
