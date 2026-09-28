namespace JobFinder.Models
{
    public class JobSkill
    {
        public int JobId { get; set; }

        public int SkillId { get; set; }

        public bool IsRequired { get; set; }

        // Relationships
        public Job Job { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}