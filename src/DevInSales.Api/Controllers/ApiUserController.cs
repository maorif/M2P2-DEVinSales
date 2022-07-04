using DevInSales.Application.Interfaces.Services;
using DevInSales.Application.Dtos;

using Microsoft.AspNetCore.Mvc;

namespace DevInSales.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApiUserController : ControllerBase
    {
        private IIdentityService _identityService;

        public ApiUserController(IIdentityService identityService)
        {
            this._identityService = identityService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<RegisterUserResponse>> Register(RegisterUserRequest userData)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            
            var response = await this._identityService.RegisterUser(userData);
            if (response.Success)
                return Ok(response);
            else if (response.Errors.Count() > 0)
                return BadRequest(response);
            
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost("login")]
        public async Task<ActionResult<RegisterUserResponse>> Login(LoginRequest login)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var response = await this._identityService.Login(login);
            if (response.Success)
                return Ok(response);

            return Unauthorized(response);
        }

        // [Authorize]
        // [HttpPost("refresh-login")]
        // public async Task<ActionResult<RegisterUserResponse>> RefreshLogin()
        // {
        //     var identity = HttpContext.User.Identity as ClaimsIdentity;
        //     var userId = identity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //     if (userId == null)
        //         return BadRequest();

        //     var response = await this._identityService.NoPasswordLogin(userId);
        //     if (response.Success)
        //         return Ok(response);

        //     return Unauthorized(response);
        // }
    }
}   