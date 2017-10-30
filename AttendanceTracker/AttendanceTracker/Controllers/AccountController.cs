using AttendanceTracker.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace AttendanceTracker.Controllers
{
    public class AccountController : BaseController
    {

        #region Views

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

        public ActionResult Transfer()
        {
            var list = GetUserTransfers();
            return View(list);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return View();
        }

        #endregion

        #region Verbs

        /// <summary>
        /// Creates an account
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="passwordVerify"></param>
        /// <returns></returns>
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
                password = passwordHash,
                Role = "None"
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
                Kiljaeden = false
            }).Wait();

            _client.PushAsync("transfers", new
            {
                User = name,
                Want = string.Empty,
                canPay = false,
                Comment = string.Empty
            });

            return Json("");
        }

        /// <summary>
        /// Log the user in and creates a session for the logged in user
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
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
            Session["UserName"] = account.Name;
            Session["Role"] = account.Role;
            return Json("");
        }

        /// <summary>
        /// Update the bosses for Tomb of Sargeras
        /// </summary>
        /// <param name="id"></param>
        /// <param name="goroth"></param>
        /// <param name="di"></param>
        /// <param name="harj"></param>
        /// <param name="sisters"></param>
        /// <param name="host"></param>
        /// <param name="mistress"></param>
        /// <param name="maiden"></param>
        /// <param name="fa"></param>
        /// <param name="kj"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateBosses(string id, string goroth, string di, string harj, string sisters, string host, string mistress, string maiden, string fa, string kj)
        {
            var list = GetUserBosses();
            var user = list.FirstOrDefault(x => x.Id.Equals(id));
            if (user == null)
            {
                return Json("ERROR: An error occurred");
            }

            var update = new BossesNeeded
            {
                User = user.User,
                Goroth = FromString(goroth),
                Di = FromString(di),
                Harjatan = FromString(harj),
                Sisters = FromString(sisters),
                Host = FromString(host),
                Mistress = FromString(mistress),
                Maiden = FromString(maiden),
                FallenAvatar = FromString(fa),
                Kiljaeden = FromString(kj)
            };

            _client.UpdateAsync($"bosses/-{id}", update).Wait();

            return Json("");
        }


        /// <summary>
        /// Update the bosses for Tomb of Sargeras
        /// </summary>
        /// <param name="id"></param>
        /// <param name="goroth"></param>
        /// <param name="di"></param>
        /// <param name="harj"></param>
        /// <param name="sisters"></param>
        /// <param name="host"></param>
        /// <param name="mistress"></param>
        /// <param name="maiden"></param>
        /// <param name="fa"></param>
        /// <param name="kj"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateTransfers(string id, string want, string canPay, string comment)
        {
            var list = GetUserBosses();
            var user = list.FirstOrDefault(x => x.Id.Equals(id));
            if (user == null)
            {
                return Json("ERROR: An error occurred");
            }

            if (string.IsNullOrEmpty(comment))
            {
                comment = string.Empty;
            }
            if(comment.Length > 50)
            {
                comment = comment.Substring(0, 50);
            }

            var update = new PlayerTransfers
            {
                User = user.User,
                Want = FromString(want).ToString(),
                CanPay = FromString(canPay),
                Comment = comment
            };

            _client.UpdateAsync($"transfers/-{id}", update).Wait();

            return Json("");
        }

        #endregion

    }
}