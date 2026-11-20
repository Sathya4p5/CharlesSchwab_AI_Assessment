namespace CourseRegistration.Models
{
    public class CourseOffering
    {
        public CourseOffering(string offeringId, string courseName, int capacity, string? requiredPrerequisiteCourseId = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(offeringId);
            ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1);

            OfferingId = offeringId;
            CourseName = courseName;
            Capacity = capacity;
            RequiredPrerequisiteCourseId = requiredPrerequisiteCourseId;
        }

        public string OfferingId { get; }

        public string CourseName { get; }

        public int Capacity { get; }

        // Null means the offering has no prerequisite.
        public string? RequiredPrerequisiteCourseId { get; }
    }
}
