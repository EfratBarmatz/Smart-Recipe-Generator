using Microsoft.AspNetCore.Mvc;
using Smart_Recipe_Generator.Models;
using Smart_Recipe_Generator.Services;
using System.Threading.Tasks;

namespace Smart_Recipe_Generator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly IRecipeService _recipeService;

        public RecipesController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        // GET: api/recipes
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "Smart Recipe Generator API";
        }

        // POST api/recipes/generate
        [HttpPost("generate")]
        public async Task<ActionResult<RecipeResponse>> Generate([FromBody] RecipeRequest request)
        {
            if (request == null || request.Ingredients == null || request.Ingredients.Count == 0)
            {
                return BadRequest("Please provide a list of ingredients.");
            }

            var recipe = await _recipeService.GenerateRecipeAsync(request);
            return Ok(recipe);
        }

        // GET api/recipes/{id}
        [HttpGet("{id}")]
        public ActionResult<string> GetById(int id)
        {
            // Placeholder - implement retrieval from DB/storage later
            return NotFound();
        }
    }
}
