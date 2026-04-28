using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.DTOs;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;

namespace Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly IMenuItemService _service;

    public MenuController(IMenuItemService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [AllowAnonymous]
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetMenuByCategory(int categoryId)
    {
        return Ok(await _service.GetMenuByCategory(categoryId));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateMenuItemDto dto)
    {
        var item = new MenuItem
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            IsAvailable = dto.IsAvailable,
            PrepTimeMinutes = dto.PrepTimeMinutes ?? 0,
            ImageUrl = dto.ImageUrl
        };

        await _service.AddAsync(item);

        return Ok("Menu item created");
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CreateMenuItemDto dto)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound();

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.CategoryId = dto.CategoryId;
        existing.IsAvailable = dto.IsAvailable;
        existing.PrepTimeMinutes = dto.PrepTimeMinutes ?? 0;
        existing.ImageUrl = dto.ImageUrl;

        await _service.UpdateAsync(existing);

        return Ok("Updated successfully");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok("Deleted successfully");
    }
}