using System.Text.Json;
using JobFinder.Models.DTOs;
using JobFinder.Services.Interfaces;

namespace JobFinder.Services
{
    public class ApplicationAgentService : IApplicationAgentService
    {
        private readonly HttpClient _httpClient;

        private static readonly JsonSerializerOptions JsonOptions =
            new(JsonSerializerDefaults.Web);

        public ApplicationAgentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApplicationAgentResponseDto?>
            GenerateApplicationAsync(
                ApplicationAgentRequestDto request)
        {
            try
            {
                var response =
                    await _httpClient.PostAsJsonAsync(
                        "api/agents/prepare-application",
                        request);

                var responseBody =
                    await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result =
                        JsonSerializer.Deserialize<ApplicationAgentResponseDto>(
                            responseBody,
                            JsonOptions);

                    if (result != null)
                    {
                        return result;
                    }

                    Console.WriteLine(
                        "[APPLICATION AGENT] API returned an empty/invalid response. " +
                        "Using deterministic fallback."
                    );
                }
                else
                {
                    Console.WriteLine(
                        $"[APPLICATION AGENT] AI service returned " +
                        $"{(int)response.StatusCode} {response.StatusCode}. " +
                        "Using deterministic fallback."
                    );

                    Console.WriteLine(responseBody);
                }
            }
            catch (HttpRequestException exception)
            {
                Console.WriteLine(
                    $"[APPLICATION AGENT] HTTP error: {exception.Message}. " +
                    "Using deterministic fallback."
                );
            }
            catch (TaskCanceledException exception)
            {
                Console.WriteLine(
                    $"[APPLICATION AGENT] Request timed out/cancelled: " +
                    $"{exception.Message}. Using deterministic fallback."
                );
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"[APPLICATION AGENT] Unexpected error: " +
                    $"{exception.Message}. Using deterministic fallback."
                );
            }

