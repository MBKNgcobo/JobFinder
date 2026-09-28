using System.Text.Json.Serialization;

namespace JobFinder.Models.DTOs
{
    public class ApplicationAgentRequestDto
    {
        [JsonPropertyName("user_profile")]
        public UserProfileDto UserProfile { get; set; } = new();

        [JsonPropertyName("job")]
        public JobApplicationDto Job { get; set; } = new();
    }

    public class UserProfileDto
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("years_of_experience")]
        public int YearsOfExperience { get; set; }

        [JsonPropertyName("skills")]
        public List<string> Skills { get; set; } = new();

        [JsonPropertyName("projects")]
        public List<ProjectDto> Projects { get; set; } = new();
    }

    public class JobApplicationDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("company")]
        public string Company { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("required_skills")]
        public List<string> RequiredSkills { get; set; } = new();

        [JsonPropertyName("preferred_skills")]
        public List<string> PreferredSkills { get; set; } = new();
    }
    public class ProjectDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("skills")]
        public List<string> Skills { get; set; } = new();
    }
}