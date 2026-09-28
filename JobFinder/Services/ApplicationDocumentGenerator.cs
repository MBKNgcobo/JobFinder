using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using JobFinder.Models;
using JobFinder.Models.DTOs;

namespace JobFinder.Services
{
    public class ApplicationDocumentGenerator
    {
        public byte[] GenerateCv(
            User user,
            Application application,
            ApplicationAgentResponseDto response)
        {
            using var stream = new MemoryStream();

            using (
                var document = WordprocessingDocument.Create(
                    stream,
                    WordprocessingDocumentType.Document))
            {
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();

                var body = new Body();

                // Name
                body.Append(CreateParagraph(
                    $"{user.FirstName} {user.LastName}",
                    bold: true,
                    fontSize: 28));

                // Contact
                var contact = new List<string>();

                if (!string.IsNullOrWhiteSpace(user.Email))
                    contact.Add(user.Email);

                if (!string.IsNullOrWhiteSpace(user.Phone))
                    contact.Add(user.Phone);

                if (!string.IsNullOrWhiteSpace(user.Location))
                    contact.Add(user.Location);

                if (contact.Count > 0)
                {
                    body.Append(
                        CreateParagraph(
                            string.Join(" | ", contact),
                            fontSize: 10));
                }

                // Links
                var links = new List<string>();

                if (!string.IsNullOrWhiteSpace(user.LinkedInUrl))
                    links.Add(user.LinkedInUrl);

                if (!string.IsNullOrWhiteSpace(user.GitHubUrl))
                    links.Add(user.GitHubUrl);

                if (!string.IsNullOrWhiteSpace(user.PortfolioUrl))
                    links.Add(user.PortfolioUrl);

                if (links.Count > 0)
                {
                    body.Append(
                        CreateParagraph(
                            string.Join(" | ", links),
                            fontSize: 9));
                }

                // Target role
                body.Append(
                    CreateHeading(
                        application.Job.Title));

                // Summary
                if (!string.IsNullOrWhiteSpace(user.Summary))
                {
                    body.Append(
                        CreateHeading("Professional Summary"));

                    body.Append(
                        CreateParagraph(user.Summary));
                }

                // Skills
                body.Append(
                    CreateHeading("Technical Skills"));

                var matchedSkills =
                    response.MatchedSkills
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x.Trim())
                        .ToList();

                var allSkills =
                    user.UserSkills
                        .Select(x => x.Skill?.Name)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(x => x!.Trim())
                        .ToList();

                var orderedSkills = matchedSkills
                    .Concat(
                        allSkills.Where(
                            x => !matchedSkills.Contains(
                                x,
                                StringComparer.OrdinalIgnoreCase)))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var skill in orderedSkills)
                {
                    body.Append(
                        CreateBullet(skill));
                }

                // Projects
                var recommendedNames =
                    response.RecommendedProjects
                        .Select(x => x.Name)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var selectedProjects =
                    user.Projects
                        .Where(
                            x => recommendedNames.Contains(x.Name))
                        .ToList();

                if (selectedProjects.Count > 0)
                {
                    body.Append(
                        CreateHeading("Technical Projects"));

                    foreach (var project in selectedProjects)
                    {
                        body.Append(
                            CreateParagraph(
                                project.Name,
                                bold: true,
                                fontSize: 12));

                        if (!string.IsNullOrWhiteSpace(
                            project.Description))
                        {
                            body.Append(
                                CreateParagraph(
                                    project.Description));
                        }

                        var projectSkills =
                            project.ProjectSkills
                                .Select(x => x.Skill?.Name)
                                .Where(x =>
                                    !string.IsNullOrWhiteSpace(x))
                                .Select(x => x!)
                                .ToList();

                        foreach (var skill in projectSkills)
                        {
                            body.Append(
                                CreateBullet(
                                    $"Demonstrated {skill}"));
                        }

                        if (!string.IsNullOrWhiteSpace(
                            project.GitHubUrl))
                        {
                            body.Append(
                                CreateParagraph(
                                    $"GitHub: {project.GitHubUrl}",
                                    fontSize: 9));
                        }

                        if (!string.IsNullOrWhiteSpace(
                            project.LiveUrl))
                        {
                            body.Append(
                                CreateParagraph(
                                    $"Live: {project.LiveUrl}",
                                    fontSize: 9));
                        }
                    }
                }

