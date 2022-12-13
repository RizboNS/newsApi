using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using newsApi.Services.AdminService;

namespace newsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost]
        public async Task<IActionResult> CheckForDomainPathInDb()
        {
            var domainName = new Uri($"{Request.Scheme}://{Request.Host}").AbsoluteUri;
            var serviceResponse = await _adminService.CheckDomain(domainName);

            return Ok(serviceResponse);
        }
    }
}