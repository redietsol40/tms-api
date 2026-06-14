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

builder.Services.AddAuthorization();

builder.Services.AddControllers();

builder.Services.AddProblemDetails();

//builder.Services.AddOpenApi();

//builder.Services.AddAuthorization();

builder.Services.AddSingleton<
    IEnrollmentService,
    EnrollmentService>();

//builder.Services.AddSingleton<
    //EnrollmentWorker>();

var app = builder.Build();
//app.UseExceptionHandler();

//app.UseStatusCodePages();

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

/*app.MapGet(
    "/api/enrollments/worker-smoke",
    (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();

    return Results.Ok("processed");

});
*/
app.MapControllers();
app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException(
        "Simulated database failure for ProblemDetails testing");
});
/*
if (app.Environment.IsDevelopment())
{
   // app.MapOpenApi();
    app.MapScalarApiReference();
}*/


app.Run();

