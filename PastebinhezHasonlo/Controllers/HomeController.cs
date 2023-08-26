using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PastebinhezHasonlo.Data;
using PastebinhezHasonlo.Models;
using PastebinhezHasonlo.Models.ViewModels;
using System.Diagnostics;
using System.Security.Claims;
using System.Xml.Linq;

namespace PastebinhezHasonlo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        // Dependency injection: EntityFramework
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
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

            // Hiba 1: Nincs azonosító, nem tudjuk melyik üzenetet kérdezzük le
            if (string.IsNullOrEmpty(messageId))
            {
                result.Msg = "Hiányzik az üzenetazonosító.";
                return View(result);
            }

            result = _db.Messages.FirstOrDefault(x => x.MessageId == messageId);
            // Hiba 2: Az adatbázisban nincs ilyen azonosítójú üzenet
            if (result == null)
            {
                result = new Message() { Msg = "Nincs ilyen azonosítójú üzenet."};
            } else if (result.DiscardDate < DateTime.Now)
            {
                // Hiba 3: Volt ilyen üzenet, de az érvényessége már lejárt
                // (Az óránkénti adatbázistisztítás még nem törölte, ezért itt fogjuk).
                _db.Messages.Remove(result);
                _db.SaveChanges();
                result = new Message() { Msg = "Nincs ilyen azonosítójú üzenet." };
            }

            // Speciális eset: Első olvasáskor törlendő
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
        [Authorize]
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
        [Authorize]
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

            // Message lejárati idejének beállítása (külön metódusba kiemelve)
            createMessageVM.Message = 
                SetMessageDiscardDate(createMessageVM.Message, createMessageVM.DiscardTime);

            _db.Messages.Add(createMessageVM.Message);
            _db.SaveChanges();
            return RedirectToAction("ShowMessageId", new { messageId = createMessageVM.Message.MessageId });
        }

        // Az üzenetben beállítjuk az időt, amikor majd törölni kell (discardTime alapján)
        private static Message SetMessageDiscardDate(Message message, int discardTime)
        {
            // Mennyi idő után kell törölni
            switch (discardTime)
            {
                case (int)DiscardTime.Time.AfterRead:
                    message.DiscardFirstRead = true;
                    // Egy hónap után akkor is töröljük, ha nem olvasta el senki
                    message.DiscardDate = DateTime.Now.AddMonths(1);
                    break;
                case (int)DiscardTime.Time.Hour1:
                    message.DiscardDate = DateTime.Now.AddHours(1);
                    break;
                case (int)DiscardTime.Time.Hour4:
                    message.DiscardDate = DateTime.Now.AddHours(4);
                    break;
                case (int)DiscardTime.Time.Hour8:
                    message.DiscardDate = DateTime.Now.AddHours(8);
                    break;
                case (int)DiscardTime.Time.Day1:
                    message.DiscardDate = DateTime.Now.AddDays(1);
                    break;
                case (int)DiscardTime.Time.Week1:
                    message.DiscardDate = DateTime.Now.AddDays(7);
                    break;
                case (int)DiscardTime.Time.Month1:
                    message.DiscardDate = DateTime.Now.AddMonths(1);
                    break;
            }

            return message;
        }

        // Ha sikeresen létrehoztunk egy üzenetet, kiírja az azonosítóját,
        // amivel majd le lehet kérdezni
        [Authorize]
        public IActionResult ShowMessageId(string messageId)
        {
            ViewBag.MessageId = messageId;
            return View();
        }

        // Bejelentkezett felhasználó megnézheti az általa létrehozott üzeneteket
        [Authorize]
        public IActionResult ShowMyMessages() {
            // Saját üzenetei növekvő lejáratiidő sorrendben
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
            string currentUserName = User.FindFirstValue(ClaimTypes.Name);

            ShowMessagesVM showMessageVM = new ShowMessagesVM()
            {
                MessageList = _db.Messages
                    .Where(x => x.UserId == currentUserId)
                    .OrderBy(x => x.DiscardDate)
                    .ToList(),
                UserName = currentUserName,
                IsAdminSupervision = false // saját üzeneteit nézi
            };

            // Ha van benne olyan üzenet, amit már törölni kellett volna, de a háttérben
            // futó időnkénti törlés még nem jutott el hozzá, akkor itt töröljük.
            DateTime currentDateTime = DateTime.Now;
            var discardMessages = showMessageVM.MessageList.Where(x => x.DiscardDate < currentDateTime);
            if (discardMessages.Any())
            {
                _db.Messages.RemoveRange(discardMessages);
                _db.SaveChanges();
            }
            
            return View("ShowUserMessages", showMessageVM );
        }

        [Authorize]
        public IActionResult ModifyMessage(string messageId)
        {
            // Hiba 1: Nincs üzenetazonosító
            if (string.IsNullOrEmpty(messageId))
            {
                ViewBag.ErrorMessage = "Hiányzik az üzenetazonosító.";
                return View("ShowErrorMessage");
            }

            // Hiba 2: Az adatbázisban nincs ilyen azonosítójú üzenet
            Message? message = _db.Messages.FirstOrDefault(x => x.MessageId == messageId);
            if (message == null)
            {
                ViewBag.ErrorMessage = "Az adatbázisban nincs ilyen azonosítójú üzenet.";
                return View("ShowErrorMessage");
            }

            // Hiba 3: Nem a sajátját akarja törölni és nem is admin csinálja
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
            if ((message.UserId != currentUserId) && (!User.IsInRole(Role.Admin)))
            {
                ViewBag.ErrorMessage = "Nincs jogosultsága más üzenetét szerkeszteni.";
                return View("ShowErrorMessage");
            }

            CreateMessageVM modifyMessageVM = new CreateMessageVM();
            modifyMessageVM.Message = message;
            modifyMessageVM.DiscardTimeList = new List<SelectListItem>();
            foreach (var (key, value) in new DiscardTime().Names)
            {
                modifyMessageVM.DiscardTimeList.Add(new SelectListItem
                {
                    Value = key.ToString(),
                    Text = value
                });
            }

            return View(modifyMessageVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult ModifyMessage(CreateMessageVM modifyMessageVM)
        {
            if (!ModelState.IsValid)
            {
                return View(modifyMessageVM);
            }
            // Nem csak az üzenet tartalma és lejárati ideje is változhat.    
            modifyMessageVM.Message = 
                SetMessageDiscardDate(modifyMessageVM.Message, modifyMessageVM.DiscardTime);

            _db.Messages.Update(modifyMessageVM.Message);
            _db.SaveChanges();
            return RedirectToAction("ShowMessageId", new { messageId = modifyMessageVM.Message.MessageId });
        }


        [Authorize]
        public IActionResult DeleteMessage(string messageId)
        {
            // Hiba 1: Nincs üzenetazonosító
            if (string.IsNullOrEmpty(messageId))
            {
                ViewBag.ErrorMessage = "Hiányzik az üzenetazonosító.";
                return View("ShowErrorMessage");
            }

            // Hiba 2: Az adatbázisban nincs ilyen azonosítójú üzenet
            Message? message = _db.Messages.FirstOrDefault(x => x.MessageId == messageId);
            if (message == null)
            {
                ViewBag.ErrorMessage = "Az adatbázisban nincs ilyen azonosítójú üzenet.";
                return View("ShowErrorMessage");
            }

            // Hiba 3: Nem a sajátját akarja törölni és nem is admin csinálja
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
            if ((message.UserId != currentUserId) && (!User.IsInRole(Role.Admin)))
            {
                ViewBag.ErrorMessage = "Nincs jogosultsága más üzenetét törölni.";
                return View("ShowErrorMessage");
            }
            return View(message);
        }

        [HttpPost, ActionName("DeleteMessage")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult DeleteMessagePOST(string messageId)
        {
            Message? message = _db.Messages.FirstOrDefault(x => x.MessageId == messageId);                               
            if (message != null)
            {
                // A saját üzenetét törli, vagy az admin az, aki törölni akar
                string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
                if ( (message.UserId == currentUserId) || (User.IsInRole(Role.Admin)) )
                {
                    _db.Messages.Remove(message);
                    _db.SaveChanges();
                }
            }
            return RedirectToAction("ShowMyMessages");
        }

        public async Task<IActionResult> ShowAllUsers()
        {
            List<UserRoleVM> userRoleVMList = new List<UserRoleVM>();

            var users = await _userManager.Users.ToListAsync();
            foreach (var user in users)
            {
                UserRoleVM userRoleVM = new UserRoleVM();
                // Név, email
                userRoleVM.Name = user.UserName;
                userRoleVM.Email = user.Email;
                
                // Milyen role (csak az első számít, mivel mindenkinek csak 1 lehet)
                var roles = await _userManager.GetRolesAsync(user);
                userRoleVM.Role = roles.First();

                // Hány aktív üzenete van az adatbázisban
                userRoleVM.NumberOfActiveMessages = _db.Messages.Count(x => x.UserId == user.Id);

                userRoleVMList.Add(userRoleVM);
            }

            userRoleVMList = userRoleVMList
                .OrderBy(x=> x.Role)
                .ThenBy(x=> x.Name)
                .ToList();

            return View(userRoleVMList);
        }

        // Admin megnézheti bárkinek az üzeneteit
        [Authorize(Roles=Role.Admin)]
        public async Task<IActionResult> ShowUserMessages(string? email)
        {
            if (email == null) {
                ViewBag.ErrorMessage = "Hiányzik a felhasználó email-címe.";
                return View("ShowErrorMessage");
            }

            // Mi annak a usernek az azonosítója, akinek az üzeneteit nézzük
            var originalUser = await _userManager.FindByEmailAsync(email);
            // Mi az aktuális useré?
            string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString();
            // Ha a kettő nem egyezik, akkor az admin nézi más leveleit.
            var isAdminSupervision = originalUser.Id != currentUserId;

            ShowMessagesVM showMessagesVM = new ShowMessagesVM()
            {
                MessageList = _db.Messages
                    .Where(x => x.UserId == originalUser.Id.ToString())
                    .OrderBy(x => x.DiscardDate)
                    .ToList(),
                UserName = originalUser.UserName,        // Kinek a leveleit nézzük
                IsAdminSupervision = isAdminSupervision
            };
            return View(showMessagesVM);
        }

        public async Task<IActionResult> ChangeAdminRoleStatus(string userEmail)
        {
            // Hiba: Nincs email
            if (string.IsNullOrEmpty(userEmail)) {
                ViewBag.ErrorMessage = "Nincs ilyen felhasználó";
                return View("ShowErrorMessage");
            }
            if (userEmail == "admin@admin")
            {
                ViewBag.ErrorMessage = "admin@admin nevű adminisztrátortól nem lehet megvonni az admin jogot.";
                return View("ShowErrorMessage");
            }

            // Hiba: Nincs user ilyen email-lel
            var user = await _db.Users.FirstOrDefaultAsync(x=> x.Email == userEmail);
            if (user == null) {
                ViewBag.ErrorMessage = "Nincs ilyen felhasználó";
                return View("ShowErrorMessage");
            }

            // Milyen role-jai vannak jelenleg
            IList<string> userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Contains(Role.Admin)) // Admin -> User
            {
                // Egy felhasználónak csak egy role-ja lehet
                await _userManager.RemoveFromRoleAsync(user, Role.Admin);
                await _userManager.AddToRoleAsync(user, Role.User);
            }
            else // User -> Admin
            {
                await _userManager.RemoveFromRoleAsync(user, Role.User);
                await _userManager.AddToRoleAsync(user, Role.Admin);
            }

            return RedirectToAction("ShowAllUsers");
        }

    } // class HomeController
} // namespace