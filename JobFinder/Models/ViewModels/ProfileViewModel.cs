namespace JobFinder.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public int YearsOfExperience { get; set; }

        public List<string> Skills { get; set; } = new();

        public List<ProfileProjectViewModel> Projects { get; set; } = new();
    }

    public class ProfileProjectViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? GitHubUrl { get; set; }

        public string? LiveUrl { get; set; }

        public List<string> Skills { get; set; } = new();
    }
}