            return BuildDeterministicFallback(request);
        }

        // ============================================================
        // DETERMINISTIC FALLBACK
        // ============================================================

        private static ApplicationAgentResponseDto
            BuildDeterministicFallback(
                ApplicationAgentRequestDto request)
        {
            /*
             * We serialize the request rather than depending directly
             * on every nested DTO property here. This keeps the fallback
             * resilient to small DTO naming changes.
             */

            var requestJson =
                JsonSerializer.Serialize(
                    request,
                    JsonOptions);

            using var document =
                JsonDocument.Parse(requestJson);

            var root = document.RootElement;

            var job =
                GetObject(
                    root,
                    "job",
                    "jobApplication",
                    "job_application");

            var candidate =
                GetObject(
                    root,
                    "userProfile",
                    "user_profile",
                    "candidate",
                    "candidateProfile");

            var jobTitle =
                GetString(
                    job,
                    "title",
                    "jobTitle",
                    "job_title")
                ?? "Target Position";

            var company =
                GetString(
                    job,
                    "company",
                    "companyName",
                    "company_name")
                ?? "Hiring Company";

            var candidateSkills =
                GetStringArray(
                    candidate,
                    "skills");

            var requiredSkills =
                GetStringArray(
                    job,
                    "requiredSkills",
                    "required_skills");

            var preferredSkills =
                GetStringArray(
                    job,
                    "preferredSkills",
                    "preferred_skills");

            // --------------------------------------------------------
            // MATCHING
            // --------------------------------------------------------

            var normalizedCandidateSkills =
                candidateSkills
                    .Select(Normalize)
                    .ToHashSet();

            var matchedSkills =
                requiredSkills
                    .Concat(preferredSkills)
                    .Where(skill =>
                        normalizedCandidateSkills.Contains(
                            Normalize(skill)))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

            var missingSkills =
                requiredSkills
                    .Where(skill =>
                        !normalizedCandidateSkills.Contains(
                            Normalize(skill)))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

            // --------------------------------------------------------
            // PROJECTS
            // --------------------------------------------------------

            var projects =
                GetArray(
                    candidate,
                    "projects");

            var projectResults =
                new List<object>();

            var relevantExperience =
                new List<string>();

            foreach (var project in projects)
            {
                var projectName =
                    GetString(
                        project,
                        "name",
                        "projectName",
                        "project_name");

                if (string.IsNullOrWhiteSpace(projectName))
                {
                    continue;
                }

                var projectDescription =
                    GetString(
                        project,
                        "description");

                var projectSkills =
                    GetStringArray(
                        project,
                        "skills");

                var matchingProjectSkills =
                    projectSkills
                        .Where(skill =>
                            normalizedCandidateSkills.Contains(
                                Normalize(skill)))
                        .ToList();

                /*
                 * Prefer projects that explicitly share skills with the
                 * candidate/job. If project skills aren't available,
                 * still retain the project as portfolio evidence.
                 */

                var jobSkillSet =
                    requiredSkills
                        .Concat(preferredSkills)
                        .Select(Normalize)
                        .ToHashSet();

                var jobProjectMatches =
                    projectSkills
                        .Where(skill =>
                            jobSkillSet.Contains(
                                Normalize(skill)))
                        .ToList();

                var relevanceScore =
                    jobProjectMatches.Count * 2
                    + matchingProjectSkills.Count;

                projectResults.Add(
                    new
                    {
                        name = projectName,
                        reason =
                            jobProjectMatches.Count > 0
                                ? $"This project demonstrates practical exposure to: " +
                                  $"{string.Join(", ", jobProjectMatches)}."
                                : "This project provides portfolio evidence that can support the application."
                    });

                if (!string.IsNullOrWhiteSpace(projectDescription))
                {
                    relevantExperience.Add(
                        $"{projectName}: {projectDescription}");
                }
            }

            // Keep the fallback focused.
            projectResults =
                projectResults
                    .Take(3)
                    .ToList();

            relevantExperience =
                relevantExperience
                    .Take(5)
                    .ToList();

            // --------------------------------------------------------
            // SUMMARY / EXPERIENCE
            // --------------------------------------------------------

            var candidateSummary =
                GetString(
                    candidate,
                    "summary");

            var yearsOfExperience =
                GetNumber(
                    candidate,
                    "yearsOfExperience",
                    "years_of_experience");

            if (!string.IsNullOrWhiteSpace(candidateSummary))
            {
                relevantExperience.Insert(
                    0,
                    candidateSummary);
            }

            if (yearsOfExperience.HasValue &&
                yearsOfExperience.Value > 0)
            {
                relevantExperience.Insert(
                    0,
                    $"{yearsOfExperience.Value} year(s) of experience recorded in the candidate profile.");
            }

            // --------------------------------------------------------
            // CV CHANGES
            // --------------------------------------------------------

            var cvChanges =
                new List<string>();

            if (matchedSkills.Any())
            {
                cvChanges.Add(
                    $"Highlight the verified skills relevant to this role: " +
                    $"{string.Join(", ", matchedSkills)}.");
            }

            if (projectResults.Any())
            {
                cvChanges.Add(
                    "Place the most relevant portfolio projects prominently " +
                    "and describe the technologies actually used.");
            }

            if (missingSkills.Any())
            {
                cvChanges.Add(
                    $"Do not claim proficiency in missing skills unless the candidate " +
                    $"can provide supporting evidence: {string.Join(", ", missingSkills)}.");
            }

            if (!cvChanges.Any())
            {
                cvChanges.Add(
                    "Keep the CV strictly evidence-based and only include information " +
                    "that can be verified from the candidate profile.");
            }

            // --------------------------------------------------------
            // COVER LETTER
            // --------------------------------------------------------

            var skillSentence =
                matchedSkills.Any()
                    ? $"My relevant technical skills include {string.Join(", ", matchedSkills)}."
                    : "My current profile contains limited directly matched skills for this position.";

            var projectSentence =
                projectResults.Any()
                    ? "My portfolio projects provide practical evidence of applying these technologies."
                    : "I would welcome the opportunity to demonstrate my technical ability through my portfolio.";

            var coverLetter =
                $"""
                Dear Hiring Team,

                I am writing to express my interest in the {jobTitle} position at {company}.

                {skillSentence} {projectSentence}

                I have focused my development work on building practical software systems and applying technical concepts through portfolio projects. I am particularly interested in opportunities where I can continue developing my skills while contributing to meaningful software solutions.

                I have intentionally kept this application evidence-based and have not included skills or experience that cannot be substantiated by my candidate profile.

                Thank you for considering my application. I would welcome the opportunity to discuss my background and suitability for the position.

                Sincerely,
                Mcebo Ngcobo
                """;

            // --------------------------------------------------------
            // BUILD DTO THROUGH JSON
            // --------------------------------------------------------

            var fallbackPayload =
                 new
                 {
                     job_title = jobTitle,
                     company,
                     matched_skills = matchedSkills,
                     missing_skills = missingSkills,
                     recommended_projects = projectResults,
                     relevant_experience = relevantExperience,
                     cover_letter = coverLetter,
                     cv_changes = cvChanges
                 };

            var fallbackJson =
                JsonSerializer.Serialize(
                    fallbackPayload,
                    JsonOptions);

            var result =
                     JsonSerializer.Deserialize<ApplicationAgentResponseDto>(
                         fallbackJson,
                         JsonOptions);

            return result
                ?? throw new InvalidOperationException(
                    "Failed to create deterministic application fallback.");
        }

        // ============================================================
        // JSON HELPERS
        // ============================================================

        private static JsonElement GetObject(
            JsonElement parent,
            params string[] names)
        {
            if (parent.ValueKind != JsonValueKind.Object)
            {
                return default;
            }

            foreach (var property in parent.EnumerateObject())
            {
                if (names.Any(name =>
                        string.Equals(
                            Normalize(name),
                            Normalize(property.Name),
                            StringComparison.OrdinalIgnoreCase)))
                {
                    if (property.Value.ValueKind ==
                        JsonValueKind.Object)
                    {
                        return property.Value;
                    }
                }
            }

            return default;
        }

        private static JsonElement[] GetArray(
            JsonElement parent,
            params string[] names)
        {
            if (parent.ValueKind != JsonValueKind.Object)
            {
                return Array.Empty<JsonElement>();
            }

            foreach (var property in parent.EnumerateObject())
            {
                if (names.Any(name =>
                        string.Equals(
                            Normalize(name),
                            Normalize(property.Name),
                            StringComparison.OrdinalIgnoreCase)))
                {
                    if (property.Value.ValueKind ==
                        JsonValueKind.Array)
                    {
                        return property.Value
                            .EnumerateArray()
                            .ToArray();
                    }
                }
            }

            return Array.Empty<JsonElement>();
        }

        private static List<string> GetStringArray(
            JsonElement parent,
            params string[] names)
        {
            var result = new List<string>();

            foreach (var element in GetArray(parent, names))
            {
                if (element.ValueKind == JsonValueKind.String)
                {
                    var value = element.GetString();

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        result.Add(value);
                    }

                    continue;
                }

                if (element.ValueKind == JsonValueKind.Object)
                {
                    var value =
                        GetString(
                            element,
                            "name",
                            "skill",
                            "skillName",
                            "skill_name");

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        result.Add(value);
                    }
                }
            }

            return result
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string? GetString(
            JsonElement parent,
            params string[] names)
        {
            if (parent.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var property in parent.EnumerateObject())
            {
                if (names.Any(name =>
                        string.Equals(
                            Normalize(name),
                            Normalize(property.Name),
                            StringComparison.OrdinalIgnoreCase)))
                {
                    if (property.Value.ValueKind ==
                        JsonValueKind.String)
                    {
                        return property.Value.GetString();
                    }
                }
            }

            return null;
        }

        private static double? GetNumber(
            JsonElement parent,
            params string[] names)
        {
            if (parent.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var property in parent.EnumerateObject())
            {
                if (names.Any(name =>
                        string.Equals(
                            Normalize(name),
                            Normalize(property.Name),
                            StringComparison.OrdinalIgnoreCase)))
                {
                    if (property.Value.ValueKind ==
                        JsonValueKind.Number &&
                        property.Value.TryGetDouble(
                            out var number))
                    {
                        return number;
                    }
                }
            }

            return null;
        }

        private static string Normalize(string value)
        {
            return value
                .Replace("_", "")
                .Replace("-", "")
                .Replace(" ", "")
                .ToLowerInvariant();
        }
    }
}