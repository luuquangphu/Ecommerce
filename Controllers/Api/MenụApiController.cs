using Ecommerce.DTO;
using Ecommerce.Models;
using Ecommerce.Services.MenuService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Ecommerce.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class MenuApiController : ControllerBase
    {
        private readonly IMenuService menuService;
        private readonly IWebHostEnvironment env;

        public MenuApiController(IMenuService menuService, IWebHostEnvironment env)
        {
            this.menuService = menuService;
            this.env = env;
        }

        // =========================
        // GET: api/MenuApi
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search)
        {
            var menus = await menuService.GetAll(search);
            return Ok(menus);
        }

        // =========================
        // GET: api/MenuApi/{id}
        // =========================
        [HttpGet("{id}")]
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
        // POST: api/MenuApi (tạo menu + nhiều ảnh)
        // =========================
        [HttpPost]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Create([FromForm] MenuCreateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new StatusDTO
                {
                    IsSuccess = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            try
            {
                var menu = new Menu
                {
                    MenuName = dto.MenuName,
                    MenuCategoryId = dto.MenuCategoryId,
                    Detail = dto.Detail,
                    FoodImages = new List<FoodImage>()
                };

                var result = await menuService.Create(menu);
                if (!result.IsSuccess)
                    return BadRequest(result);

                // ✅ Upload ảnh
                if (dto.Images != null && dto.Images.Any())
                {
                    var uploadPath = Path.Combine(env.WebRootPath, "images", "menu");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    int index = 0;
                    foreach (var file in dto.Images)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        menu.FoodImages.Add(new FoodImage
                        {
                            UrlImage = $"/images/menu/{fileName}",
                            MenuId = menu.MenuId,
                            SortOrder = index,
                            MainImage = index == 0
                        });

                        index++;
                    }

                    await menuService.Update(menu);
                }

                return Ok(new StatusDTO
                {
                    IsSuccess = true,
                    Message = $"Thêm món '{dto.MenuName}' thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new StatusDTO
                {
                    IsSuccess = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }

        // =========================
        // PUT: api/MenuApi/{id} (chỉnh sửa menu + cập nhật ảnh)
        // =========================
        [HttpPut("{id}")]
        [RequestSizeLimit(20_000_000)]
        public async Task<IActionResult> Update(int id, [FromForm] MenuUpdateDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new StatusDTO
                {
                    IsSuccess = false,
                    Message = "Dữ liệu không hợp lệ"
                });
            }

            try
            {
                var existing = await menuService.GetEntityById(id);
                if (existing == null)
                {
                    return NotFound(new StatusDTO
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy món ăn"
                    });
                }

                // ✅ Cập nhật thông tin cơ bản
                existing.MenuName = dto.MenuName;
                existing.Detail = dto.Detail;
                existing.MenuCategoryId = dto.MenuCategoryId;

                // ✅ Parse JSON existingImages: { keep, main }
                if (!string.IsNullOrEmpty(dto.ExistingImages))
                {
                    using var doc = JsonDocument.Parse(dto.ExistingImages);
                    var root = doc.RootElement;

                    // Danh sách giữ lại
                    var keepList = new List<string>();
                    if (root.TryGetProperty("keep", out var keepProp))
                    {
                        keepList = keepProp.EnumerateArray()
                            .Select(e => e.GetString())
                            .Where(e => !string.IsNullOrEmpty(e))
                            .Cast<string>()
                            .ToList();
                    }

                    // Ảnh chính
                    string? mainUrl = null;
                    if (root.TryGetProperty("main", out var mainProp))
                    {
                        mainUrl = mainProp.GetString();
                    }

                    // ✅ Giữ lại ảnh cần thiết
                    existing.FoodImages = existing.FoodImages
                        .Where(img => keepList.Contains(img.UrlImage))
                        .ToList();

                    // ✅ Cập nhật cờ MainImage
                    if (mainUrl != null)
                    {
                        foreach (var img in existing.FoodImages)
                            img.MainImage = img.UrlImage == mainUrl;
                    }
                }

                // ✅ Upload ảnh mới
                if (dto.Images != null && dto.Images.Any())
                {
                    var uploadPath = Path.Combine(env.WebRootPath, "images", "menu");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    int sortOrder = existing.FoodImages.Count;
                    foreach (var file in dto.Images)
                    {
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        existing.FoodImages.Add(new FoodImage
                        {
                            UrlImage = $"/images/menu/{fileName}",
                            MenuId = id,
                            SortOrder = sortOrder,
                            MainImage = false
                        });
                        sortOrder++;
                    }
                }

                // ✅ Đảm bảo có 1 ảnh chính
                if (!existing.FoodImages.Any(i => i.MainImage) && existing.FoodImages.Any())
                {
                    existing.FoodImages.First().MainImage = true;
                }

                var result = await menuService.Update(existing);

                if (!result.IsSuccess)
                    return BadRequest(result);

                return Ok(new StatusDTO
                {
                    IsSuccess = true,
                    Message = "Cập nhật món ăn thành công"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new StatusDTO
                {
                    IsSuccess = false,
                    Message = $"Lỗi server: {ex.Message}"
                });
            }
        }


        // =========================
        // DELETE: api/MenuApi/{id}
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await menuService.Delete(id);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
