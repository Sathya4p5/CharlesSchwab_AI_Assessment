using CourseRegistration.Models;
using CourseRegistration.Repositories;

namespace CourseRegistration.Tests
{
    public class CourseOfferingTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Constructor_CapacityBelowOne_Throws(int capacity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CourseOffering("O1", "Course", capacity));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void Constructor_BlankOfferingId_Throws(string offeringId)
        {
            Assert.Throws<ArgumentException>(() => new CourseOffering(offeringId, "Course", 1));
        }

        [Fact]
        public void Repository_AddingSameLearnerTwice_IsRejected()
        {
            var repository = new InMemoryEnrollmentRepository(new CourseOffering("O1", "Course", 1), []);
            repository.AddEnrollment(new Enrollment("L1", "O1"));

            Assert.Throws<InvalidOperationException>(() => repository.AddToWaitlist(new WaitlistEntry("L1", "O1")));
            Assert.Throws<InvalidOperationException>(() => repository.AddEnrollment(new Enrollment("L1", "O1")));
        }
    }
}
