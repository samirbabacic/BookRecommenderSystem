using BookRecommenderSystem.DTOs;
using BookRecommenderSystem.Requests.User;
using BookRecommenderSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookRecommenderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet(Name = "GetUser")]
        public async Task<UserDTO> Get(string id)
        {
            return await _userService.GetAsync(id);
        }

        [HttpPost("RateBook")]
        public async Task<UserDTO> RateBook([FromBody] RateBookRequest request)
        {
            return await _userService.RateBook(request.UserId, request.BookId, request.Rating, request.Description);
        }

        [HttpPost("OrderBook")]
        public async Task<UserDTO> OrderBook([FromBody] OrderBookRequest request)
        {
            return await _userService.OrderBook(request.UserId, request.BookId);
        }
    }
}
