using CourseRegistration.Models;

namespace CourseRegistration.DTOs
{
    public class DropResponse
    {
        public DropResponse(EnrollmentOutcome outcome, string message, string? learnerId, IReadOnlyList<string> promoted)
        {
            Outcome = outcome;
            Message = message;
            LearnerId = learnerId;
            Promoted = promoted;
        }

        public EnrollmentOutcome Outcome { get; }

        public string Message { get; }

        public string? LearnerId { get; }

        public IReadOnlyList<string> Promoted { get; }
    }
}
