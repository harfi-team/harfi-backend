using Harfi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Harfi.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServicesController : ControllerBase
{
    private readonly IServiceTypeService _serviceTypeService;

    public ServicesController(IServiceTypeService serviceTypeService)
    {
        _serviceTypeService = serviceTypeService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveServices()
    {
        var services = await _serviceTypeService.GetActiveServiceTypesAsync();
        return Ok(services);
    }
}
