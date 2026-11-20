using CourseRegistration.Models;

namespace CourseRegistration.DTOs
{
    public class EnrollmentResponse
    {
        public EnrollmentResponse(EnrollmentOutcome outcome, string message, string? learnerId, int? waitlistPosition = null)
        {
            Outcome = outcome;
            Message = message;
            LearnerId = learnerId;
            WaitlistPosition = waitlistPosition;
        }

        public EnrollmentOutcome Outcome { get; }

        public string Message { get; }

        public string? LearnerId { get; }

        public int? WaitlistPosition { get; }

    }
}