                // Experience
                if (user.YearsOfExperience > 0)
                {
                    body.Append(
                        CreateHeading("Experience"));

                    body.Append(
                        CreateParagraph(
                            $"{user.YearsOfExperience} years of experience"));
                }

                // Tailoring changes are not printed into the CV.
                // They control ordering/content rather than appearing
                // as AI instructions inside the applicant's document.

                body.Append(
                    CreateParagraph(
                        $"Prepared specifically for: " +
                        $"{application.Job.Title} at " +
                        $"{application.Job.Company?.Name}",
                        italic: true,
                        fontSize: 8));

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            return stream.ToArray();
        }

        public byte[] GenerateCoverLetter(
            User user,
            Application application,
            ApplicationAgentResponseDto response)
        {
            using var stream = new MemoryStream();

            using (
                var document = WordprocessingDocument.Create(
                    stream,
                    WordprocessingDocumentType.Document))
            {
                var mainPart = document.AddMainDocumentPart();
                mainPart.Document = new Document();

                var body = new Body();

                body.Append(
                    CreateParagraph(
                        $"{user.FirstName} {user.LastName}",
                        bold: true,
                        fontSize: 16));

                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    body.Append(
                        CreateParagraph(user.Email));
                }

                if (!string.IsNullOrWhiteSpace(user.Phone))
                {
                    body.Append(
                        CreateParagraph(user.Phone));
                }

                body.Append(
                    CreateParagraph(
                        DateTime.Now.ToString("dd MMMM yyyy")));

                body.Append(
                    CreateParagraph(
                        $"Application: {application.Job.Title}",
                        bold: true));

                body.Append(
                    CreateParagraph(
                        application.Job.Company?.Name
                        ?? response.Company));

                body.Append(
                    CreateParagraph(""));

                var paragraphs =
                    response.CoverLetter
                        .Split(
                            new[] { "\r\n\r\n", "\n\n" },
                            StringSplitOptions.RemoveEmptyEntries);

                foreach (var paragraph in paragraphs)
                {
                    body.Append(
                        CreateParagraph(
                            paragraph.Trim()));
                }

                body.Append(CreateParagraph(""));
                body.Append(
                    CreateParagraph(
                        $"Sincerely,"));

                body.Append(
                    CreateParagraph(
                        $"{user.FirstName} {user.LastName}",
                        bold: true));

                mainPart.Document.Append(body);
                mainPart.Document.Save();
            }

            return stream.ToArray();
        }

        private static Paragraph CreateHeading(string text)
        {
            return CreateParagraph(
                text,
                bold: true,
                fontSize: 16);
        }

        private static Paragraph CreateBullet(string text)
        {
            var paragraph = new Paragraph();

            var properties = new ParagraphProperties(
                new NumberingProperties(
                    new NumberingLevelReference
                    {
                        Val = 0
                    },
                    new NumberingId
                    {
                        Val = 1
                    }));

            paragraph.Append(properties);

            paragraph.Append(
                new Run(
                    new Text(text)));

            return paragraph;
        }

        private static Paragraph CreateParagraph(
            string text,
            bool bold = false,
            bool italic = false,
            int fontSize = 11)
        {
            var runProperties = new RunProperties(
                new FontSize
                {
                    Val = (fontSize * 2).ToString()
                });

            if (bold)
                runProperties.Append(new Bold());

            if (italic)
                runProperties.Append(new Italic());

            var run = new Run(
                runProperties,
                new Text(text)
                {
                    Space =
                        SpaceProcessingModeValues.Preserve
                });

            return new Paragraph(run);
        }
    }
}