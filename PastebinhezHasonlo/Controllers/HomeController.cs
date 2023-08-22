using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PastebinhezHasonlo.Data;
using PastebinhezHasonlo.Models;
using System.Diagnostics;
using System.Security.Claims;

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

        // Akkor se fut le, ha a request localhost:12345/Controller/Action-nek szól
        [Authorize(Roles = Role.User)]
        public IActionResult CreateMessage()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role.User)]
        public IActionResult WriteMessage(Message message)
        {
            if (!ModelState.IsValid)
            {
                return View(message);
            }

            message.MessageId = Guid.NewGuid().ToString();
            message.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _db.Messages.Add(message);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}