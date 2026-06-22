using Microsoft.AspNetCore.Mvc;
using TmsApi.Data;

namespace TmsApi.Controllers;

//[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly TmsDbContext context;

    public TestController(TmsDbContext context)
    {
        this.context = context;
    }

    private static bool IsHonorRoll(decimal gpa)
    {
        return gpa >= 3.5m;
    }

    [HttpGet("deferred")]
    public IActionResult TestDeferred()
    {
        var query = context.Students
            .Where(s => s.GPA >= 3.0m);

        var orderedQuery = query
            .OrderBy(s => s.Name);

        var results = orderedQuery
            .ToList();

        return Ok(results);
    }

    [HttpGet("translation-fail")]
    public IActionResult TestTranslationFail()
    {
        var students = context.Students
            .Where(s => s.GPA >= 3.5m)
            .ToList();

        return Ok(students);
    }
}