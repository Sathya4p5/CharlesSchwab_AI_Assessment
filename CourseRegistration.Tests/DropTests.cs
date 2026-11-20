using CourseRegistration.Models;

namespace CourseRegistration.Tests
{
    public class DropTests
    {
        [Fact]
        public void Drop_EnrolledLearner_FreesSeat()
        {
            var h = new TestHelper(capacity: 2).Eligible("L1", "L2");
            h.EnrollAll("L1", "L2");

            var result = h.Service.Drop("L1");

            Assert.Equal(EnrollmentOutcome.Dropped, result.Outcome);
            Assert.Empty(result.Promoted);
            Assert.Equal(["L2"], h.Enrolled);
            Assert.Equal(1, h.SeatsAvailable);
        }

        [Fact]
        public void Drop_LearnerWhoNeverEnrolled_ReturnsNotEnrolledAndChangesNothing()
        {
            var h = new TestHelper(capacity: 1).Eligible("L1", "L2");
            h.Service.Enroll("L1");

            var result = h.Service.Drop("L2");

            Assert.Equal(EnrollmentOutcome.NotEnrolled, result.Outcome);
            Assert.Equal(["L1"], h.Enrolled);
        }

        [Fact]
        public void Drop_Twice_SecondCallReturnsNotEnrolledAndPromotesNobodyExtra()
        {
            var h = new TestHelper(capacity: 1).Eligible("L1", "L2", "L3");
            h.EnrollAll("L1", "L2", "L3");

            var first = h.Service.Drop("L1");
            var second = h.Service.Drop("L1");

            Assert.Equal(["L2"], first.Promoted);
            Assert.Equal(EnrollmentOutcome.NotEnrolled, second.Outcome);
            Assert.Empty(second.Promoted);
            Assert.Equal(["L2"], h.Enrolled);
            Assert.Equal(["L3"], h.Waitlist);
        }

        [Fact]
        public void Drop_WaitlistedLearner_LeavesWaitlistWithoutChangingSeats()
        {
            var h = new TestHelper(capacity: 1).Eligible("L1", "L2", "L3");
            h.EnrollAll("L1", "L2", "L3");

            var result = h.Service.Drop("L2");

            Assert.Equal(EnrollmentOutcome.LeftWaitlist, result.Outcome);
            Assert.Equal(["L1"], h.Enrolled);
            Assert.Equal(["L3"], h.Waitlist);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Drop_BlankLearnerId_ReturnsInvalidRequestAndChangesNothing(string? learnerId)
        {
            var h = new TestHelper(capacity: 1).Eligible("L1");
            h.Service.Enroll("L1");

            var result = h.Service.Drop(learnerId);

            Assert.Equal(EnrollmentOutcome.InvalidRequest, result.Outcome);
            Assert.Equal(["L1"], h.Enrolled);
        }

        [Fact]
        public void Drop_UnknownLearner_ReturnsLearnerNotFoundAndChangesNothing()
        {
            var h = new TestHelper(capacity: 1).Eligible("L1");
            h.Service.Enroll("L1");

            var result = h.Service.Drop("ghost");

            Assert.Equal(EnrollmentOutcome.LearnerNotFound, result.Outcome);
            Assert.Equal(["L1"], h.Enrolled);
        }

        [Fact]
        public void Drop_ThenEnrollAgain_GoesToBackOfWaitlist()
        {
            var h = new TestHelper(capacity: 1).Eligible("L1", "L2");
            h.EnrollAll("L1", "L2");

            h.Service.Drop("L2");
            var result = h.Service.Enroll("L2");

            Assert.Equal(EnrollmentOutcome.Waitlisted, result.Outcome);
            Assert.Equal(["L2"], h.Waitlist);
        }
    }
}
