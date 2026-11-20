using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CourseRegistration.Interfaces;
using CourseRegistration.DTOs;
using CourseRegistration.Models;

namespace CourseRegistration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private const int UnprocessableContent = 422;

        private readonly IEnrollmentService _service;

        public EnrollmentController(IEnrollmentService service) => _service = service;

        [HttpPost]
        public IActionResult Enroll([FromBody] EnrollmentRequest request)
        {
            var response = _service.Enroll(request.LearnerId);
            return StatusCode(ToStatusCode(response.Outcome), response);
        }

        [HttpDelete("{learnerId}")]
        public IActionResult Drop(string learnerId)
        {
            var response = _service.Drop(learnerId);
            return StatusCode(ToStatusCode(response.Outcome), response);
        }

        [HttpGet]
        public ActionResult<OfferingStatusResponse> GetStatus() => _service.GetStatus();

        private static int ToStatusCode(EnrollmentOutcome outcome) => outcome switch
        {
            EnrollmentOutcome.Enrolled => StatusCodes.Status201Created,
            EnrollmentOutcome.Waitlisted => StatusCodes.Status202Accepted,
            EnrollmentOutcome.Dropped or EnrollmentOutcome.LeftWaitlist => StatusCodes.Status200OK,
            EnrollmentOutcome.AlreadyEnrolled or EnrollmentOutcome.AlreadyWaitlisted => StatusCodes.Status409Conflict,
            EnrollmentOutcome.NotEligible => UnprocessableContent,
            EnrollmentOutcome.LearnerNotFound or EnrollmentOutcome.NotEnrolled => StatusCodes.Status404NotFound,
            EnrollmentOutcome.InvalidRequest => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}

