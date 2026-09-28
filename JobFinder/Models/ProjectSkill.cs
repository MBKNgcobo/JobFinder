namespace JobFinder.Models
{
    public class ProjectSkill
    {
        public int ProjectId { get; set; }

        public int SkillId { get; set; }

        // Relationships
        public Project Project { get; set; } = null!;

        public Skill Skill { get; set; } = null!;
    }
}