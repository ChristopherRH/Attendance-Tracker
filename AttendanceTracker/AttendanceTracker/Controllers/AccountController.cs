using AttendanceTracker.Models;
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

        [HttpPost]
        public JsonResult UpdateBosses(string id, string goroth, string di, string harj, string sisters, string host, string mistress, string maiden, string fa, string kj)
        {
            var list = GetUserBosses();
            var user = list.FirstOrDefault(x => x.Id.Equals(id));
            if(user == null)
            {
                return Json("ERROR: An error occurred");
            }
            var update = new BossesNeeded
            {
                User =  user.User,
                Goroth = Boolean.Parse(goroth),
                Di = Boolean.Parse(di),
                Harjatan = Boolean.Parse(harj),
                Sisters = Boolean.Parse(sisters),
                Host = Boolean.Parse(host),
                Mistress = Boolean.Parse(mistress),
                Maiden = Boolean.Parse(maiden),
                FallenAvatar = Boolean.Parse(fa),
                Kiljaeden = Boolean.Parse(kj)
            };

            _client.UpdateAsync($"bosses/{id}", update).Wait();

            return Json("");
        }
    }
}