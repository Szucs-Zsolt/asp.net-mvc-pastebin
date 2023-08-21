using Microsoft.AspNetCore.Mvc;
using PastebinhezHasonlo.Data;
using PastebinhezHasonlo.Models;
using System.Diagnostics;

namespace PastebinhezHasonlo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // Dependency injection: EntityFramework
        private readonly ApplicationDbContext _db;


        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        // GET
        public IActionResult Index()
        {
            return View(new Message());    
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string messageId)
        {
            var result = new Message();

            if (string.IsNullOrEmpty(messageId))
            {
                result.Msg = "Hiányzik az üzenetazonosító.";
                return View(result);
            }

            result = _db.Messages.FirstOrDefault(x => x.MessageId == messageId);
            if (result == null)
            {
                result = new Message() { Msg = "Nincs ilyen azonosítójú üzenet."};
            }
            return View(result);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}