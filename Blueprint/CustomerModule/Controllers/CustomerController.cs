using AutoMapper;
using Blueprint.CustomerModule.DTO;
using Blueprint.CustomerModule.Models;
using Blueprint.CustomerModule.Services;
using Core.Base;
using Core.Utils.DB;
using Microsoft.AspNetCore.Mvc;

namespace Blueprint.CustomerModule.Controllers;

[Route("blueprint/[controller]")]
[ApiController]
public class CustomerController(ICustomerService customerService,
    ICache cache, 
    ILogger<CustomerController> logger, 
    IConnection dbConnection,
    IMapper mapper) : SuperController(cache, logger, dbConnection)
{
    protected override string CacheKeyRoot => "Blueprint.CustomerModule.Controllers.customer";
        
    [HttpGet("")]
    public async Task<IActionResult> Get()
    {
        var customers = await UseListCacheAsync("listCustomer",customerService.GetCustomersAsync);
        return Ok(mapper.Map<List<CustomerResponse>>(customers));

    }

    // GET api/<CustomerController>/5
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var customer = await UseCacheAsync(id.ToString(),() => customerService.GetCustomerAsync(id));

        if (customer == null)
            return NotFound();
            
        return Ok(mapper.Map<CustomerResponse>(customer));
    }

    // POST api/<CustomerController>
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CustomerRequest value)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await ClearCacheAsync(value.Email,() => customerService.InsertCustomerAsync(mapper.Map<Customer>(value)));

        var customerOnDb = await UseCacheAsync(value.Email,() => customerService.GetCustomerByEmailAsync(value.Email));

        if (customerOnDb == null)
            return BadRequest("Customer failed to be created");

        var customerResponse = new CustomerResponse
        {
            Id = customerOnDb.Id,
            Name = customerOnDb.Name,
            Address = customerOnDb.Address,
            Email = customerOnDb.Email2,
            Phone = customerOnDb.Phone
        };
        return Accepted(customerResponse);
    }

    // PUT api/<CustomerController>/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Put(int id, [FromBody] CustomerRequest value)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = await UseCacheAsync(id.ToString(), () => customerService.GetCustomerAsync(id));
        
        if (customer == null)
            return NotFound();

        await ClearCacheAsync(id.ToString(), () => customerService.UpdateCustomerAsync(mapper.Map<Customer>(value), id));
            
        return Accepted();
    }

    // DELETE api/<CustomerController>/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await customerService.GetCustomerAsync(id);
        if (customer == null)
            return NotFound();
        
        await ClearCacheAsync(id.ToString(), () => customerService.DeleteCustomerAsync(id));
     
        return Accepted();
    }

    //get customer by email
    [HttpGet("GetByEmail/{email}")]
    public async Task<IActionResult> GetByEmail(string email)
    {
        var customer = await UseCacheAsync(email, () => customerService.GetCustomerByEmailAsync(email));

        if (customer == null)
            return NotFound();
        
        return Ok(mapper.Map<CustomerResponse>(customer));
    }

    // get customer by name and phone using query parameters
    [HttpGet("GetByNameAndPhone")]
    public async Task<IActionResult> GetByNameAndPhone([FromQuery] string name, [FromQuery] string phone)
    {

        var customer  = await UseCacheAsync(name + phone,
            () => customerService.GetCustomerByNameAndPhoneAsync(name, phone));

        if (customer == null)
            return NotFound();
        
        return Ok(mapper.Map<CustomerResponse>(customer));
    }
}