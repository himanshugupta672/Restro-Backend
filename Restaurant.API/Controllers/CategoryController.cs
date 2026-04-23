using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.Interfaces.Services;
using Restaurant.Domain.Entities;

namespace Restaurant.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoryController(ICategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _service.GetAllAsync();
        return Ok(data);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _service.GetByIdAsync(id);
        return Ok(data);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category category)
    {
        await _service.AddAsync(category);
        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update(Category category)
    {
        await _service.UpdateAsync(category);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok();
    }
}