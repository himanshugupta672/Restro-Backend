using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _service;

    public UserController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("chefs")]
    public async Task<IActionResult> GetChefs()
    {
        return Ok(await _service.GetChefsAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        await _service.AddAsync(user);
        return Ok(user);
    }

    [HttpPut("available/{id}")]
    public async Task<IActionResult> SetAvailable(int id)
    {
        await _service.SetAvailable(id);
        return Ok();
    }

    [HttpPut("busy/{id}")]
    public async Task<IActionResult> SetBusy(int id)
    {
        await _service.SetBusy(id);
        return Ok();
    }

    [HttpPut("offline/{id}")]
    public async Task<IActionResult> SetOffline(int id)
    {
        await _service.SetOffline(id);
        return Ok();
    }
}