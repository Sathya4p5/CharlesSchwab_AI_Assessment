namespace CourseRegistration.Models
{
    public class WaitlistEntry
    {
        public WaitlistEntry(string learnerId, string offeringId)
        {
            LearnerId = learnerId;
            OfferingId = offeringId;
        }

        public string LearnerId { get; }

        public string OfferingId { get; }
    }
}
