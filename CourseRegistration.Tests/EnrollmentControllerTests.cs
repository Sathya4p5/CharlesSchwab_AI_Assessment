using CourseRegistration.DTOs;
using CourseRegistration.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CourseRegistration.Tests
{
    public class EnrollmentControllerTests : IDisposable
    {
        private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly WebApplicationFactory<Program> _factory = new();
        private readonly HttpClient _client;

        public EnrollmentControllerTests() => _client = _factory.CreateClient();

        public void Dispose()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        private Task<HttpResponseMessage> Enroll(string? learnerId) =>
            _client.PostAsJsonAsync("/api/enrollment", new { learnerId }, Json);

        private static async Task<T> Read<T>(HttpResponseMessage response) =>
            (await response.Content.ReadFromJsonAsync<T>(Json))!;

        [Fact]
        public async Task Post_WithSeatAvailable_Returns201Enrolled()
        {
            var response = await Enroll("L1");

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(EnrollmentOutcome.Enrolled, (await Read<EnrollmentResponse>(response)).Outcome);
        }

        [Fact]
        public async Task Post_WhenFull_Returns202WithWaitlistPosition()
        {
            await Enroll("L1");
            await Enroll("L2");

            var response = await Enroll("L3");

            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.Equal(1, (await Read<EnrollmentResponse>(response)).WaitlistPosition);
        }

        [Fact]
        public async Task Post_Duplicate_Returns409()
        {
            await Enroll("L1");

            var response = await Enroll("L1");

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal(EnrollmentOutcome.AlreadyEnrolled, (await Read<EnrollmentResponse>(response)).Outcome);
        }

        [Fact]
        public async Task Post_IneligibleLearner_Returns422()
        {
            var response = await Enroll("L6");

            Assert.Equal((HttpStatusCode)422, response.StatusCode);
            Assert.Equal(EnrollmentOutcome.NotEligible, (await Read<EnrollmentResponse>(response)).Outcome);
        }

        [Fact]
        public async Task Post_UnknownLearner_Returns404()
        {
            var response = await Enroll("ghost");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Post_BlankLearnerId_Returns400(string? learnerId)
        {
            var response = await Enroll(learnerId);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Delete_EnrolledLearner_Returns200AndReportsPromotion()
        {
            await Enroll("L1");
            await Enroll("L2");
            await Enroll("L3");

            var response = await _client.DeleteAsync("/api/enrollment/L1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(["L3"], (await Read<DropResponse>(response)).Promoted);
        }

        [Fact]
        public async Task Delete_Repeated_Returns404()
        {
            await Enroll("L1");
            await _client.DeleteAsync("/api/enrollment/L1");

            var response = await _client.DeleteAsync("/api/enrollment/L1");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(EnrollmentOutcome.NotEnrolled, (await Read<DropResponse>(response)).Outcome);
        }

        [Fact]
        public async Task Delete_WaitlistedLearner_Returns200()
        {
            await Enroll("L1");
            await Enroll("L2");
            await Enroll("L3");

            var response = await _client.DeleteAsync("/api/enrollment/L3");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(EnrollmentOutcome.LeftWaitlist, (await Read<DropResponse>(response)).Outcome);
        }

        [Fact]
        public async Task Delete_BlankLearnerId_Returns400()
        {
            var response = await _client.DeleteAsync("/api/enrollment/%20");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Get_ReturnsStatusSnapshot()
        {
            await Enroll("L1");
            await Enroll("L2");
            await Enroll("L3");

            var response = await _client.GetAsync("/api/enrollment");
            var status = await Read<OfferingStatusResponse>(response);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("Advanced C#", status.CourseName);
            Assert.Equal(["L1", "L2"], status.Enrolled);
            Assert.Equal(["L3"], status.Waitlist);
            Assert.Equal(0, status.SeatsAvailable);
        }
    }
}

