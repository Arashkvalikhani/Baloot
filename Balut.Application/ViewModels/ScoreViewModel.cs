using System.ComponentModel.DataAnnotations;

namespace Balut.Application.ViewModels
{
    public class ScoreViewModel
    {
        public int Id { get; set; }

        [Required]
        public int SessionId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [Range(0, 10, ErrorMessage = "نمره باید بین 0 تا 10 باشد")]
        public decimal ScoreValue { get; set; }

        public string? Comments { get; set; }

        public string? StudentName { get; set; }
        public string? SessionTopic { get; set; }
    }
}