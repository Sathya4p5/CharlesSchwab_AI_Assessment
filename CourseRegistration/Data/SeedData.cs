using CourseRegistration.Models;
using CourseRegistration.Repositories;

namespace CourseRegistration.Data
{
    public class SeedData
    {
        public const string Prerequisite = "CS101";

        // One offering (capacity 2). L1-L5 have the prerequisite; L6-L7 do not.
        public static InMemoryEnrollmentRepository CreateRepository() =>
            new(
                new CourseOffering("CS201-2026", "Advanced C#", capacity: 2, requiredPrerequisiteCourseId: Prerequisite),
                [
                    new Learner("L1", [Prerequisite]),
                new Learner("L2", [Prerequisite]),
                new Learner("L3", [Prerequisite]),
                new Learner("L4", [Prerequisite]),
                new Learner("L5", [Prerequisite]),
                new Learner("L6", []),
                new Learner("L7", [])
                ]);
    }
}
