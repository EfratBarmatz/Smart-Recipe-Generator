using AutoMapper;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Smart_Recipe_Generator.Models;
using Smart_Recipe_Generator.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Smart_Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        private readonly ILogger<CategoriesController> _logger;
        private readonly IMapper _mapper;
        public ProductsController(IProductService service, ILogger<CategoriesController> logger, IMapper mapper)
        {
            _service = service;
            _logger = logger;
            _mapper = mapper;
        }
        // GET: api/<ProductsController>
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] int? categoryId = null)
        //{
        //    try
        //    {
        //        var products = await _service.GetAllProductsAsync(categoryId);
        //        return Ok(products);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "שגיאה בטעינת מוצרים");
        //        return StatusCode(500, "שגיאה בטעינת מוצרים");
        //    }
        //}

        // GET api/<ProductsController>/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}
        [HttpGet("by-category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetByCategory(int categoryId)
        {
            var products = await _service.GetProductsByCategoryAsync(categoryId);

            if (!products.Any())
                return NotFound("לא נמצאו מוצרים בקטגוריה זו");

            var dto = _mapper.Map<IEnumerable<ProductDto>>(products);
            return Ok(dto);
        }

        // POST api/<ProductsController>
        [HttpPost]
        public async Task<ActionResult> AddProduct([FromBody] AddProductDto productDto, int catId)
        {
            try
            {
                Product product = _mapper.Map<Product>(productDto);
                await _service.AddProductAsync(product,catId);
               
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "שגיאה בהוספת מוצר");
                return StatusCode(500, "שגיאה בהוספת מוצר");
            }
            return Ok();
        }

        // PUT api/<ProductsController>/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        // DELETE api/<ProductsController>/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
