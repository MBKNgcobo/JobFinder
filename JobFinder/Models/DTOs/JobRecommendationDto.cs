using System.Text.Json.Serialization;
using JobFinder.Models.DTOs;
using JobFinder.Models.ViewModels;
using JobFinder.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JobFinder.Models.DTOs
{
    public class JobRecommendationDto
    {
        [JsonPropertyName("job_id")]
        public int JobId { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("company")]
        public string Company { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("employment_type")]
        public string? EmploymentType { get; set; }

        [JsonPropertyName("salary_min")]
        public decimal? SalaryMin { get; set; }

        [JsonPropertyName("salary_max")]
        public decimal? SalaryMax { get; set; }

        [JsonPropertyName("posted_date")]
        public DateTime? PostedDate { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("source_url")]
        public string? SourceUrl { get; set; }

        [JsonPropertyName("match_score")]
        public double MatchScore { get; set; }

        [JsonPropertyName("matched_required_skills")]
        public List<string> MatchedRequiredSkills { get; set; } = new();

        [JsonPropertyName("missing_required_skills")]
        public List<string> MissingRequiredSkills { get; set; } = new();

        [JsonPropertyName("matched_preferred_skills")]
        public List<string> MatchedPreferredSkills { get; set; } = new();

        [JsonPropertyName("missing_preferred_skills")]
        public List<string> MissingPreferredSkills { get; set; } = new();
    }
}