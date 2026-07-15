using CapfortuneBE.DataAccess;
using CapfortuneBE.Interface;
using CapfortuneBE.Models;
using CapfortuneBE.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static CapfortuneBE.Models.AdminUserDTO;

namespace CapfortuneBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IUserDataAccess _userDataAccess;

        public UserController(UserService userService, IUserDataAccess userDataAccess)
        {
            _userService = userService;
            _userDataAccess = userDataAccess;
        }

        /// <summary>
        /// Authenticates an admin user and returns a JWT token.
        /// </summary>
        /// <param name="request">Login credentials.</param>
        /// <returns>JWT token and user details.</returns>
        /// <response code="200">Login successful.</response>
        /// <response code="401">Invalid credentials.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _userService.LoginAsync(request);
                if (result == null)
                {
                    return ApiResponse.Unauthorized("Invalid email or password.");
                }
                return ApiResponse.Ok(result, "Login successful.");
            }
            catch (Exception ex)
            {
                await _userDataAccess.LogErrorAsync(Constants.Constants.Layers.Controller,nameof(Login),ex,request);
                return ApiResponse.Error(ex, null, Constants.Constants.Messages.ErrorMessage);
            }
        }

        /// <summary>
        /// Registers a new admin user.
        /// </summary>
        /// <param name="request">Registration details.</param>
        /// <returns>The newly registered user.</returns>
        /// <response code="201">User registered successfully.</response>
        /// <response code="409">Email already exists.</response>
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _userService.RegisterAsync(request);
                if (result == null)
                    return ApiResponse.Conflict("A user with this email already exists.");
                return ApiResponse.Created(result, "User registered successfully.");
            }
            catch (Exception ex)
            {
                await _userDataAccess.LogErrorAsync(Constants.Constants.Layers.Controller, nameof(RegisterAsync), ex, request);
                return ApiResponse.Error(ex, null, Constants.Constants.Messages.ErrorMessage);
            }
        }
    }
}
