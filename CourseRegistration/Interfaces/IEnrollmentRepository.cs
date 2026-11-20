using CourseRegistration.Models;

namespace CourseRegistration.Interfaces
{
    public interface IEnrollmentRepository
    {
        CourseOffering Offering { get; }

        Learner? FindLearner(string learnerId);

        bool IsEnrolled(string learnerId);

        bool IsWaitlisted(string learnerId);

        int EnrolledCount();

        IReadOnlyList<Enrollment> GetEnrollments();

        // Returns a copy in FIFO order, so callers can modify the waitlist while iterating.
        IReadOnlyList<WaitlistEntry> GetWaitlist();

        void AddEnrollment(Enrollment enrollment);

        bool RemoveEnrollment(string learnerId);

        void AddToWaitlist(WaitlistEntry entry);

        bool RemoveFromWaitlist(string learnerId);
    }
}
