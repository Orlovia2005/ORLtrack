using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using plt.Controllers;

namespace ORLtrack.Tests.Infrastructure;

internal static class HomeControllerTestHost
{
    public static HomeController Create(HomeControllerDependencies dependencies, int teacherId)
    {
        var controller = new HomeController(
            dependencies.Context,
            LoggerFactory.Create(builder => { }).CreateLogger<HomeController>(),
            new TestStudentRiskAiService());

        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, teacherId.ToString()),
                new Claim(ClaimTypes.Name, $"Teacher {teacherId}"),
                new Claim("Id", teacherId.ToString())
            }, "TestAuth"))
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        controller.TempData = new TempDataDictionary(httpContext, new TestTempDataProvider());

        return controller;
    }
}

internal sealed record HomeControllerDependencies(plt.Models.Model.EducationDbContext Context);
