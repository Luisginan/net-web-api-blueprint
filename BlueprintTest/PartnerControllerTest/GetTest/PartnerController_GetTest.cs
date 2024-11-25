using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Core.Utils.DB;
using ICache = Core.Utils.DB.ICache;
using Blueprint.CustomerModule.Controllers;
using Core.Base;
using Microsoft.AspNetCore.Http;

namespace BlueprintTest.PartnerControllerTest.GetTest;

public class PartnerControllerGetTest
{

    [Fact]
    public void Get_PartnerSuccess()
    {
        var mockCache = new Mock<ICache>();
        var mockLogger = new Mock<ILogger<PartnerController>>();
        var mockDbConnection = new Mock<IConnection>();

        // not found in cache
        mockCache.Setup(x => x.GetStringAsync(It.IsAny<string>()))
            .ReturnsAsync((string?)null);
        var header = new Partner
        {
            AppId = "OCTO-CIMB",
            PartnerName = "OCTO",
            PrincipleName = "CIMB",
            Roles = "admin"
        };

           
        // init controller
        var httpContext = new DefaultHttpContext();
       
        httpContext.Request.Headers["api-key"] = "CIMB-123214213";
        httpContext.Request.Headers["principleName"] = header.PrincipleName;
        httpContext.Request.Headers["partnerName"] = header.PartnerName;
        httpContext.Request.Headers["roles"] = header.Roles;
        httpContext.Request.Headers["appId"] = header.AppId;
           
        var controller = new PartnerController(mockCache.Object, mockLogger.Object, mockDbConnection.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            }
        };

        // Act
        var result = controller.GetAuthPartner();

        // Assert result should be ok and return the correct customer from database
        var viewResult = Assert.IsType<OkObjectResult>(result);
        var responseDto = Assert.IsAssignableFrom<Partner>(viewResult.Value);

           
        Assert.Equal("OCTO-CIMB", responseDto.AppId);
        Assert.Equal("OCTO", responseDto.PartnerName);
        Assert.Equal("CIMB", responseDto.PrincipleName);
        Assert.Equal("admin", responseDto.Roles);
    }
}