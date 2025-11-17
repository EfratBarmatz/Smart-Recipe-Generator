using AutoMapper;
using DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Smart_Recipe_Generator.Models;
using Smart_Recipe_Generator.Services;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Smart_Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;
        private readonly ILogger<CategoriesController> _logger;
        private readonly IMapper _mapper;
        public CategoriesController(ICategoryService service, ILogger<CategoriesController> logger, IMapper mapper)
        {
            _service = service;
            _logger = logger;
            _mapper = mapper;
        }
        // GET: api/<CategoriesController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            try
            {
                IEnumerable<Category> categories = await _service.GetAllCategoriesAsync();
                IEnumerable<CategoryDto> dto = _mapper.Map<IEnumerable<CategoryDto>>(categories);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בטעינת קטגוריות");
                return StatusCode(500, "שגיאה בטעינת קטגוריות");
            }
        }

        // GET api/<CategoriesController>/5
        //[HttpGet("{id}")]
        //public void Get(int id)
        //{
        //}


        // POST api/<CategoriesController>
        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] AddCategoryDto categoryDto)
        {
            try
            {
                Category category = _mapper.Map<Category>(categoryDto);
                await _service.AddCategoryAsync(category);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בהוספת קטגוריה");
                return StatusCode(500, "שגיאה בהוספת קטגוריה");
            }
            return Ok();
        }

        // PUT api/<CategoriesController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE api/<CategoriesController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
