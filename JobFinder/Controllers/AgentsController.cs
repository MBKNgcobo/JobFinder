using Microsoft.AspNetCore.Mvc;

namespace JobFinder.Controllers
{
    public class AgentsController : Controller
    {
        // GET: Agents
        public IActionResult Index()
        {
            return View();
        }

        // POST: Agents/AnalyzeJob
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AnalyzeJob(int jobId)
        {
            // Later:
            // Call Python Agent API

            return RedirectToAction("Details", "Jobs",
                new { id = jobId });
        }

        // POST: Agents/MatchJob
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MatchJob(int jobId, int userId)
        {
            // Later:
            // Call Python Matching Agent

            return RedirectToAction("Details", "Jobs",
                new { id = jobId });
        }

        // POST: Agents/GenerateApplication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerateApplication(int jobId, int userId)
        {
            // Later:
            // Call Python Application Agent

            return RedirectToAction("Create", "Applications",
                new { jobId });
        }
    }
}