namespace JobFinder.Models
{
    public class UserSkill
    {
        public int UserId { get; set; }

        public int SkillId { get; set; }

        public string? ProficiencyLevel { get; set; }

        public double? YearsOfExperience { get; set; }

        // Relationships
        public User User { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}