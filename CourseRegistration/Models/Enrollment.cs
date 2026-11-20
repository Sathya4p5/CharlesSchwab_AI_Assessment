namespace CourseRegistration.Models
{
    public class Enrollment
    {
        public Enrollment(string learnerId, string offeringId)
        {
            LearnerId = learnerId;
            OfferingId = offeringId;
        }

        public string LearnerId { get; }

        public string OfferingId { get; }
    }
}
