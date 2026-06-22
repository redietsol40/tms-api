using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly TmsDbContext context;

    public ReportsController(TmsDbContext context)
    {
        this.context = context;
    }

    // 1. Active Students Count
    [HttpGet("active-students-count")]
    public async Task<IActionResult> ActiveStudentsCount()
    {
        var count = await context.Students
            .Where(s => s.IsActive && s.GPA >= 3.0m)
            .CountAsync();

        return Ok(count);
    }

    // 2. Courses With Most Enrollments
    [HttpGet("most-enrolled-courses")]
    public async Task<IActionResult> MostEnrolledCourses()
    {
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .ToListAsync();

        return Ok(list);
    }

    // 3. Average GPA Per Course
    [HttpGet("average-gpa-per-course")]
    public async Task<IActionResult> AverageGpaPerCourse()
    {
        var list = await context.Enrollments
            .GroupBy(e => e.Course.Title)
            .Select(g => new
            {
                Course = g.Key,
                AverageGPA = g.Average(e => e.Student.GPA)
            })
            .ToListAsync();

        return Ok(list);
    }

    // 4A. Students With No Enrollments (Subquery)
    [HttpGet("students-no-enrollments-a")]
    public async Task<IActionResult> StudentsNoEnrollmentsA()
    {
        var list = await context.Students
            .Where(s => !s.Enrollments.Any())
            .Select(s => s.Name)
            .ToListAsync();

        return Ok(list);
    }

    // 4B. Students With No Enrollments (Left Join)
    [HttpGet("students-no-enrollments-b")]
    public async Task<IActionResult> StudentsNoEnrollmentsB()
    {
        var list = await context.Students
            .GroupJoin(
                context.Enrollments,
                s => s.Id,
                e => e.StudentId,
                (s, enrollments) => new
                {
                    Student = s,
                    Enrollments = enrollments
                })
            .Where(x => !x.Enrollments.Any())
            .Select(x => x.Student.Name)
            .ToListAsync();

        return Ok(list);
    }

    // Pagination
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;

        var students = await context.Students
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(students);
    }

    // Top 5 Courses
    [HttpGet("top-5-courses")]
    public async Task<IActionResult> Top5Courses()
    {
        var list = await context.Courses
            .Select(c => new
            {
                c.Title,
                EnrollmentCount = c.Enrollments.Count
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync();

        return Ok(list);
    }

    // Bulk Archive Old Enrollments
    [HttpPost("archive-old-enrollments")]
    public async Task<IActionResult> ArchiveOldEnrollments()
    {
        var cutoff = DateTime.UtcNow.AddMonths(-6);

        await context.Enrollments
            .Where(e => e.EnrolledAt < cutoff)
            .ExecuteUpdateAsync(
                s => s.SetProperty(
                    e => e.IsArchived,
                    true));

        return Ok("Archived");
    }

    // Admin View (Ignore Query Filters)
    [HttpGet("all-students-admin")]
    public async Task<IActionResult> AllStudentsAdmin()
    {
        var students = await context.Students
            .IgnoreQueryFilters()
            .ToListAsync();

        return Ok(students);
    }
}