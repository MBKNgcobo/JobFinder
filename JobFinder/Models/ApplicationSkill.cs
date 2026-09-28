namespace JobFinder.Models
{
    public class ApplicationSkill
    {
        public int ApplicationId { get; set; }

        public int SkillId { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(50)]
        public string? MatchStatus { get; set; }

        // Relationships
        public Application Application { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}