using CourseRegistration.Models;
using CourseRegistration.Repositories;
using CourseRegistration.Services;

namespace CourseRegistration.Tests
{
    public class EnrollmentTests
    {
        [Fact]
        public void Enroll_WithSeatAvailable_Enrolls()
        {
            var h = new TestHelper(capacity: 2).Eligible("L1");

            var result = h.Service.Enroll("L1");

            Assert.Equal(EnrollmentOutcome.Enrolled, result.Outcome);
            Assert.Equal(["L1"], h.Enrolled);
            Assert.Equal(1, h.SeatsAvailable);
        }

        [Fact]
        public void Enroll_WhenFull_JoinsWaitlistInOrderWithPosition()
        {
            var h = new TestHelper(capacity: 1).Eligible("L1", "L2", "L3");
            h.Service.Enroll("L1");

            var second = h.Service.Enroll("L2");
            var third = h.Service.Enroll("L3");

            Assert.Equal(EnrollmentOutcome.Waitlisted, second.Outcome);
            Assert.Equal(1, second.WaitlistPosition);
            Assert.Equal(2, third.WaitlistPosition);
            Assert.Equal(["L1"], h.Enrolled);
            Assert.Equal(["L2", "L3"], h.Waitlist);
        }

        [Fact]
        public void Enroll_Twice_ReturnsAlreadyEnrolledAndCreatesNoDuplicate()
        {
            var h = new TestHelper(capacity: 2).Eligible("L1");
            h.Service.Enroll("L1");

            var result = h.Service.Enroll("L1");

            Assert.Equal(EnrollmentOutcome.AlreadyEnrolled, result.Outcome);
            Assert.Equal(["L1"], h.Enrolled);
            Assert.Empty(h.Waitlist);
        }

        [Fact]
        public void Enroll_WhenAlreadyWaitlisted_ReturnsAlreadyWaitlistedWithSamePosition()
        {
            var h = new TestHelper(capacity: 1).Eligible("L1", "L2", "L3");
            h.EnrollAll("L1", "L2", "L3");

            var result = h.Service.Enroll("L2");

            Assert.Equal(EnrollmentOutcome.AlreadyWaitlisted, result.Outcome);
            Assert.Equal(1, result.WaitlistPosition);
            Assert.Equal(["L2", "L3"], h.Waitlist);
        }

        [Fact]
        public void Enroll_IneligibleWithSeatAvailable_IsRejectedAndNotRecorded()
        {
            var h = new TestHelper(capacity: 2).Ineligible("L1");

            var result = h.Service.Enroll("L1");

            Assert.Equal(EnrollmentOutcome.NotEligible, result.Outcome);
            Assert.Empty(h.Enrolled);
            Assert.Empty(h.Waitlist);
            Assert.Equal(2, h.SeatsAvailable);
        }

        [Fact]
        public void Enroll_IneligibleWhenFull_IsNotWaitlisted()
        {
            var h = new TestHelper(capacity: 1).Eligible("L1").Ineligible("L2");
            h.Service.Enroll("L1");

            var result = h.Service.Enroll("L2");

            Assert.Equal(EnrollmentOutcome.NotEligible, result.Outcome);
            Assert.Empty(h.Waitlist);
        }

        [Fact]
        public void Enroll_RejectedLearnerWhoLaterBecomesEligible_MustEnrollAgain()
        {
            var h = new TestHelper(capacity: 2).Ineligible("L1");
            Assert.Equal(EnrollmentOutcome.NotEligible, h.Service.Enroll("L1").Outcome);

            h.Restore("L1");
            Assert.Empty(h.Enrolled);

            Assert.Equal(EnrollmentOutcome.Enrolled, h.Service.Enroll("L1").Outcome);
            Assert.Equal(["L1"], h.Enrolled);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Enroll_BlankLearnerId_ReturnsInvalidRequestAndChangesNothing(string? learnerId)
        {
            var h = new TestHelper().Eligible("L1");

            var result = h.Service.Enroll(learnerId);

            Assert.Equal(EnrollmentOutcome.InvalidRequest, result.Outcome);
            Assert.Empty(h.Enrolled);
            Assert.Empty(h.Waitlist);
        }

        [Fact]
        public void Enroll_UnknownLearner_ReturnsLearnerNotFoundAndChangesNothing()
        {
            var h = new TestHelper().Eligible("L1");

            var result = h.Service.Enroll("ghost");

            Assert.Equal(EnrollmentOutcome.LearnerNotFound, result.Outcome);
            Assert.Empty(h.Enrolled);
            Assert.Empty(h.Waitlist);
        }

        [Fact]
        public void Enroll_WhenOfferingHasNoPrerequisites_EveryKnownLearnerIsEligible()
        {
            var repository = new InMemoryEnrollmentRepository(
                new CourseOffering("OPEN100-2026", "Open course", 1),
                [new Learner("L1", [])]);
            var service = new EnrollmentService(repository);

            Assert.Equal(EnrollmentOutcome.Enrolled, service.Enroll("L1").Outcome);
        }
    }
}
