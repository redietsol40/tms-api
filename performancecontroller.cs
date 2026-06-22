using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/performance")]
public class PerformanceController : ControllerBase
{
    private readonly TmsDbContext db;

    public PerformanceController(TmsDbContext db)
    {
        this.db = db;
    }

    [HttpGet("nplus1")]
    public async Task<IActionResult> NPlusOne(
        CancellationToken cancellationToken)
    {
        var students = await db.Students
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var s in students)
        {
            var count = await db.Enrollments
                .AsNoTracking()
                .CountAsync(
                    e => e.StudentId == s.Id,
                    cancellationToken);

            Console.WriteLine(
                $"{s.Name}: {count} enrollments");
        }

        return Ok("Check console log");
    }

    [HttpGet("nplus1-fixed")]
    public async Task<IActionResult> NPlusOneFixed(
        CancellationToken cancellationToken)
    {
        var report = await db.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.Name,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync(cancellationToken);

        return Ok(report);
    }
}