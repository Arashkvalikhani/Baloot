namespace Balut.Application.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalParents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalClasses { get; set; }
        public int TotalSessions { get; set; }
        public int ActiveCourses { get; set; }
        public int EndingSoonCourses { get; set; }
        public int TotalAbsences { get; set; }
        public decimal AverageScore { get; set; }
        public int PendingSubmissions { get; set; }
        public int UnreadMessages { get; set; }
    }
}