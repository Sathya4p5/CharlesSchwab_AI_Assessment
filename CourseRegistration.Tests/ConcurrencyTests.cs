using CourseRegistration.DTOs;
using CourseRegistration.Models;

namespace CourseRegistration.Tests
{
    public class ConcurrencyTests
    {
        private const int Rounds = 50;

        private static string[] Ids(int count, string prefix = "L") =>
            Enumerable.Range(1, count).Select(i => $"{prefix}{i}").ToArray();

        [Fact]
        public void ParallelEnrolls_NeverExceedCapacity_AndEveryoneElseIsWaitlistedOnce()
        {
            const int capacity = 5;
            var ids = Ids(60);

            for (var round = 0; round < Rounds; round++)
            {
                var h = new TestHelper(capacity).Eligible(ids);

                var results = new EnrollmentResponse[ids.Length];
                Parallel.For(0, ids.Length, i => results[i] = h.Service.Enroll(ids[i]));

                Assert.Equal(capacity, results.Count(r => r.Outcome == EnrollmentOutcome.Enrolled));
                Assert.Equal(ids.Length - capacity, results.Count(r => r.Outcome == EnrollmentOutcome.Waitlisted));
                Assert.Equal(capacity, h.Enrolled.Length);
                Assert.Equal(ids.Length, h.Enrolled.Concat(h.Waitlist).Distinct().Count());
                Assert.Equal(
                    Enumerable.Range(1, ids.Length - capacity),
                    results.Where(r => r.Outcome == EnrollmentOutcome.Waitlisted).Select(r => r.WaitlistPosition!.Value).Order());
            }
        }

        [Fact]
        public void ParallelDuplicateEnrollsOfSameLearner_CreateExactlyOneRecord()
        {
            for (var round = 0; round < Rounds; round++)
            {
                var h = new TestHelper(capacity: 3).Eligible("L1");

                var results = new EnrollmentResponse[50];
                Parallel.For(0, results.Length, i => results[i] = h.Service.Enroll("L1"));

                Assert.Equal(1, results.Count(r => r.Outcome == EnrollmentOutcome.Enrolled));
                Assert.Equal(results.Length - 1, results.Count(r => r.Outcome == EnrollmentOutcome.AlreadyEnrolled));
                Assert.Equal(["L1"], h.Enrolled);
            }
        }

        [Fact]
        public void ParallelRepeatedDropsOfSameLearner_PromoteExactlyOneWaitlistedLearner()
        {
            for (var round = 0; round < Rounds; round++)
            {
                var h = new TestHelper(capacity: 1).Eligible("L1", "L2", "L3", "L4");
                h.EnrollAll("L1", "L2", "L3", "L4");

                var results = new DropResponse[50];
                Parallel.For(0, results.Length, i => results[i] = h.Service.Drop("L1"));

                Assert.Equal(1, results.Count(r => r.Outcome == EnrollmentOutcome.Dropped));
                Assert.Equal(["L2"], h.Enrolled);
                Assert.Equal(["L3", "L4"], h.Waitlist);
            }
        }

        [Fact]
        public void MixedParallelEnrollsDropsAndReads_KeepInvariants()
        {
            const int capacity = 5;
            for (var round = 0; round < Rounds; round++)
            {
                var initial = Ids(40);
                var newcomers = Ids(30, "N");
                var h = new TestHelper(capacity).Eligible(initial).Eligible(newcomers);
                h.EnrollAll(initial);
                var droppers = h.Enrolled;

                var work = droppers.Select(id => (Action)(() => h.Service.Drop(id)))
                    .Concat(newcomers.Select(id => (Action)(() => h.Service.Enroll(id))))
                    .Concat(initial.Select(id => (Action)(() => h.Service.Enroll(id))))
                    .Concat(Enumerable.Range(0, 20).Select(_ => (Action)(() => h.Service.GetStatus())));

                Parallel.Invoke(work.ToArray());

                var status = h.Service.GetStatus();
                var everyone = status.Enrolled.Concat(status.Waitlist).ToArray();
                Assert.True(status.Enrolled.Count <= capacity);
                Assert.Equal(everyone.Length, everyone.Distinct().Count());
                Assert.Equal(capacity - status.Enrolled.Count, status.SeatsAvailable);
                Assert.True(status.SeatsAvailable == 0 || status.Waitlist.Count == 0);
            }
        }
    }
}

