using CourseRegistration.Models;

namespace CourseRegistration.Tests
{
    public class WaitListTests
    {
        [Fact]
        public void Drop_PromotesEarliestWaitlistedLearner()
        {
            var h = new TestHelper(capacity: 1).Eligible("L1", "L2", "L3", "L4");
            h.EnrollAll("L1", "L2", "L3", "L4");

            var result = h.Service.Drop("L1");

            Assert.Equal(["L2"], result.Promoted);
            Assert.Equal(["L2"], h.Enrolled);
            Assert.Equal(["L3", "L4"], h.Waitlist);
        }

        [Fact]
        public void TwoSeatsFreed_SkipsIneligibleAndKeepsTheirOrder()
        {
            // Waitlist A,B,C,D,E where B and C lose eligibility after joining.
            var h = new TestHelper(capacity: 2).Eligible("X", "Y", "A", "B", "C", "D", "E");
            h.EnrollAll("X", "Y", "A", "B", "C", "D", "E");
            h.Revoke("B", "C");

            var first = h.Service.Drop("X");
            var second = h.Service.Drop("Y");

            Assert.Equal(["A"], first.Promoted);
            Assert.Equal(["D"], second.Promoted);
            Assert.Equal(["A", "D"], h.Enrolled);
            Assert.Equal(["B", "C", "E"], h.Waitlist);
        }

        [Fact]
        public void ThreeSeatsFreed_PromotesEligibleAndRetainsIneligibleInOrder()
        {
            var h = new TestHelper(capacity: 3).Eligible("X", "Y", "Z", "A", "B", "C", "D", "E");
            h.EnrollAll("X", "Y", "Z", "A", "B", "C", "D", "E");
            h.Revoke("B", "C");

            h.Service.Drop("X");
            h.Service.Drop("Y");
            h.Service.Drop("Z");

            Assert.Equal(["A", "D", "E"], h.Enrolled);
            Assert.Equal(["B", "C"], h.Waitlist);
        }

        [Fact]
        public void Drop_WhenNobodyWaitingIsEligible_PromotesNoOneAndKeepsWaitlistUnchanged()
        {
            var h = new TestHelper(capacity: 1).Eligible("X", "A", "B");
            h.EnrollAll("X", "A", "B");
            h.Revoke("A", "B");

            var result = h.Service.Drop("X");

            Assert.Empty(result.Promoted);
            Assert.Empty(h.Enrolled);
            Assert.Equal(["A", "B"], h.Waitlist);
            Assert.Equal(1, h.SeatsAvailable);
        }

        [Fact]
        public void RetainedLearnerRegainingEligibility_KeepsOriginalPlaceInQueue()
        {
            var h = new TestHelper(capacity: 1).Eligible("X", "A", "B");
            h.EnrollAll("X", "A", "B");
            h.Revoke("A");
            h.Restore("A");

            var result = h.Service.Drop("X");

            Assert.Equal(["A"], result.Promoted);
            Assert.Equal(["B"], h.Waitlist);
        }

        [Fact]
        public void Enroll_WhenSeatFreeAndOnlyIneligibleLearnersWait_EnrollsNewcomerDirectly()
        {
            var h = new TestHelper(capacity: 1).Eligible("X", "A", "N");
            h.EnrollAll("X", "A");
            h.Revoke("A");
            h.Service.Drop("X");

            var result = h.Service.Enroll("N");

            Assert.Equal(EnrollmentOutcome.Enrolled, result.Outcome);
            Assert.Equal(["A"], h.Waitlist);
        }
    }
}
