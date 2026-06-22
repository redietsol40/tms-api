using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
 using TmsApi;
using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;                                                                                   


var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication("Training")
    .AddScheme<
        AuthenticationSchemeOptions,
        TrainingAuthHandler>(
        "Training",
        null);
builder.Services.AddDbContext<TmsDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("TmsDatabase"))
    .LogTo(Console.WriteLine, LogLevel.Information)
    .EnableSensitiveDataLogging());

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddOptions<paymentOptions>

()

.BindConfiguration("payments")
.ValidateDataAnnotations()
.ValidateOnStart();
//builder.Services.AddOpenApi();

builder.Services.AddAuthorization();

builder.Services.AddSingleton<
    IEnrollmentService,
    EnrollmentService>();

builder.Services.AddSingleton<
    EnrollmentWorker>();

var app = builder.Build();
app.UseExceptionHandler();

app.UseStatusCodePages();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/api/assessments/results",
    () =>
{
    return Results.Ok(new
    {
        courseCode = "CS-101",
        studentId = "S-001",
        letterGrade = "A"
    });
})
.RequireAuthorization();

app.MapGet(
    "/api/enrollments/worker-smoke",
    (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();

    return Results.Ok("processed");

});

app.MapControllers();
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});

if (app.Environment.IsDevelopment())
{
   // app.MapOpenApi();
    app.MapScalarApiReference();
}
using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
        .GetRequiredService<TmsDbContext>();

    context.Database.Migrate();

    if (!context.Students.Any())
    {
        var students = new List<Student>
        {
            new()
            {
                RegistrationNumber="TMS-2026-0001",
                Name="Alice Smith",
                GPA=3.8m,
                IsActive=true
            },

            new()
            {
                RegistrationNumber="TMS-2026-0002",
                Name="Bob Jones",
                GPA=2.9m,
                IsActive=true
            }
        };

        context.Students.AddRange(students);

        var courses = new List<Course>
        {
            new()
            {
                Code="CS-101",
                Title="Intro to CS",
                Capacity=30
            },

            new()
            {
                Code="CS-201",
                Title="Data Structures",
                Capacity=25
            }
        };

        context.Courses.AddRange(courses);

        context.SaveChanges();
    }
}

app.Run();

