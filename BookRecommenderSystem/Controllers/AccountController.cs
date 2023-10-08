using BookRecommenderSystem.DTOs;
using BookRecommenderSystem.Requests;
using BookRecommenderSystem.Requests.Account;
using BookRecommenderSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookRecommenderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Login")]
        public UserDTO Login([FromBody] LoginRequest request)
        {
            return _userService.GetUser(request.ProvisionId, request.Password);
        }
    }
}
