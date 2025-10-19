using Ecommerce.DTO;
using Ecommerce.Models;
using Ecommerce.Services.InventoryService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class InventoryApiController : ControllerBase
    {
        private readonly IInventoryService inventoryService;

        public InventoryApiController(IInventoryService inventoryService)
        {
            this.inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await inventoryService.GetAll();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await inventoryService.GetById(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Inventory model)
        {
            var result = await inventoryService.Create(model);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Inventory model)
        {
            if (id != model.InventoryId)
                return BadRequest(new StatusDTO { IsSuccess = false, Message = "ID không khớp" });

            var result = await inventoryService.Update(model);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await inventoryService.Delete(id);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
