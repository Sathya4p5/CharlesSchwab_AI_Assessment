using CourseRegistration.DTOs;
using CourseRegistration.Interfaces;
using CourseRegistration.Models;

namespace CourseRegistration.Services
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _repository;
        private readonly object _gate = new();

        public EnrollmentService(IEnrollmentRepository repository) => _repository = repository;

        public EnrollmentResponse Enroll(string? learnerId)
        {
            if (string.IsNullOrWhiteSpace(learnerId))
            {
                return new EnrollmentResponse(EnrollmentOutcome.InvalidRequest, "Learner id is required.", learnerId);
            }

            lock (_gate)
            {
                var learner = _repository.FindLearner(learnerId);
                if (learner is null)
                {
                    return new EnrollmentResponse(EnrollmentOutcome.LearnerNotFound, $"Learner '{learnerId}' does not exist.", learnerId);
                }

                if (_repository.IsEnrolled(learnerId))
                {
                    return new EnrollmentResponse(EnrollmentOutcome.AlreadyEnrolled, "Learner is already enrolled.", learnerId);
                }

                if (_repository.IsWaitlisted(learnerId))
                {
                    return new EnrollmentResponse(EnrollmentOutcome.AlreadyWaitlisted, "Learner is already on the waitlist.", learnerId, PositionOf(learnerId));
                }

                if (!IsEligible(learner))
                {
                    return new EnrollmentResponse(EnrollmentOutcome.NotEligible, "Learner has not completed the prerequisite.", learnerId);
                }

                var offeringId = _repository.Offering.OfferingId;
                if (SeatsAvailable() > 0)
                {
                    _repository.AddEnrollment(new Enrollment(learnerId, offeringId));
                    return new EnrollmentResponse(EnrollmentOutcome.Enrolled, "Learner enrolled.", learnerId);
                }

                _repository.AddToWaitlist(new WaitlistEntry(learnerId, offeringId));
                return new EnrollmentResponse(EnrollmentOutcome.Waitlisted, "Course is full; learner added to the waitlist.", learnerId, PositionOf(learnerId));
            }
        }

        public DropResponse Drop(string? learnerId)
        {
            if (string.IsNullOrWhiteSpace(learnerId))
            {
                return new DropResponse(EnrollmentOutcome.InvalidRequest, "Learner id is required.", learnerId, []);
            }

            lock (_gate)
            {
                if (_repository.FindLearner(learnerId) is null)
                {
                    return new DropResponse(EnrollmentOutcome.LearnerNotFound, $"Learner '{learnerId}' does not exist.", learnerId, []);
                }

                if (_repository.RemoveFromWaitlist(learnerId))
                {
                    return new DropResponse(EnrollmentOutcome.LeftWaitlist, "Learner removed from the waitlist.", learnerId, []);
                }

                if (!_repository.RemoveEnrollment(learnerId))
                {
                    return new DropResponse(EnrollmentOutcome.NotEnrolled, "Learner has no enrollment or waitlist entry.", learnerId, []);
                }

                return new DropResponse(EnrollmentOutcome.Dropped, "Learner dropped; seat released.", learnerId, PromoteFromWaitlist());
            }
        }

        public OfferingStatusResponse GetStatus()
        {
            lock (_gate)
            {
                var offering = _repository.Offering;
                var enrolled = _repository.GetEnrollments().Select(e => e.LearnerId).ToList();
                var waitlist = _repository.GetWaitlist().Select(w => w.LearnerId).ToList();

                return new OfferingStatusResponse(offering.CourseName, offering.Capacity, offering.Capacity - enrolled.Count, enrolled, waitlist);
            }
        }

        // Skip-and-retain: walk the waitlist front to back, promote eligible learners while seats remain,
        // and leave ineligible learners untouched so they keep their position. Caller holds the lock.
        private List<string> PromoteFromWaitlist()
        {
            var promoted = new List<string>();
            var seats = SeatsAvailable();

            foreach (var entry in _repository.GetWaitlist())
            {
                if (seats == 0)
                {
                    break;
                }

                var learner = _repository.FindLearner(entry.LearnerId);
                if (learner is null || !IsEligible(learner))
                {
                    continue;
                }

                _repository.RemoveFromWaitlist(entry.LearnerId);
                _repository.AddEnrollment(new Enrollment(entry.LearnerId, entry.OfferingId));
                promoted.Add(entry.LearnerId);
                seats--;
            }

            return promoted;
        }

        private bool IsEligible(Learner learner)
        {
            var required = _repository.Offering.RequiredPrerequisiteCourseId;
            return required is null || learner.CompletedCourseIds.Contains(required);
        }

        private int SeatsAvailable() => _repository.Offering.Capacity - _repository.EnrolledCount();

        private int PositionOf(string learnerId) =>
            _repository.GetWaitlist().ToList().FindIndex(w => w.LearnerId == learnerId) + 1;
    }

}
