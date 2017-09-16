using FireSharp;
using FireSharp.Config;
using System;
using System.Linq;
using System.Web.Mvc;

namespace AttendanceTracker.Controllers
{
    public class AccountController : BaseController
    {

        #region Constructor
        public AccountController()
        {
            var config = new FirebaseConfig()
            {
                BasePath = "https://attendance-7f6fe.firebaseio.com/",
                AuthSecret = "ZZNsXOiCbqIYvy6HYQOQBUrrnzumJsv163EGqaA0"
            };
            _client = new FirebaseClient(config);
        }
        #endregion

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Bosses()
        {
            var list = GetUserBosses();
            return View(list);
        }

        [HttpPost]
        public JsonResult CreateAccount(string name, string password, string passwordVerify)
        {
            // not valid
            var msg = "ERROR: missing data: Username or matching passwords.";
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordVerify))
            {
                return Json(msg);
            }
            if (password != passwordVerify)
            {
                return Json(msg);
            }

            // valid, hash this stuff and send it to the db
            var passwordHash = CalculateMD5Hash(password);

            // create the account
            var list = GetCurrentUsers();
            var account = list.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (account != null)
            {
                return Json("ERROR: already in list");
            }

            _client.PushAsync("users", new
            {
                name = name,
                password = passwordHash
            }).Wait();

            _client.PushAsync("bosses", new
            {
                User = name,
                Goroth = false,
                Di = false,
                Harjatan = false,
                Sisters = false,
                Mistress = false,
                Host = false,
                Maiden = false,
                FallenAvatar = false,
                Kiljaeden = true
            }).Wait();

            return Json("");
        }
        [HttpPost]
        public JsonResult Login(string name, string password)
        {
            // not valid
            var msg = "ERROR: missing data: Username or matching passwords.";
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                return Json(msg);
            }

            // valid, hash this stuff and send it to the db
            var passwordHash = CalculateMD5Hash(password);

            // create the account
            var list = GetCurrentUsers();
            var account = list.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (account == null)
            {
                return Json("ERROR: account not found");
            }

            if (!account.Password.Equals(passwordHash))
            {
                return Json("ERROR: invalid password");
            }

            // set the username for this session
            Session["UserName"] = name;
            return Json("");
        }
    }
}