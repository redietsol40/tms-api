namespace TmsApi;

public class EnrollmentWorker
{
    private readonly IEnrollmentService _service;

    public EnrollmentWorker(IEnrollmentService service)
    {
        _service = service;
    }

    public void ProcessBatch()
    {
        _service.EnrollAsync(
            "S-001",
            "CS-101").Wait();

        _service.EnrollAsync(
            "S-002",
            "CS-101").Wait();

        _service.EnrollAsync(
            "S-003",
            "CS-102").Wait();
    }
}