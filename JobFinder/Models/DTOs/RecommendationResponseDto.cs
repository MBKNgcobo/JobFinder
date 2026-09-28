namespace JobFinder.Models.DTOs
{
    public class RecommendationResponseDto
    {
        public int UserId { get; set; }

        public int Count { get; set; }

        public List<JobRecommendationDto> Recommendations { get; set; }
            = new();
    }
}