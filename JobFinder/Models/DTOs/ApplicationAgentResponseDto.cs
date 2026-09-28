using System.Text.Json.Serialization;

namespace JobFinder.Models.DTOs
{
    public class ApplicationAgentResponseDto
    {
        [JsonPropertyName("job_title")]
        public string JobTitle { get; set; } = string.Empty;

        [JsonPropertyName("company")]
        public string Company { get; set; } = string.Empty;

        [JsonPropertyName("matched_skills")]
        public List<string> MatchedSkills { get; set; } = new();

        [JsonPropertyName("missing_skills")]
        public List<string> MissingSkills { get; set; } = new();

        [JsonPropertyName("recommended_projects")]
        public List<RecommendedProjectDto> RecommendedProjects { get; set; } = new();

        [JsonPropertyName("relevant_experience")]
        public List<string> RelevantExperience { get; set; } = new();

        [JsonPropertyName("cover_letter")]
        public string CoverLetter { get; set; } = string.Empty;

        [JsonPropertyName("cv_changes")]
        public List<string> CvChanges { get; set; } = new();
    }

    public class RecommendedProjectDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }
}