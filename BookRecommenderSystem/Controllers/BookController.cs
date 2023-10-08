using BookRecommenderSystem.DTOs;
using BookRecommenderSystem.Requests;
using BookRecommenderSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookRecommenderSystem.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        /// <summary>
        /// Use to get ratings.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet(Name = "GetBook")]
        public BookDTO Get(string id)
        {
            return _bookService.GetById(id);
        }

        [HttpPost("GetPaged")]
        public List<BookDTO> GetPaged(GetPagedRequest request)
        {
            return _bookService.GetBooks(request.PageIndex, request.PageSize);
        }
    }
}
