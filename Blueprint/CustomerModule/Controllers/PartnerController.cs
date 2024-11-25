using Core.Base;
using Core.Utils.DB;
using Microsoft.AspNetCore.Mvc;

namespace Blueprint.CustomerModule.Controllers;

[Route("blueprint/[controller]")]
[ApiController]
public class PartnerController(
    ICache cache,
    ILogger<PartnerController> logger,
    IConnection dbConnection) : SuperController(cache, logger, dbConnection)
{
    protected override string CacheKeyRoot => "Blueprint.CustomerModule.Controllers.partner";

    // get api caller registered partner info
    [HttpGet]
    public IActionResult GetAuthPartner()
    {
        var partner = GetPartner();
        return Ok(partner);
    }
}