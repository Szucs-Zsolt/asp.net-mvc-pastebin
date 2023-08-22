using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

            // Ha első olvasáskor törlendő, még megjelenítés előtt töröljük
            if (result.DiscardFirstRead)
            {
                _db.Messages.Remove(result);
                _db.SaveChanges();
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
            CreateMessageVM createMessageVM = new CreateMessageVM();
            createMessageVM.Message = new Message();
            createMessageVM.DiscardTimeList = new List<SelectListItem>();
            foreach (var (key, value) in new DiscardTime().Names)
            {
                createMessageVM.DiscardTimeList.Add(new SelectListItem
                {
                    Value = key.ToString(),
                    Text = value
                });
            }

            return View(createMessageVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = Role.User)]
        public IActionResult CreateMessage(CreateMessageVM createMessageVM)
        {
            if (!ModelState.IsValid)
            {
                return View(createMessageVM);
            }
            
            // Általunk beállított adatok:
            // Biztos, hogy az adatbázisban ne legyen ilyen MessageId-jű üzenet.
            do
            {
                createMessageVM.Message.MessageId = Guid.NewGuid().ToString();
            } while (_db.Messages.FirstOrDefault(x => x.MessageId == createMessageVM.Message.MessageId) != null);
            
            // Létrehozó felhasználó
            createMessageVM.Message.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Mennyi idő után kell törölni
            switch (createMessageVM.DiscardTime)
            {
                case (int)DiscardTime.Time.AfterRead:
                    createMessageVM.Message.DiscardFirstRead = true;
                    // Egy hónap után akkor is töröljük, ha nem olvasta el senki
                    createMessageVM.Message.DiscardDate = DateTime.Now.AddMonths(1);
                    break;
                case (int)DiscardTime.Time.Hour1:
                    createMessageVM.Message.DiscardDate = DateTime.Now.AddHours(1);
                    break;
                case (int)DiscardTime.Time.Hour4:
                    createMessageVM.Message.DiscardDate = DateTime.Now.AddHours(4);
                    break;
                case (int)DiscardTime.Time.Hour8:
                    createMessageVM.Message.DiscardDate = DateTime.Now.AddHours(8);
                    break;
                case (int)DiscardTime.Time.Day1:
                    createMessageVM.Message.DiscardDate = DateTime.Now.AddDays(1);
                    break;
                case (int)DiscardTime.Time.Week1:
                    createMessageVM.Message.DiscardDate = DateTime.Now.AddDays(7);
                    break;
                case (int)DiscardTime.Time.Month1:
                    createMessageVM.Message.DiscardDate = DateTime.Now.AddMonths(1);
                    break;
            }

            _db.Messages.Add(createMessageVM.Message);
            _db.SaveChanges();
            return RedirectToAction("ShowMessageId", new { messageId = createMessageVM.Message.MessageId });
        }


        [Authorize(Roles = Role.User)]
        public IActionResult ShowMessageId(string messageId)
        {
            ViewBag.MessageId = messageId;
            return View();
        }
    }
}