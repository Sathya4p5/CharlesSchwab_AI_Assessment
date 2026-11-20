using CourseRegistration.Interfaces;
using CourseRegistration.Models;

namespace CourseRegistration.Repositories
{

    public class InMemoryEnrollmentRepository : IEnrollmentRepository
    {
        private readonly Dictionary<string, Learner> _learners = new(StringComparer.Ordinal);
        private readonly List<Enrollment> _enrollments = new();
        private readonly List<WaitlistEntry> _waitlist = new();

        public InMemoryEnrollmentRepository(CourseOffering offering, IEnumerable<Learner> learners)
        {
            Offering = offering;
            foreach (var learner in learners)
            {
                AddLearner(learner);
            }
        }

        public CourseOffering Offering { get; }

        public void AddLearner(Learner learner) => _learners[learner.LearnerId] = learner;

        public Learner? FindLearner(string learnerId) => _learners.GetValueOrDefault(learnerId);

        public bool IsEnrolled(string learnerId) => _enrollments.Exists(e => e.LearnerId == learnerId);

        public bool IsWaitlisted(string learnerId) => _waitlist.Exists(w => w.LearnerId == learnerId);

        public int EnrolledCount() => _enrollments.Count;

        public IReadOnlyList<Enrollment> GetEnrollments() => _enrollments.ToList();

        public IReadOnlyList<WaitlistEntry> GetWaitlist() => _waitlist.ToList();

        public void AddEnrollment(Enrollment enrollment)
        {
            if (IsEnrolled(enrollment.LearnerId) || IsWaitlisted(enrollment.LearnerId))
            {
                throw new InvalidOperationException($"Learner '{enrollment.LearnerId}' already has a record.");
            }

            _enrollments.Add(enrollment);
        }

        public bool RemoveEnrollment(string learnerId) => _enrollments.RemoveAll(e => e.LearnerId == learnerId) > 0;

        public void AddToWaitlist(WaitlistEntry entry)
        {
            if (IsEnrolled(entry.LearnerId) || IsWaitlisted(entry.LearnerId))
            {
                throw new InvalidOperationException($"Learner '{entry.LearnerId}' already has a record.");
            }

            _waitlist.Add(entry);
        }

        public bool RemoveFromWaitlist(string learnerId) => _waitlist.RemoveAll(w => w.LearnerId == learnerId) > 0;
    }

}
