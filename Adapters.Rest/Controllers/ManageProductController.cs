using Application.Dto;
using Application.Ports.Driven;
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
            _service.RegistrarProduto(product);

            return Ok();
        }
    }
}
