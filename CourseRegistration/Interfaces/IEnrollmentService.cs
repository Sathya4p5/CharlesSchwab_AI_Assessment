using CourseRegistration.DTOs;

namespace CourseRegistration.Interfaces
{
    public interface IEnrollmentService
    {
        EnrollmentResponse Enroll(string? learnerId);

        DropResponse Drop(string? learnerId);

        OfferingStatusResponse GetStatus();
    }
}
