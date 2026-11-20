namespace CourseRegistration.DTOs
{
    public class OfferingStatusResponse
    {
        public OfferingStatusResponse(string courseName, int capacity, int seatsAvailable, IReadOnlyList<string> enrolled, IReadOnlyList<string> waitlist)
        {
            CourseName = courseName;
            Capacity = capacity;
            SeatsAvailable = seatsAvailable;
            Enrolled = enrolled;
            Waitlist = waitlist;
        }

        public string CourseName { get; }

        public int Capacity { get; }

        public int SeatsAvailable { get; }

        public IReadOnlyList<string> Enrolled { get; }

        public IReadOnlyList<string> Waitlist { get; }
    }
}
namespace CourseRegistration.DTOs
{
    public class OrderingStatusResponse
    {
    }
}
