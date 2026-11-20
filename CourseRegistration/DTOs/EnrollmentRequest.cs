using System.ComponentModel.DataAnnotations;

namespace CourseRegistration.DTOs
{
    public class EnrollmentRequest
    {
        [Required]
        public string? LearnerId { get; set; }
    }
}
