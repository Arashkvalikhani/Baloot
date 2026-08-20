namespace Balut.Application.ViewModels
{
    public class ScoreEditViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal? ScoreValue { get; set; }
        public string? Comments { get; set; }
    }

    public class SaveScoresRequest
    {
        public int SessionId { get; set; }
        public List<ScoreEditViewModel> Items { get; set; } = new();
    }
}