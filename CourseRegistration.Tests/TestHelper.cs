using CourseRegistration.Repositories;
using CourseRegistration.Models;
using CourseRegistration.Services;

namespace CourseRegistration.Tests
{
    public class TestHelper
    {
        public const string Prerequisite = "CS101";

        public TestHelper(int capacity = 2)
        {
            Repository = new InMemoryEnrollmentRepository(
                new CourseOffering("CS201-2026", "Advanced C#", capacity, Prerequisite),
                []);
            Service = new EnrollmentService(Repository);
        }

        public InMemoryEnrollmentRepository Repository { get; }

        public EnrollmentService Service { get; }

        public string[] Enrolled => Service.GetStatus().Enrolled.ToArray();

        public string[] Waitlist => Service.GetStatus().Waitlist.ToArray();

        public int SeatsAvailable => Service.GetStatus().SeatsAvailable;

        public TestHelper Eligible(params string[] ids)
        {
            foreach (var id in ids)
            {
                Repository.AddLearner(new Learner(id, [Prerequisite]));
            }

            return this;
        }

        public TestHelper Ineligible(params string[] ids)
        {
            foreach (var id in ids)
            {
                Repository.AddLearner(new Learner(id, []));
            }

            return this;
        }

        public void Revoke(params string[] ids)
        {
            foreach (var id in ids)
            {
                Repository.FindLearner(id)!.CompletedCourseIds.Remove(Prerequisite);
            }
        }

        public void Restore(params string[] ids)
        {
            foreach (var id in ids)
            {
                Repository.FindLearner(id)!.CompletedCourseIds.Add(Prerequisite);
            }
        }

        public void EnrollAll(params string[] ids)
        {
            foreach (var id in ids)
            {
                Service.Enroll(id);
            }
        }
    }
}

