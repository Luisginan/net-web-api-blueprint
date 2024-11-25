using Blueprint.CustomerModule.DTO;
using Blueprint.CustomerModule.Services;
using Microsoft.AspNetCore.Mvc;


namespace Blueprint.CustomerModule.Controllers;

[Route("blueprint/[controller]")]
[ApiController]
public class ProductController(IProductExternal productExternal) : ControllerBase
{
    // GET: api/<ProductController>
    [HttpGet("{id}")]
    public Task<IActionResult> Get(string id)
    {
        var products = productExternal.GetProduct(id);
        if (products == null)
        {
            return Task.FromResult<IActionResult>(NotFound("Product doesn't exist"));
        }
        return Task.FromResult<IActionResult>(Ok(new ProductResponse { 
            Id = products.Id, 
            Title = products.Title, 
            Brand = products.Brand , 
            Category = products.Category, 
            Description = products.Description, 
            Thumbnail = products.Thumbnail
        }));
    }
}