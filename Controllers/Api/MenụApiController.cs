using Ecommerce.DTO;
using Ecommerce.Models;
using Ecommerce.Services.MenuService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuApiController : ControllerBase
    {
        private readonly IMenuService menuService;

        public MenuApiController(IMenuService menuService)
        {
            this.menuService = menuService;
        }

        // =========================
        // GET: api/MenuApi
        // =========================
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var menus = await menuService.GetAll(search);
            return Ok(menus);
        }


        // =========================
        // GET: api/MenuApi/{id}
        // =========================
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            var menu = await menuService.GetById(id);
            if (menu == null)
                return NotFound(new StatusDTO
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy món ăn"
                });

            return Ok(menu);
        }

        // =========================
        // POST: api/MenuApi
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Menu model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new StatusDTO
                {
                    IsSuccess = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            var result = await menuService.Create(model);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        // =========================
        // PUT: api/MenuApi/{id}
        // =========================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Menu model)
        {
            if (id != model.MenuId)
            {
                return BadRequest(new StatusDTO
                {
                    IsSuccess = false,
                    Message = "Không tìm thấy món ăn"
                });
            }

            var result = await menuService.Update(model);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {   
            var result = await menuService.Delete(id);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("GetAvailableMenus")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetAvailableMenus()
        {
            var result = await menuService.GetAvailableMenusAsync();
            if (result == null || !result.Any())
                return NotFound(new { message = "Không có món ăn khả dụng." });

            return Ok(result);
        }
    }
}
