namespace CourseRegistration.Models
{
    public class Learner
    {
        public Learner(string learnerId, IEnumerable<string> completedCourseIds)
        {
            LearnerId = learnerId;
            CompletedCourseIds = new HashSet<string>(completedCourseIds, StringComparer.OrdinalIgnoreCase);
        }

        public string LearnerId { get; }

        public HashSet<string> CompletedCourseIds { get; }
    }
}
